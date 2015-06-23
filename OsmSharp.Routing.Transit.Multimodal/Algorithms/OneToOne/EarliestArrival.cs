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
    /// An algorithm that calculates a one-to-one path between two locations in the routing graph, with a given departure time, that has the best arrival time.
    /// </summary>
    public class EarliestArrival : RoutingAlgorithmBase
    {
        private readonly MultimodalConnectionsDbBase<Edge> _db;
        private readonly ConnectionsView _connections;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = OsmSharp.Routing.Transit.Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly Func<float, float> _lazyness;
        private readonly OneToManyDykstra _sourceSearch;
        private readonly OneToManyDykstra _targetSearch;

        /// <summary>
        /// Holds the function to compare two stop statuses.
        /// </summary>
        private readonly Func<StopStatus, StopStatus, int> _compareStatuses = (status1, status2) =>
        {
            if (status1.Seconds == status2.Seconds)
            {
                return status1.Transfers.CompareTo(status2.Transfers);
            }
            return (status1.Seconds + status1.Lazyness).CompareTo(status2.Seconds + status2.Lazyness);
        };

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public EarliestArrival(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, OneToManyDykstra targetSearch)
            : this(db, departureTime, sourceSearch, targetSearch, (t) => { return 0; })
        {

        }

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public EarliestArrival(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
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
        public EarliestArrival(MultimodalConnectionsDbBase<Edge> db, DateTime departureTime,
            OneToManyDykstra sourceSearch, OneToManyDykstra targetSearch, Func<float, float> lazyness, 
            Func<int, DateTime, bool> isTripPossible, Func<StopStatus, StopStatus, int> compareStatuses)
        {
            _db = db;
            _connections = db.ConnectionsDb.GetDepartureTimeView();
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _lazyness = lazyness;

            _isTripPossible = isTripPossible;
            _compareStatuses = compareStatuses;
        }

        /// <summary>
        /// Holds all the statuses of all stops that have been touched by the forward search.
        /// </summary>
        private Dictionary<int, StopStatus> _forwardStopStatuses;

        /// <summary>
        /// Holds all the statuses of all stops that have been touched by the backward search.
        /// </summary>
        private Dictionary<int, StopStatus> _backwardStopStatuses;

        /// <summary>
        /// Holds the best stop to target.
        /// </summary>
        private int _bestTargetStop = -1;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // initialize visits.
            _forwardStopStatuses = new Dictionary<int, StopStatus>(1000);
            _backwardStopStatuses = new Dictionary<int, StopStatus>(100);

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

            // STEP3: use the calculate times at the stops from source/target and start to scan connections.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            _bestTargetStop = -1;
            var bestWeight = double.MaxValue;

            // initialize stops status.
            for (var connectionId = 0; connectionId < _connections.Count; connectionId++)
            { // scan all connections.
                var connection = _connections[connectionId];
                var departureTime = connection.DepartureTime + (day * OsmSharp.Routing.Transit.Constants.OneDayInSeconds);

                // check max search time.
                if (departureTime - startTime > _maximumSearchTime)
                { // maximum search time exceeded.
                    break; // stop searching without finding a result.
                }

                // check if target has been reached and if departure time exceeds target arrival time.
                if (_bestTargetStop > 0)
                { // there was already a best time.
                    if (departureTime >= bestWeight)
                    { // the current time to target is never going to be better, even with lazyness.
                        break;
                    }
                }

                StopStatus status;
                if (_forwardStopStatuses.TryGetValue(connection.DepartureStop, out status))
                { // stop was visited, has a status.
                    var transferTime = _minimumTransferTime;
                    var tripPossible = false;
                    var transfer = 1;
                    if (status.TripId == connection.TripId)
                    { // the same trip.
                        transferTime = 0;
                        tripPossible = true; // trip is possible because it is already used.
                        transfer = 0;
                    }

                    if (status.Seconds <= departureTime - transferTime)
                    { // a departure here is possible if the trip is possible.
                        if (!tripPossible && !tripPossibilities.TryGetValue(connection.TripId, out tripPossible))
                        { // trip was not checked yet.
                            tripPossible = _isTripPossible.Invoke(connection.TripId, date);
                            tripPossibilities.Add(connection.TripId, tripPossible);
                        }

                        if (tripPossible)
                        { // trip is possible.
                            // calculate status at the target stop if this trip is taken.
                            var arrivalStatus = new StopStatus()
                            {
                                TripId = connection.TripId,
                                ConnectionId = connectionId,
                                Transfers = (short)(status.Transfers + transfer),
                                Seconds = connection.ArrivalTime,
                                Lazyness = status.Lazyness
                            };

                            StopStatus existingStatus;
                            if (_forwardStopStatuses.TryGetValue(connection.ArrivalStop, out existingStatus))
                            { // compare statuses if already a status there.
                                if (_compareStatuses(existingStatus, arrivalStatus) > 0)
                                { // existingStatus > targetStatus here, replace existing status.
                                    _forwardStopStatuses[connection.ArrivalStop] = arrivalStatus;
                                }
                            }
                            else
                            { // no status yet, just set it.
                                _forwardStopStatuses.Add(connection.ArrivalStop, arrivalStatus);
                            }

                            // check target(s).
                            StopStatus backwardStatus;
                            if (_backwardStopStatuses.TryGetValue(connection.ArrivalStop, out backwardStatus))
                            { // this stop has been reached by the backward search, figure out if it represents a better connection.
                                var arrivalStopStatus = _forwardStopStatuses[connection.ArrivalStop];
                                var weight = backwardStatus.Seconds + arrivalStopStatus.Seconds +
                                    backwardStatus.Lazyness + arrivalStopStatus.Lazyness;
                                if (_bestTargetStop < 0 ||
                                    bestWeight > weight)
                                { // this current route is a better one.
                                    bestWeight = weight;
                                    _bestTargetStop = connection.ArrivalStop;
                                    this.HasSucceeded = true;
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
            if(_db.TryGetStop(vertex, out stopId))
            { // the vertex is a stop, mark it as reached.
                _forwardStopStatuses.Add(stopId, new StopStatus()
                {
                    ConnectionId = -1,
                    Seconds = (int)time + (int)(_departureTime - _departureTime.Date).TotalSeconds,
                    Lazyness = (int)_lazyness(time),
                    Transfers = 0,
                    TripId = -1
                });
            }
            return true;
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <param name="vertex">The vertex reached.</param>
        /// <param name="time">The time to reach it.</param>
        /// <returns></returns>
        private bool ReachedVertexBackward(uint vertex, float time)
        {
            int stopId;
            if (_db.TryGetStop(vertex, out stopId))
            { // the vertex is a stop, mark it as reached.
                _backwardStopStatuses.Add(stopId, new StopStatus()
                {
                    ConnectionId = -1,
                    Seconds = (int)time,
                    Lazyness = (int)_lazyness(time),
                    Transfers = 0,
                    TripId = -1
                });
            }
            return true;
        }

        /// <summary>
        /// Represents a stop status. 
        /// </summary>
        /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
        public struct StopStatus
        {
            /// <summary>
            /// Gets or sets the last trip id.
            /// </summary>
            public int TripId { get; set; }

            /// <summary>
            /// Gets or sets the connection id of the connection used.
            /// </summary>
            public int ConnectionId { get; set; }

            /// <summary>
            /// Gets or sets the # of transfers.
            /// </summary>
            public short Transfers { get; set; }

            /// <summary>
            /// Gets or sets the number of seconds from source.
            /// </summary>
            public int Seconds { get; set; }

            /// <summary>
            /// Gets or sets the lazyness factor penalty.
            /// </summary>
            /// <remarks>Use to force lazy pedestrians onto transit.</remarks>
            public int Lazyness { get; set; }
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
        /// Gets the calculated arrival time.
        /// </summary>
        /// <returns></returns>
        public DateTime ArrivalTime()
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(
                _forwardStopStatuses[_bestTargetStop].Seconds + _backwardStopStatuses[_bestTargetStop].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public int Duration()
        {
            this.CheckHasRunAndHasSucceeded();

            return _forwardStopStatuses[_bestTargetStop].Seconds + _backwardStopStatuses[_bestTargetStop].Seconds 
                - (int)(_departureTime - _departureTime.Date).TotalSeconds;
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
        /// Gets the status for the given stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public StopStatus GetStopStatus(int stopId)
        {
            this.CheckHasRunAndHasSucceeded();

            StopStatus status;
            if (!_forwardStopStatuses.TryGetValue(stopId, out status))
            { // status not found.
                return new StopStatus()
                {
                    ConnectionId = -1,
                    Seconds = -1,
                    Transfers = -1,
                    TripId = -1
                };
            }
            return status;
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
    }
}