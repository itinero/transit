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

using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Algorithms.Default;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Data;
using System;
using System.Collections.Generic;
using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Profiles;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two locations in the routing graph, with a given departure time, that has the best arrival time.
    /// </summary>
    public class EarliestArrivalSearch : RoutingAlgorithmBase
    {
        private readonly MultimodalDb _db;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = OsmSharp.Routing.Transit.Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly Func<float, float> _lazyness;
        private readonly ClosestStopSearchBase _sourceSearch;
        private readonly ClosestStopSearchBase _targetSearch;

        /// <summary>
        /// Holds the function to compare two stop statuses.
        /// </summary>
        private readonly Func<StopStatus, StopStatus, int> _compareStatuses = (status1, status2) =>
        {
            if (status1.Seconds == status2.Seconds)
            {
                if (status1.Transfers == status2.Transfers)
                {
                    return status2.ConnectionId.CompareTo(status1.ConnectionId);
                }
                return status1.Transfers.CompareTo(status2.Transfers);
            }
            if (status1.Seconds + status1.Lazyness !=
                status2.Seconds + status2.Lazyness)
            {
                return (status1.Seconds + status1.Lazyness).CompareTo(
                    status2.Seconds + status2.Lazyness);
            }
            return status2.ConnectionId.CompareTo(status1.ConnectionId);
        };

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public EarliestArrivalSearch(MultimodalDb db, DateTime departureTime,
            ClosestStopSearchBase sourceSearch, ClosestStopSearchBase targetSearch)
            : this(db, departureTime, sourceSearch, targetSearch, (t) => { return 0; })
        {

        }

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        public EarliestArrivalSearch(MultimodalDb db, DateTime departureTime,
            ClosestStopSearchBase sourceSearch, ClosestStopSearchBase targetSearch, Func<float, float> lazyness)
        {
            _db = db;
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _lazyness = lazyness;

            _isTripPossible = db.ConnectionsDb.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public EarliestArrivalSearch(MultimodalDb db, DateTime departureTime,
            ClosestStopSearchBase sourceSearch, ClosestStopSearchBase targetSearch, Func<float, float> lazyness,
            Func<int, DateTime, bool> isTripPossible, Func<StopStatus, StopStatus, int> compareStatuses)
        {
            _db = db;
            _departureTime = departureTime;
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _lazyness = lazyness;

            _isTripPossible = isTripPossible;
            _compareStatuses = compareStatuses;
        }

        private Dictionary<int, StopStatus> _forwardStopStatuses; // Holds all the statuses of all stops that have been touched by the forward search.
        private Dictionary<int, StopStatus> _backwardStopStatuses; // Holds all the statuses of all stops that have been touched by the backward search.
        private int _bestTargetStop = -1; // Holds the best stop to target.
        private ConnectionsView _connections;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // get the connection view.
            _connections = _db.ConnectionsDb.GetDepartureTimeView();

            // initialize visits.
            _forwardStopStatuses = new Dictionary<int, StopStatus>(100);
            _backwardStopStatuses = new Dictionary<int, StopStatus>(100);

            // calculate forward from source and keep track of all stops reached.
            _sourceSearch.StopFound = this.ReachedVertexForward;
            _sourceSearch.Run();

            // calculate backward from target and keep track of all stops reached.
            _targetSearch.StopFound = this.ReachedEdgeBackward;
            _targetSearch.Run();

            // use the calculate times at the stops from source/target and start to scan connections.
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
                    if (status.TripId == connection.TripId ||
                        (status.TripId != Constants.PseudoConnectionTripId && connection.TripId == Constants.PseudoConnectionTripId) ||
                        (status.TripId == Constants.PseudoConnectionTripId && connection.TripId != Constants.PseudoConnectionTripId))
                    { // the same trip.
                        transferTime = 0;
                        tripPossible = true; // trip is possible because it is already used.
                        transfer = 0;
                        if (connection.TripId == Constants.PseudoConnectionTripId)
                        { // consider this a transfer because the connection itself is a transfer.
                            transfer = 1;
                        }
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
                            var accepted = false;
                            if (_forwardStopStatuses.TryGetValue(connection.ArrivalStop, out existingStatus))
                            { // compare statuses if already a status there.
                                if (_compareStatuses(existingStatus, arrivalStatus) > 0)
                                { // existingStatus > targetStatus here, replace existing status.
                                    _forwardStopStatuses[connection.ArrivalStop] = arrivalStatus;
                                    accepted = true;
                                }
                            }
                            else
                            { // no status yet, just set it.
                                _forwardStopStatuses.Add(connection.ArrivalStop, arrivalStatus);
                                accepted = true;
                            }

                            // check target(s).
                            StopStatus backwardStatus;
                            if (accepted && _backwardStopStatuses.TryGetValue(connection.ArrivalStop, out backwardStatus))
                            { // this stop has been reached by the backward search, figure out if it represents a better connection.
                                var arrivalStopStatus = _forwardStopStatuses[connection.ArrivalStop];
                                var weight = backwardStatus.Seconds + arrivalStopStatus.Seconds +
                                    backwardStatus.Lazyness + arrivalStopStatus.Lazyness;
                                if (_bestTargetStop < 0 ||
                                    bestWeight >= weight)
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
        /// <returns></returns>
        private bool ReachedVertexForward(uint stop, float seconds)
        {
            var newStatus = new StopStatus()
                    {
                        ConnectionId = Constants.NoConnectionId,
                        Seconds = (int)seconds + (int)(_departureTime - _departureTime.Date).TotalSeconds,
                        Lazyness = (int)_lazyness(seconds),
                        Transfers = 0,
                        TripId = Constants.NoTripId
                    };

            StopStatus existingStatus;
            if (_forwardStopStatuses.TryGetValue((int)stop, out existingStatus))
            { // a status already exists, compare.
                if(existingStatus.Seconds > newStatus.Seconds)
                {
                    _forwardStopStatuses[(int)stop] = newStatus;
                }
            }
            else
            {
                _forwardStopStatuses.Add((int)stop, newStatus);
            }
            return false;
        }

        /// <summary>
        /// Called when an edge was reached during a backward search.
        /// </summary>
        private bool ReachedEdgeBackward(uint stop, float seconds)
        {
            var newStatus = new StopStatus()
            {
                ConnectionId = Constants.NoConnectionId,
                Seconds = (int)seconds,
                Lazyness = (int)_lazyness(seconds),
                Transfers = 0,
                TripId = Constants.NoTripId
            };
            
            StopStatus existingStatus;
            if(_backwardStopStatuses.TryGetValue((int)stop, out existingStatus))
            {
                if(existingStatus.Seconds > newStatus.Seconds)
                {
                    _backwardStopStatuses[(int)stop] = newStatus;
                }
            }
            else
            {
                _backwardStopStatuses.Add((int)stop, newStatus);
            }
            return false;
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

            /// <summary>
            /// Returns a description of this status.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{4} -> {0}s@{1} #{2} ({3})", this.Seconds, this.TripId, this.Transfers, this.Lazyness,
                    this.ConnectionId);
            }
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public ClosestStopSearchBase SourceSearch
        {
            get
            {
                return _sourceSearch;
            }
        }

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public ClosestStopSearchBase TargetSearch
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
                    ConnectionId = Constants.NoConnectionId,
                    Seconds = -1,
                    Transfers = -1,
                    TripId = Constants.NoTripId
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