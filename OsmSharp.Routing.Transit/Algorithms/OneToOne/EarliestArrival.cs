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

using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two stops, with a given departure time, that has the best arrival time.
    /// </summary>
    public class EarliestArrival : OneToOneRoutingAlgorithmBase
    {

        /// <summary>
        /// Holds the connections db.
        /// </summary>
        private readonly ConnectionsDb _connections;

        /// <summary>
        /// Holds the source stop.
        /// </summary>
        private readonly int _sourceStop;

        /// <summary>
        /// Holds the target stop.
        /// </summary>
        private readonly int _targetStop;

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
            return (status1.Transfers * 3 * 60 + status1.Seconds).CompareTo(status2.Transfers * 3 * 60 + status2.Seconds);
        };

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        /// <param name="connections">The connections db.</param>
        /// <param name="sourceStop">The stop to start at.</param>
        /// <param name="targetStop">The stop to end at.</param>
        /// <param name="departureTime">The departure time.</param>
        public EarliestArrival(ConnectionsDb connections, int sourceStop, int targetStop, DateTime departureTime)
        {
            _connections = connections;
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = connections.IsTripPossible;
        }

        /// <summary>
        /// Holds all the statuses of all stops that have been touched.
        /// </summary>
        private Dictionary<int, StopStatus> _stopStatuses;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // Remarks:
            // - Use the number of seconds from the previous midnight, this is also what is used to sort the connections.
            // - Use the date to determine if a trip is possible.
            // - When the midnight barries is passed, increase the date.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();

            // initialize stops status.
            _stopStatuses = new Dictionary<int, StopStatus>();
            _stopStatuses.Add(_sourceStop, new StopStatus()
            {
                TripId = -1,
                ConnectionId = -1,
                Seconds = startTime,
                Transfers = 0
            });
            StopStatus? toStatus = null;

            var connections = _connections.GetDepartureTimeView();
            for (int connectionId = 0; connectionId < connections.Count; connectionId++)
            { // scan all connections.
                var connection = connections[connectionId];
                var departureTime = connection.DepartureTime + (day * Constants.OneDayInSeconds);

                // check max search time.
                if (departureTime - startTime > _maximumSearchTime)
                { // maximum search time exceeded.
                    break; // stop searching without finding a result.
                }

                // check if target has been reached and if departure time exceeds target arrival time.
                if (toStatus.HasValue && departureTime >= toStatus.Value.Seconds)
                { // the current status at 'to' is the best status it's ever going to get.

                }

                StopStatus status;
                if (_stopStatuses.TryGetValue(connection.DepartureStop, out status))
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
                            var targetStatus = new StopStatus()
                            {
                                TripId = connection.TripId,
                                ConnectionId = connectionId,
                                Transfers = (short)(status.Transfers + transfer),
                                Seconds = connection.ArrivalTime
                            };

                            StopStatus existingStatus;
                            if (_stopStatuses.TryGetValue(connection.ArrivalStop, out existingStatus))
                            { // compare statuses if already a status there.
                                if (_compareStatuses(existingStatus, targetStatus) > 0)
                                { // existingStatus > targetStatus here, replace existing status.
                                    _stopStatuses[connection.ArrivalStop] = targetStatus;
                                }
                            }
                            else
                            { // no status yet, just set it.
                                _stopStatuses.Add(connection.ArrivalStop, targetStatus);
                            }

                            // check target.
                            if (connection.ArrivalStop == _targetStop)
                            { // update toStatus.
                                toStatus = _stopStatuses[connection.ArrivalStop];
                                this.HasSucceeded = true;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the calculated arrival time.
        /// </summary>
        /// <returns></returns>
        public DateTime ArrivalTime()
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(_stopStatuses[_targetStop].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public int Duration()
        {
            this.CheckHasRunAndHasSucceeded();

            return _stopStatuses[_targetStop].Seconds - (int)(_departureTime - _departureTime.Date).TotalSeconds;
        }

        /// <summary>
        /// Represents a stop status. 
        /// </summary>
        /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
        private struct StopStatus
        {
            /// <summary>
            /// Gets or sets the last trip id.
            /// </summary>
            public int TripId { get; set; }

            /// <summary>
            /// Gets or sets the connection id of the connection used.
            /// </summary>
            public long ConnectionId { get; set; }

            /// <summary>
            /// Gets or sets the # of transfers.
            /// </summary>
            public short Transfers { get; set; }

            /// <summary>
            /// Gets or sets the number of seconds from source.
            /// </summary>
            public int Seconds { get; set; }
        }
    }
}