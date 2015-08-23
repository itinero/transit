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
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Transit.Multimodal.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two locations, with a given departure time, that has the best arrival time.
    /// </summary>
    public class ProfileSearch : RoutingAlgorithmBase, IConnectionList
    {
        private readonly MultimodalConnectionsDbBase<Edge> _db;
        private readonly ConnectionsView _connections;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = OsmSharp.Routing.Transit.Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly int _defaultTransferPentaly = 5 * 60;
        private readonly OneToManyDykstra _sourceSearch;
        private readonly OneToManyDykstra _targetSearch;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly Func<float, float> _lazyness;
        

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public ProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, OneToManyDykstra targetSearch)
            : this(db, departureTime, sourceSearch, targetSearch, (t) => { return 0; })
        {

        }

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public ProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, OneToManyDykstra targetSearch, Func<float, float> lazyness)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _lazyness = lazyness;

            _isTripPossible = db.ConnectionsDb.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public ProfileSearch(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, OneToManyDykstra targetSearch, Func<float, float> lazyness,
            Func<int, DateTime, bool> isTripPossible, Func<Profile, Profile, int> compareStatuses)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _lazyness = lazyness;

            _isTripPossible = isTripPossible;
        }

        // transit data management.
        private Dictionary<int, Profile> _backwardProfiles;
        private Dictionary<int, ProfileCollection> _forwardProfiles;
        private int _bestTargetStop;
        private Dictionary<int, TripStatus> _tripStatuses;

        // bidirectional dykstra management.
        private uint _bestVertex = uint.MaxValue;
        private float _bestWeight = float.MaxValue;
        private bool _bidirectional = false;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // initialize visits.
            _forwardProfiles = new Dictionary<int, ProfileCollection>();
            _backwardProfiles = new Dictionary<int, Profile>(100);
            _bestTargetStop = -1;
            _bestVertex = uint.MaxValue;
            _bestWeight = float.MaxValue;
            _bidirectional = _sourceSearch.Vehicle.UniqueName.Equals(_targetSearch.Vehicle.UniqueName);
            _tripStatuses = new Dictionary<int, TripStatus>();

            // STEP1: calculate forward from source and keep track of all stops reached.
            _sourceSearch.WasFound = (vertex, time) =>
            {
                return this.ReachedVertexForward((uint)vertex, time);
            };
            _sourceSearch.Run();

            // STEP2: calculate backward from target and keep track of all stops reached.
            _targetSearch.WasFound = (vertex, time) =>
            {
                return this.ReachedVertexBackward((uint)vertex, time);
            };
            _targetSearch.Run();

            if((_forwardProfiles.Count == 0 || _backwardProfiles.Count == 0) &&
                _bestWeight == float.MaxValue)
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
                if(connection.TripId != Constants.NoRouteId &&
                    tripPerRoute.TryGetValue(connection.TripId, out routeTripId) &&
                    routeTripId != connection.TripId)
                { // a different trip, but same route, do not consider again.
                    continue;
                }
                tripPerRoute[connection.RouteId] = connection.TripId;

                ProfileCollection departureProfiles;
                if (_forwardProfiles.TryGetValue(connection.DepartureStop, out departureProfiles))
                { // stop was visited, has a status.// first time on this trip.
                    var tripFound = false;
                    var tripStatus = new TripStatus();
                    if(connection.TripId != Constants.PseudoConnectionTripId && 
                        _tripStatuses.TryGetValue(connection.TripId, out tripStatus))
                    { // trip was found.
                        tripFound = true;
                    }

                    // latest arrival time in case of a transfer.
                    var latestArrivalTime = connection.DepartureTime - _minimumTransferTime;

                    // build new profile.
                    ProfileCollection arrivalProfiles = null;
                    if(!_forwardProfiles.TryGetValue(connection.ArrivalStop, out arrivalProfiles))
                    { // create new empty arrival profiles.
                        arrivalProfiles = new ProfileCollection();
                    }

                    if (tripFound)
                    { // if trip was found, there is only one good option.
                        arrivalProfiles.UpdateStatus(tripStatus.Transfers, new Profile()
                        {
                            PreviousConnectionId = connectionId,
                            Seconds = connection.ArrivalTime
                        });

                        // check if now this trip was reached with less transfers at this stop.
                        for (var i = 0; i < tripStatus.Transfers - 2 &&  i < departureProfiles.Count; i++)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.Seconds == Constants.NoSeconds)
                            { // no source at this transfer count.
                                continue;
                            }

                            // check if connection is reachable.
                            if (sourceProfile.Seconds > latestArrivalTime)
                            { // source arrives too late for this connection, all other source have higher or equal arrival times.
                                continue;
                            }

                            // ok here, this should lead to one less transfer.
                            _tripStatuses[connection.TripId] = new TripStatus()
                            {
                                Transfers = i + 1,
                                StopId = connection.DepartureStop
                            };
                            break;
                        }
                    }
                    else
                    { // if trip was not found have a look and see if we can tranfer to this connection.
                        var tripTransfers = int.MaxValue;
                        for (var i = departureProfiles.Count - 1; i >= 0; i--)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.Seconds == Constants.NoSeconds)
                            { // no source at this transfer count.
                                continue;
                            }

                            // check if connection is reachable.
                            var transfer = 1;
                            if (sourceProfile.Seconds > latestArrivalTime)
                            { // source arrives too late for this connection, all other source have higher or equal arrival times.
                                if(connection.PreviousConnectionId != Constants.PseudoConnectionTripId)
                                { // connection is a regular trip.
                                    continue;
                                }
                                if (sourceProfile.Seconds > connection.DepartureStop)
                                { // connection is a pseudo connection, but arrival is too late.
                                    break;
                                }
                                transfer = 0; // going to a pseudo connection is not a transfer.
                            }
                            else
                            {
                                if(connection.PreviousConnectionId == Constants.PseudoConnectionTripId)
                                { // going to a pseudo connection is not a transfer.
                                    transfer = 0;
                                }
                            }

                            // ok, there is an actual move possible here.
                            arrivalProfiles.UpdateStatus(i + transfer, new Profile()
                                {
                                    PreviousConnectionId = connectionId,
                                    Seconds = connection.ArrivalTime
                                });
                            if (i + transfer < tripTransfers)
                            { // keep the lowest transfer count for this trip.
                                tripTransfers = i + transfer;
                            }
                        }

                        if (tripTransfers < int.MaxValue &&
                            connection.TripId != Constants.PseudoConnectionTripId)
                        { // trip was not found, but was reached.
                            _tripStatuses[connection.TripId] = new TripStatus()
                            {
                                StopId = connection.DepartureStop,
                                Transfers = tripTransfers
                            };
                        }
                    }

                    if(arrivalProfiles.Count > 0)
                    { // make sure that the arrival profiles are set.
                        _forwardProfiles[connection.ArrivalStop] = arrivalProfiles;

                        // check target(s).
                        Profile backwardStatus;
                        if (_backwardProfiles.TryGetValue(connection.ArrivalStop, out backwardStatus))
                        { // this stop has been reached by the backward search, figure out if it represents a better connection.
                            var arrivalStopProfiles = _forwardProfiles[connection.ArrivalStop];
                            var weight = backwardStatus.Seconds + arrivalStopProfiles.Seconds;// +
                                // backwardStatus.Lazyness + arrivalStopProfiles.Lazyness;
                            if (_bestTargetStop < 0 || targetProfilesWeight >= weight)
                            { // this current route is a better one.
                                _bestTargetStop = connection.ArrivalStop;
                                targetProfilesWeight = weight;
                                targetProfilesTime = backwardStatus.Seconds + arrivalStopProfiles.Seconds;
                                this.HasSucceeded = true;
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
                    PreviousConnectionId = Constants.NoConnectionId,
                    //Lazyness = (int)_lazyness(time),
                    Seconds = (int)time + (int)(_departureTime - _departureTime.Date).TotalSeconds
                    //Transfers = 0,
                    //PreviousConnectionId = Constants.NoConnectionId
                }));
            }

            if (!_bidirectional)
            { // only use forward visits when non-bidirectional routing.
                if(_targetSearch.Source.Contains(vertex))
                {
                    var pathTo = _targetSearch.Source.GetPathTo(vertex);
                    if(pathTo.Weight + time < _bestWeight)
                    { // best vertex was found.
                        _bestWeight = (float)pathTo.Weight + time;
                        _bestVertex = vertex;
                        this.HasSucceeded = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <param name="vertex">The vertex reached.</param>
        /// <param name="weight">The time to reach it.</param>
        /// <returns></returns>
        private bool ReachedVertexBackward(uint vertex, float weight)
        {
            int stopId;
            if (_db.TryGetStop(vertex, out stopId) &&
                !_backwardProfiles.ContainsKey(stopId))
            { // the vertex is a stop, mark it as reached.
                _backwardProfiles.Add(stopId, new Profile()
                {
                    PreviousConnectionId = Constants.NoConnectionId,
                    Seconds = (int)weight //,
                    //Lazyness = (int)_lazyness(weight),
                    //Transfers = 0,
                    //PreviousConnectionId = Constants.NoConnectionId
                });
            }

            // check forward search for the same vertex.
            if (_bidirectional)
            { // only use backward visits when bidirectional routing.
                DykstraVisit forwardVisit;
                if (_sourceSearch.TryGetVisit(vertex, out forwardVisit))
                { // there is a status for this vertex in the source search.
                    weight = weight + forwardVisit.Weight;
                    if (weight < _bestWeight)
                    { // this vertex is a better match.
                        _bestWeight = weight;
                        _bestVertex = vertex;
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
        public DateTime ArrivalTime()
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(
                _forwardProfiles[_bestTargetStop].Seconds + _backwardProfiles[_bestTargetStop].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public int Duration()
        {
            this.CheckHasRunAndHasSucceeded();

            return _forwardProfiles[_bestTargetStop].Seconds + _backwardProfiles[_bestTargetStop].Seconds
                - (int)(_departureTime - _departureTime.Date).TotalSeconds;
        }

        /// <summary>
        /// Returns true if the best route route has transit, false otherwise.
        /// </summary>
        public bool HasTransit
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                if(_bestWeight == float.MaxValue)
                {
                    return true;
                }
                if (_bestTargetStop != -1)
                {
                    var transitTime = _forwardProfiles[_bestTargetStop].Seconds + _backwardProfiles[_bestTargetStop].Seconds
                        - (int)(_departureTime - _departureTime.Date).TotalSeconds;
                    return transitTime < (int)_bestWeight;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the status for the given stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public ProfileCollection GetStopProfiles(int stopId)
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
        public int GetBestTargetStop()
        {
            this.CheckHasRunAndHasSucceeded();

            return _bestTargetStop;
        }

        /// <summary>
        /// Gets the best non-transit vertex.
        /// </summary>
        public uint GetBestNonTransitVertex()
        {
            this.CheckHasRunAndHasSucceeded();

            return _bestVertex;
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
        public OneToManyDykstra TargetSearch
        {
            get
            {
                return _targetSearch;
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
        /// A collections of profiles indexed per #transfers.
        /// </summary>
        public class ProfileCollection : List<Profile>
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
            {
                this.UpdateStatus(0, profile);
            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection(int transfers, Profile profile)
            {
                this.UpdateStatus(transfers, profile);
            }

            /// <summary>
            /// Updates a profile for the given number of transfers.
            /// </summary>
            public bool UpdateStatus(int transfers, Profile profile)
            {
                if(this.Count > 0)
                { // check if dominated by latest entry.
                    if(this.Count - 1 <= transfers &&
                        this[this.Count - 1].Seconds <= profile.Seconds)
                    { // dominated by latest, do nothing.
                        return false;
                    }
                }
                if (this.Count - 1 < transfers)
                { // no profile yet at this tranfer, just update list and insert.
                    do
                    {
                        this.Add(Profile.Empty);
                    } while (this.Count - 1 < transfers);
                    this[transfers] = profile;
                    return true;
                }
                else
                { // yes, there is a profile, compare it and remove dominated entries if needed.
                    for(var i = this.Count - 1; i > transfers; i--)
                    {
                        if(this[i].PreviousConnectionId != Constants.NoConnectionId &&
                            this[i].Seconds >= profile.Seconds)
                        {
                            if (i == this.Count - 1)
                            { // remove last if it would be set to empty.
                                this.RemoveAt(i);
                            }
                            else
                            { // ... or empty out if not the last entry.
                                this[i] = Profile.Empty;
                            }
                        }
                    }

                    if(this[transfers].PreviousConnectionId == Constants.NoConnectionId)
                    {
                        this[transfers] = profile;
                        return true;
                    }
                    else if(this[transfers].Seconds > profile.Seconds)
                    {
                        this[transfers] = profile;
                        return true;
                    }
                }
                return false;
            }

            public int Seconds
            {
                get
                {
                    return this[this.Count - 1].Seconds;
                }
            }
        }

        /// <summary>
        /// Returns the status for the given trip.
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        public TripStatus GetTripStatus(int tripId)
        {
            TripStatus status;
            if(!_tripStatuses.TryGetValue(tripId, out status))
            {
                status = new TripStatus()
                {
                    StopId = Constants.NoStopId,
                    Transfers = Constants.NoTransfers
                };
            }
            return status;
        }

        /// <summary>
        /// Returns the minimum transfer time.
        /// </summary>
        public int MinimumTransferTime
        {
            get
            {
                return _minimumTransferTime;
            }
        }
    }

    /// <summary>
    /// Represents a trip status.
    /// 
    /// Keep the first stop possible to reach this trip.
    /// Keep the #transfers to reach this trip.
    /// </summary>
    public struct TripStatus
    {
        public int StopId { get; set; }

        public override string ToString()
        {
            return string.Format("@{0}", this.StopId);
        }

        public int Transfers { get; set; }
    }

    /// <summary>
    /// Represents a stop status. 
    /// </summary>
    /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
    public struct Profile
    {
        public int PreviousConnectionId { get; set; }

        public int Seconds { get; set; }

        public override string ToString()
        {
            return string.Format("{0}@{1}", this.PreviousConnectionId, this.Seconds);
        }

        public static Profile Empty = new Profile()
        {
            PreviousConnectionId = Constants.NoConnectionId,
            Seconds = Constants.NoSeconds
        };
    }

    ///// <summary>
    ///// Extension methods for profile collections.
    ///// </summary>
    //public static class ProfileCollectionExtensions
    //{
    //    /// <summary>
    //    /// Gets one of the best profiles.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static Profile GetBest(this IReadOnlyList<Profile> profileCollection)
    //    {
    //        Profile? found = null;
    //        foreach (var profile in profileCollection)
    //        {
    //            if (found == null)
    //            {
    //                found = profile;
    //            }
    //            else if (found.Value.Seconds + found.Value.Lazyness > 
    //                profile.Seconds + profile.Lazyness)
    //            {
    //                found = profile;
    //            }
    //            else if (found.Value.Seconds + found.Value.Lazyness ==
    //                profile.Seconds + profile.Lazyness &&
    //                found.Value.Transfers > profile.Transfers)
    //            {
    //                found = profile;
    //            }
    //        }
    //        return found.Value;
    //    }

    //    /// <summary>
    //    /// Gets the best profile given the previous profile.
    //    /// </summary>
    //    /// <returns></returns>
    //    public static Profile GetBest(this IReadOnlyList<Profile> profileCollection, Profile previousProfile)
    //    {
    //        foreach (var profile in profileCollection)
    //        {
    //            if(profile.ConnectionId == previousProfile.PreviousConnectionId)
    //            {
    //                return profile;
    //            }
    //        }
    //        throw new Exception("Connection not found but profile points to connection as previous.");
    //    }
    //}
}