﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two locations, with a given departure time, that has the best arrival time.
    /// </summary>
    public class OneToManyProfileSearch : RoutingAlgorithmBase, IConnectionList
    {
        private readonly MultimodalConnectionsDbBase<Edge> _db;
        private readonly ConnectionsView _connections;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = OsmSharp.Routing.Transit.Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly int _defaultTransferPentaly = 5 * 60;
        private readonly OneToManyDykstra _sourceSearch;
        private readonly List<OneToManyDykstra> _targetsSearch;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly Func<float, float> _lazyness;
        private readonly Func<Profile, Profile, int> _compareStatuses = (status1, status2) =>
        {
            if (status1.Seconds == status2.Seconds)
            {
                return status1.Transfers.CompareTo(status2.Transfers);
            }
            return status1.Seconds.CompareTo(status2.Seconds);
        };


        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public OneToManyProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, params OneToManyDykstra[] targetsSearch)
            : this(db, departureTime, sourceSearch, (t) => { return 0; }, targetsSearch)
        {

        }

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public OneToManyProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, Func<float, float> lazyness, params OneToManyDykstra[] targetsSearch)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetsSearch = new List<OneToManyDykstra>(targetsSearch);
            _lazyness = lazyness;

            _isTripPossible = db.ConnectionsDb.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public OneToManyProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, List<OneToManyDykstra> targetsSearch, Func<float, float> lazyness,
            Func<int, DateTime, bool> isTripPossible, Func<Profile, Profile, int> compareStatuses)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetsSearch = targetsSearch;
            _lazyness = lazyness;

            _isTripPossible = isTripPossible;
            _compareStatuses = compareStatuses;
        }

        // transit data management.
        private Dictionary<int, ProfileCollection> _forwardProfiles;
        private List<Dictionary<int, Profile>> _backwardProfiles;
        private List<int> _bestTargetStops;

        // bidirectional dykstra management.
        private List<uint> _bestVertices;
        private List<float> _bestWeights;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // initialize visits.
            _forwardProfiles = new Dictionary<int, ProfileCollection>();
            _backwardProfiles = new List<Dictionary<int,Profile>>(_targetsSearch.Count);
            _backwardProfiles.AddAll(() => new Dictionary<int, Profile>(), _targetsSearch.Count);
            _bestTargetStops = new List<int>(_targetsSearch.Count);
            _bestTargetStops.AddAll(-1, _targetsSearch.Count);
            _bestVertices = new List<uint>();
            _bestVertices.AddAll(uint.MaxValue, _targetsSearch.Count);
            _bestWeights = new List<float>();
            _bestWeights.AddAll(float.MaxValue, _targetsSearch.Count);

            // STEP1: calculate forward from source and keep track of all stops reached.
            _sourceSearch.WasFound = (vertex, time) =>
            {
                return this.ReachedVertexForward((uint)vertex, time);
            };
            _sourceSearch.Run();

            // STEP2: calculate backward from target and keep track of all stops reached.
            for (int targetIdx = 0; targetIdx < _targetsSearch.Count; targetIdx++)
            {
                _targetsSearch[targetIdx].WasFound = (vertex, time) =>
                {
                    return this.ReachedVertexBackward(targetIdx, (uint)vertex, time);
                };
                _targetsSearch[targetIdx].Run();
            }

            if ((_forwardProfiles.Count == 0 || !_backwardProfiles.Any(x => x.Count > 0)) &&
                !this.HasSucceeded)
            { // search failed because no forward or backward stops in range.
                return;
            }

            // Remarks:
            // - Use the number of seconds from the previous midnight, this is also what is used to sort the connections.
            // - Use the date to determine if a trip is possible.
            // - When the midnight barries is passed, increase the date.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            var tripPerRoute = new Dictionary<int, int>(100);

            // keep a list of possible target stops.
            var targetProfilesWeight = double.MaxValue;
            var targetProfilesTime = double.MaxValue;
            for (var connectionId = 0; connectionId < _connections.Count; connectionId++)
            { // scan all connections.
                var connection = _connections[connectionId];
                var departureTime = connection.DepartureTime + (day * Constants.OneDayInSeconds);

                // check max search time.
                if (departureTime - startTime > _maximumSearchTime)
                { // maximum search time exceeded.
                    break; // stop searching without finding a result.
                }

                // check if target has been reached and if departure time exceeds target arrival time.
                if (departureTime - targetProfilesTime >= (_defaultTransferPentaly * 5))
                { // the current status at 'to' is the best status it's ever going to get.
                    break;
                }

                // check if route was visited already.
                int routeTripId;
                if (connection.TripId != Constants.NoRouteId &&
                    tripPerRoute.TryGetValue(connection.TripId, out routeTripId) &&
                    routeTripId != connection.TripId)
                { // a different trip, but same route, do not consider again.
                    continue;
                }
                tripPerRoute[connection.RouteId] = connection.TripId;

                ProfileCollection departureProfiles;
                if (_forwardProfiles.TryGetValue(connection.DepartureStop, out departureProfiles))
                { // stop was visited, has a status.
                    var transferTime = _minimumTransferTime;
                    var tripPossible = false;
                    var transfer = 1;
                    var departureProfileTripId = Constants.NoConnectionId;
                    if (departureProfiles.Seconds > departureTime)
                    { // a departure here is not possible.
                        continue;
                    }
                    foreach (var departureProfile in departureProfiles)
                    {
                        transferTime = _minimumTransferTime;
                        tripPossible = false;
                        transfer = 1;
                        departureProfileTripId = Constants.NoConnectionId;
                        if (departureProfile.ConnectionId != Constants.NoConnectionId)
                        { // connection is set.
                            departureProfileTripId = _connections[departureProfile.ConnectionId].TripId;
                        }
                        if (departureProfileTripId == connection.TripId ||
                            (departureProfileTripId != Constants.PseudoConnectionTripId && connection.TripId == Constants.PseudoConnectionTripId) ||
                            (departureProfileTripId == Constants.PseudoConnectionTripId && connection.TripId != Constants.PseudoConnectionTripId))
                        { // the same trip.
                            transferTime = 0;
                            tripPossible = true; // trip is possible because it is already used.
                            transfer = 0;
                            if (connection.TripId == Constants.PseudoConnectionTripId)
                            { // consider this a transfer because the connection itself is a transfer.
                                transfer = 1;
                            }
                        }

                        if (departureProfile.Seconds <= departureTime - transferTime)
                        { // a departure here is possible if the trip is possible.
                            if (!tripPossibilities.TryGetValue(connection.TripId, out tripPossible))
                            { // trip was not checked yet.
                                tripPossible = _isTripPossible.Invoke(connection.TripId, date);
                                tripPossibilities.Add(connection.TripId, tripPossible);
                            }

                            if (tripPossible)
                            { // trip is possible.
                                // calculate status at the target stop if this trip is taken.
                                var arrivalProfile = new Profile()
                                {
                                    ConnectionId = connectionId,
                                    Transfers = (short)(departureProfile.Transfers + transfer),
                                    Seconds = connection.ArrivalTime,
                                    Lazyness = departureProfile.Lazyness,
                                    PreviousConnectionId = departureProfile.ConnectionId
                                };

                                ProfileCollection arrivalProfiles;
                                var accepted = false;
                                if (_forwardProfiles.TryGetValue(connection.ArrivalStop, out arrivalProfiles))
                                { // compare statuses if already a status there.
                                    accepted = arrivalProfiles.TryAdd(arrivalProfile);
                                }
                                else
                                { // no status yet, just set it.
                                    _forwardProfiles.Add(connection.ArrivalStop, new ProfileCollection(arrivalProfile));
                                    accepted = true;
                                }

                                // check target(s).
                                Profile backwardStatus;
                                for (var i = 0; i < _backwardProfiles.Count; i++)
                                {
                                    var backwardProfile = _backwardProfiles[i];
                                    if (accepted && backwardProfile.TryGetValue(connection.ArrivalStop, out backwardStatus))
                                    { // this stop has been reached by the backward search, figure out if it represents a better connection.
                                        var arrivalStopProfiles = _forwardProfiles[connection.ArrivalStop];
                                        var weight = backwardStatus.Seconds + arrivalStopProfiles.Seconds +
                                            backwardStatus.Lazyness + arrivalStopProfiles.Lazyness;
                                        if (_bestTargetStops[i] < 0 || targetProfilesWeight >= weight)
                                        { // this current route is a better one.
                                            _bestTargetStops[i] = connection.ArrivalStop;
                                            targetProfilesWeight = weight;
                                            targetProfilesTime = backwardStatus.Seconds + arrivalStopProfiles.Seconds;
                                            this.HasSucceeded = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when a vertex was reached during a forward search.
        /// </summary>
        /// <param name="vertex">The vertex reached.</param>
        /// <param name="time">The time to reach it.</param>
        /// <returns></returns>
        private bool ReachedVertexForward(uint vertex, float time)
        {
            int stopId;
            if (_db.TryGetStop(vertex, out stopId) &&
                !_forwardProfiles.ContainsKey(stopId))
            { // the vertex is a stop, mark it as reached.
                _forwardProfiles.Add(stopId, new ProfileCollection(new Profile()
                {
                    ConnectionId = Constants.NoConnectionId,
                    Lazyness = (int)_lazyness(time),
                    Seconds = (int)time + (int)(_departureTime - _departureTime.Date).TotalSeconds,
                    Transfers = 0,
                    PreviousConnectionId = Constants.NoConnectionId
                }));
            }

            // only use forward visits when non-bidirectional routing.
            for (int i = 0; i < _targetsSearch.Count; i++)
            {
                var targetSearch = _targetsSearch[i];
                if (targetSearch.Source.Contains(vertex))
                {
                    var pathTo = targetSearch.Source.GetPathTo(vertex);
                    if (pathTo.Weight + time < _bestWeights[i])
                    { // best vertex was found.
                        _bestWeights[i] = (float)pathTo.Weight + time;
                        _bestVertices[i] = vertex;
                        this.HasSucceeded = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedVertexBackward(int targetIdx, uint vertex, float weight)
        {
            var backwardprofile = _backwardProfiles[targetIdx];
            int stopId;
            if (_db.TryGetStop(vertex, out stopId) &&
                !backwardprofile.ContainsKey(stopId))
            { // the vertex is a stop, mark it as reached.
                backwardprofile.Add(stopId, new Profile()
                {
                    ConnectionId = Constants.NoConnectionId,
                    Seconds = (int)weight,
                    Lazyness = (int)_lazyness(weight),
                    Transfers = 0,
                    PreviousConnectionId = Constants.NoConnectionId
                });
            }

            // check forward search for the same vertex.
            // only use backward visits when bidirectional routing.
            DykstraVisit forwardVisit;
            if (_sourceSearch.TryGetVisit(vertex, out forwardVisit))
            { // there is a status for this vertex in the source search.
                if (_targetsSearch[targetIdx].Vehicle.UniqueName.Equals(
                   _sourceSearch.Vehicle))
                { // 
                    weight = weight + forwardVisit.Weight;
                    if (weight < _bestWeights[targetIdx])
                    { // this vertex is a better match.
                        _bestWeights[targetIdx] = weight;
                        _bestVertices[targetIdx] = vertex;
                        this.HasSucceeded = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the calculated arrival time.
        /// </summary>
        /// <returns></returns>
        public DateTime ArrivalTime(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(
                _forwardProfiles[_bestTargetStops[i]].Seconds + _backwardProfiles[i][_bestTargetStops[i]].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public int Duration(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            return _forwardProfiles[_bestTargetStops[i]].Seconds + _backwardProfiles[i][_bestTargetStops[i]].Seconds
                - (int)(_departureTime - _departureTime.Date).TotalSeconds;
        }

        /// <summary>
        /// Returns true if the best route route has transit, false otherwise.
        /// </summary>
        public bool GetHasTransit(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            if (_bestWeights[i] == float.MaxValue)
            {
                return (_bestTargetStops[i] != -1);
            }
            if (_bestTargetStops[i] != -1)
            {
                var transitTime = _forwardProfiles[_bestTargetStops[i]].Seconds + _backwardProfiles[i][_bestTargetStops[i]].Seconds
                    - (int)(_departureTime - _departureTime.Date).TotalSeconds;
                return transitTime < (int)_bestWeights[i];
            }
            return false;
        }

        /// <summary>
        /// Returns true if a route was found to the given target.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetHasRoute(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            return (_bestWeights[i] != float.MaxValue ||
                _bestTargetStops[i] != -1);
        }

        /// <summary>
        /// Gets the status for the given stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public IReadOnlyList<Profile> GetStopProfiles(int stopId)
        {
            this.CheckHasRunAndHasSucceeded();

            ProfileCollection profiles;
            if (!_forwardProfiles.TryGetValue(stopId, out profiles))
            { // status not found.
                return new ProfileCollection();
            }
            return profiles;
        }

        /// <summary>
        /// Gets the connection with the given id.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Connection GetConnection(int connectionId)
        {
            return _connections[connectionId];
        }

        /// <summary>
        /// Gets the best target stop.
        /// </summary>
        /// <returns></returns>
        public int GetBestTargetStop(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            return _bestTargetStops[i];
        }

        /// <summary>
        /// Gets the best non-transit vertex.
        /// </summary>
        public uint GetBestNonTransitVertex(int i)
        {
            this.CheckHasRunAndHasSucceeded();

            return _bestVertices[i];
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public OneToManyDykstra SourceSearch
        {
            get
            {
                return _sourceSearch;
            }
        }

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public OneToManyDykstra GetTargetSearch(int i)
        {
                return _targetsSearch[i];
        }

        /// <summary>
        /// Gets the departure time.
        /// </summary>
        public DateTime DepartureTime
        {
            get
            {
                return _departureTime;
            }
        }

        /// <summary>
        /// A profile collection that just points to the first and last profile of a linked-list.
        /// </summary>
        private class ProfileCollection : List<Profile>
        {
            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection()
            {

            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection(Profile profile)
                : base(1)
            {
                this.Add(profile);

                this.Seconds = profile.Seconds;
                this.Lazyness = profile.Lazyness;
            }

            /// <summary>
            /// Gets or sets the time in seconds.
            /// </summary>
            public int Seconds { get; private set; }

            /// <summary>
            /// Gets or sets the lazyness.
            /// </summary>
            public int Lazyness { get; private set; }

            /// <summary>
            /// Tries to add a profile and removes all entries that are dominated.
            /// </summary>
            /// <param name="profile"></param>
            /// <returns></returns>
            public bool TryAdd(Profile profile)
            {
                var i = this.Count - 1;
                while (i >= 0)
                {
                    if (profile.ConnectionId != this[i].ConnectionId)
                    { // no use comparing profiles on different connections.
                        break;
                    }
                    var domination = this.Dominates(this[i], profile);
                    if (domination.HasValue)
                    {
                        if (domination.Value)
                        { // profile is dominated, do not add.
                            return false;
                        }
                        else
                        { // remove newly dominated profile.
                            this.RemoveAt(i);
                        }
                    }
                    i--;
                }

                // when we get to this point no domination of profile.
                this.Add(profile);
                if (this.Lazyness + this.Seconds > profile.Seconds + profile.Lazyness)
                {
                    this.Seconds = profile.Seconds;
                    this.Lazyness = profile.Lazyness;
                }
                return true;
            }

            /// <summary>
            /// Figures out if, of the two given profiles, one dominates the other.
            /// </summary>
            /// <returns>null if there is no domination, true if profile1 dominates profile2 and false if profile2 dominates profile1.</returns>
            private bool? Dominates(Profile profile1, Profile profile2)
            {
                if (profile1.Transfers != profile2.Transfers)
                { // transfer count is different, one will definetly dominate.
                    return profile1.Transfers.CompareTo(profile2.Transfers) < 0;
                }
                else if (profile1.Lazyness != profile2.Lazyness)
                {
                    return profile1.Lazyness.CompareTo(profile2.Lazyness) < 0;
                }
                else if (profile1.ConnectionId == profile2.ConnectionId)
                { // identical.
                    return true;
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Represents a stop status. 
    /// </summary>
    /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
    public struct Profile
    {
        /// <summary>
        /// Gets or sets the connection id of the connection used.
        /// </summary>
        public int ConnectionId { get; set; }

        /// <summary>
        /// The connection id before the connection we're coming from.
        /// </summary>
        public int PreviousConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the # of transfers.
        /// </summary>
        public short Transfers { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds from source.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Gets or sets the lazyness.
        /// </summary>
        public int Lazyness { get; set; }

        /// <summary>
        /// Returns a description of this status.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{3} -> {0} #{1} ({2})", this.Seconds, this.Transfers, this.Lazyness,
                this.ConnectionId);
        }
    }

    /// <summary>
    /// Extension methods for profile collections.
    /// </summary>
    public static class ProfileCollectionExtensions
    {
        /// <summary>
        /// Gets one of the best profiles.
        /// </summary>
        /// <returns></returns>
        public static Profile GetBest(this IReadOnlyList<Profile> profileCollection)
        {
            Profile? found = null;
            foreach (var profile in profileCollection)
            {
                if (found == null)
                {
                    found = profile;
                }
                else if (found.Value.Seconds + found.Value.Lazyness >
                    profile.Seconds + profile.Lazyness)
                {
                    found = profile;
                }
                else if (found.Value.Seconds + found.Value.Lazyness ==
                    profile.Seconds + profile.Lazyness &&
                    found.Value.Transfers > profile.Transfers)
                {
                    found = profile;
                }
            }
            return found.Value;
        }

        /// <summary>
        /// Gets the best profile given the previous profile.
        /// </summary>
        /// <returns></returns>
        public static Profile GetBest(this IReadOnlyList<Profile> profileCollection, Profile previousProfile)
        {
            foreach (var profile in profileCollection)
            {
                if (profile.ConnectionId == previousProfile.PreviousConnectionId)
                {
                    return profile;
                }
            }
            throw new Exception("Connection not found but profile points to connection as previous.");
        }
    }
}