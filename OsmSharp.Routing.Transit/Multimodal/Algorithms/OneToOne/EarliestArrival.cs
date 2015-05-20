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
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Transit.Algorithms.OneToOne;
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
    public class EarliestArrival : OneToOneRoutingAlgorithmBase
    {
        /// <summary>
        /// Holds the multimodal connections db.
        /// </summary>
        private readonly MultimodalConnectionsDbBase<LiveEdge> _db;

        /// <summary>
        /// Holds the connections view.
        /// </summary>
        private readonly ConnectionsView _connections;

        /// <summary>
        /// Holds the routing graph.
        /// </summary>
        private readonly DynamicGraphRouterDataSource<LiveEdge> _graph;

        /// <summary>
        /// Hold the routing intepreter.
        /// </summary>
        private readonly IRoutingInterpreter _interpreter;

        /// <summary>
        /// Holds the source vehicle.
        /// </summary>
        private readonly Vehicle _sourceVehicle;

        /// <summary>
        /// Holds the source location.
        /// </summary>
        private readonly PathSegmentVisitList _source;

        /// <summary>
        /// Holds the maximum seconds for the source vehicle.
        /// </summary>
        private readonly int _sourceMax;

        /// <summary>
        /// Holds the target vehicle.
        /// </summary>
        private readonly Vehicle _targetVehicle;

        /// <summary>
        /// Holds the target location.
        /// </summary>
        private readonly PathSegmentVisitList _target;

        /// <summary>
        /// Holds the maximum seconds for the target vehicle.
        /// </summary>
        private readonly int _targetMax;

        /// <summary>
        /// Holds the departure time.
        /// </summary>
        private readonly DateTime _departureTime;

        /// <summary>
        /// Holds the maximum time in seconds to search from the given departure time before failing.
        /// </summary>
        private readonly int _maximumSearchTime = Constants.OneDayInSeconds;

        /// <summary>
        /// Holds the minimum time it takes to tranfer from one trip to another at the same stop.
        /// </summary>
        private readonly int _minimumTransferTime = 3 * 60;

        /// <summary>
        /// Holds the function to determine if a trip is possible or not on a given date.
        /// </summary>
        private readonly Func<int, DateTime, bool> _isTripPossible;

        /// <summary>
        /// Holds the function to compare two stop statuses.
        /// </summary>
        private readonly Func<StopStatus, StopStatus, int> _compareStatuses = (status1, status2) =>
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
        /// <param name="db">The connections db.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="sourceVehicle">The vehicle at the source.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceMax">The maximum seconds for the source vehicle to travel.</param>
        /// <param name="targetVehicle">The vehicle at the target.</param>
        /// <param name="target">The target location.</param>
        /// <param name="targetMax">The maximum seconds for the target vehicle to travel.</param>
        /// <param name="departureTime">The departure time.</param>
        public EarliestArrival(MultimodalConnectionsDbBase<LiveEdge> db, IRoutingInterpreter interpreter, Vehicle sourceVehicle, PathSegmentVisitList source, int sourceMax,
            Vehicle targetVehicle, PathSegmentVisitList target, int targetMax, DateTime departureTime)
        {
            _db = db;
            _connections = db.GetDepartureTimeView();
            _graph = db.Graph;
            _interpreter = interpreter;
            _source = source;
            _sourceVehicle = sourceVehicle;
            _sourceMax = sourceMax;
            _target = target;
            _targetVehicle = targetVehicle;
            _targetMax = targetMax;
            _departureTime = departureTime;

            _isTripPossible = db.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        /// <param name="db">The connections db.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="sourceVehicle">The vehicle at the source.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceMax">The maximum seconds for the source vehicle to travel.</param>
        /// <param name="targetVehicle">The vehicle at the target.</param>
        /// <param name="target">The target location.</param>
        /// <param name="targetMax">The maximum seconds for the target vehicle to travel.</param>
        /// <param name="departureTime">The departure time.</param>
        /// <param name="minimumTransferTime">The minimum transfer time (default: 3 * 60s).</param>
        /// <param name="maxmumSearchTime">The maximum search time (default: one day in seconds).</param>
        /// <param name="isTripPossible">The function to check if a trip is possible (default: connections.IsTripPossible).</param>
        /// <param name="compareStatuses">The function to compare a status at a stop of two distinct statuses are found at one stop (default: the lowest seconds, than the lowest transfer count).</param>
        public EarliestArrival(MultimodalConnectionsDb db, IRoutingInterpreter interpreter, Vehicle sourceVehicle, PathSegmentVisitList source, int sourceMax,
            Vehicle targetVehicle, PathSegmentVisitList target, int targetMax, DateTime departureTime, int minimumTransferTime, 
            int maxmumSearchTime, Func<int, DateTime, bool> isTripPossible, Func<StopStatus, StopStatus, int> compareStatuses)
        {
            _db = db;
            _connections = db.GetDepartureTimeView();
            _graph = db.Graph;
            _interpreter = interpreter;
            _source = source;
            _sourceVehicle = sourceVehicle;
            _sourceMax = sourceMax;
            _target = target;
            _targetVehicle = targetVehicle;
            _targetMax = targetMax;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
            _minimumTransferTime = minimumTransferTime;
            _maximumSearchTime = maxmumSearchTime;
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
            var oneToManyForward = new OneToManyDykstra(_graph, _interpreter, _sourceVehicle, _source, _sourceMax, false, (vertex, time) => 
                {
                    return this.ReachedVertexForward((uint)vertex, time);
                });
            oneToManyForward.Run();

            // STEP2: calculate backward from target and keep track of all stops reached.
            var oneToManyBackward = new OneToManyDykstra(_graph, _interpreter, _targetVehicle, _target, _targetMax, true, (vertex, time) =>
            {
                return this.ReachedVertexBackward((uint)vertex, time);
            });
            oneToManyBackward.Run();

            // STEP3: use the calculate times at the stops from source/target and start to scan connections.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            _bestTargetStop = -1;

            // initialize stops status.
            for (int connectionId = 0; connectionId < _connections.Count; connectionId++)
            { // scan all connections.
                var connection = _connections[connectionId];
                var departureTime = connection.DepartureTime + (day * Constants.OneDayInSeconds);

                // check max search time.
                if (departureTime - startTime > _maximumSearchTime)
                { // maximum search time exceeded.
                    break; // stop searching without finding a result.
                }

                // check if target has been reached and if departure time exceeds target arrival time.
                var currentBestTime = 0;
                if (_bestTargetStop > 0)
                { // there was already a best time.
                    currentBestTime = _backwardStopStatuses[_bestTargetStop].Seconds +
                        _forwardStopStatuses[_bestTargetStop].Seconds;
                    if (departureTime >= currentBestTime)
                    { // the current time to target is never going to be better.
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

                    if (status.Seconds < departureTime - transferTime)
                    { // a departure here is possible if the trip is possible.
                        if (!tripPossibilities.TryGetValue(connection.TripId, out tripPossible))
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
                                Seconds = connection.ArrivalTime
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
                                var timeToTarget = backwardStatus.Seconds + arrivalStopStatus.Seconds;
                                if (_bestTargetStop < 0 ||
                                    currentBestTime > timeToTarget)
                                { // this current route is a better one.
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
            if(_db.IsStop(vertex, out stopId))
            { // the vertex is a stop, mark it as reached.
                _forwardStopStatuses.Add(stopId, new StopStatus()
                {
                    ConnectionId = -1,
                    Seconds = (int)time + (int)(_departureTime - _departureTime.Date).TotalSeconds,
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
            if (_db.IsStop(vertex, out stopId))
            { // the vertex is a stop, mark it as reached.
                _backwardStopStatuses.Add(stopId, new StopStatus()
                {
                    ConnectionId = -1,
                    Seconds = (int)time,
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