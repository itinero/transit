// OsmSharp - OpenStreetMap (OSM) SDK
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
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Transit.Builders;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToAll
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two locations, with a given departure time, that has the best arrival time.
    /// </summary>
    public class OneToAllProfileSearch : RoutingAlgorithmBase, IConnectionList, IHeatmapSource
    {
        private readonly MultimodalConnectionsDbBase<Edge> _db;
        private readonly ConnectionsView _connections;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly int _defaultTransferPentaly = 5 * 60;
        private readonly OneToManyDykstra _sourceSearch;
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
        public OneToAllProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, int max)
            : this(db, departureTime, sourceSearch, max, (t) => { return 0; })
        {

        }

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public OneToAllProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, int max, Func<float, float> lazyness)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _lazyness = lazyness;
            _maximumSearchTime = max;

            _isTripPossible = db.ConnectionsDb.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public OneToAllProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, int max, Func<float, float> lazyness,
            Func<int, DateTime, bool> isTripPossible, Func<Profile, Profile, int> compareStatuses)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _lazyness = lazyness;
            _maximumSearchTime = max;

            _isTripPossible = isTripPossible;
            _compareStatuses = compareStatuses;
        }

        // transit data management.
        private Dictionary<int, ProfileCollection> _forwardProfiles;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // ... this always succeeds, no real targets.
            this.HasSucceeded = true;

            // initialize visits.
            _forwardProfiles = new Dictionary<int, ProfileCollection>();

            // search source.
            _sourceSearch.WasFound = (vertex, time) =>
            {
                return this.ReachedVertexForward((uint)vertex, time);
            };
            _sourceSearch.Initialize();

            // Remarks:
            // - Use the number of seconds from the previous midnight, this is also what is used to sort the connections.
            // - Use the date to determine if a trip is possible.
            // - When the midnight barrier is passed, increase the date.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            var tripPerRoute = new Dictionary<int, int>(100);

            // keep a list of possible target stops.
            var targetProfilesTime = double.MaxValue;
            for (var connectionId = 0; connectionId < _connections.Count; connectionId++)
            { // scan all connections.
                var connection = _connections[connectionId];
                var departureTime = connection.DepartureTime + (day * Constants.OneDayInSeconds);

                if(departureTime - startTime < 0)
                { // departure time before startime, ignore connection.
                    continue;
                }

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

                // ok, now a first valid connection.
                float dykstraWeight = startTime;
                if(_sourceSearch.Current != null)
                { // get proper weight from dykstra current.
                    dykstraWeight = _sourceSearch.Current.Weight;
                }
                while(dykstraWeight < departureTime && 
                    _sourceSearch.Step())
                {
                    dykstraWeight = _sourceSearch.Current.Weight;
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
                            (departureProfileTripId != Constants.PseudoConnectionTripId && 
                                connection.TripId == Constants.PseudoConnectionTripId) ||
                            (departureProfileTripId == Constants.PseudoConnectionTripId && 
                                connection.TripId != Constants.PseudoConnectionTripId))
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

                                // set the dykstra status.
                                uint arrivalStopVertex;
                                if(_db.TryGetVertex(connection.ArrivalStop, out arrivalStopVertex))
                                { // set a status with no previous, no edge, and weight equal to the arrivaltime relative to the starttime.
                                    _sourceSearch.SetVisit(new DykstraVisit(arrivalStopVertex, -1, connection.ArrivalTime - startTime,
                                        default(Edge), null));
                                }
                            }
                        }
                    }
                }
            }

            // keep search when max is not yet reached.
            if(_sourceSearch.Current != null &&
                _sourceSearch.Current.Weight < _maximumSearchTime)
            {
                _sourceSearch.WasFound = (x, y) => true;
                while (_sourceSearch.Step() &&
                    _sourceSearch.Current.Weight < _maximumSearchTime)
                {
                    // search, search, search...
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

            // be a good heatmapsource here.
            if(((IHeatmapSource)this).ReportSampleAction != null)
            {
                float lat, lon;
                if(_db.Graph.GetVertex(vertex, out lat, out lon))
                {
                    ((IHeatmapSource)this).ReportSampleAction(lat, lon, time);
                }
            }

            return true;
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

        /// <summary>
        /// Gets or sets the report sample action.
        /// </summary>
        Action<float, float, float> IHeatmapSource.ReportSampleAction
        {
            get;
            set;
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