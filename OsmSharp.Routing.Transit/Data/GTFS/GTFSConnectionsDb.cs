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

using GTFS;
using GTFS.Entities;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data.GTFS
{
    /// <summary>
    /// A connections database built based on GTFS feeds.
    /// </summary>
    public class GTFSConnectionsDb : ConnectionsDb
    {
        /// <summary>
        /// Holds all feeds.
        /// </summary>
        private readonly List<IGTFSFeed> _feeds;

        /// <summary>
        /// Creates a new GTFS connections db based on the given feeds.
        /// </summary>
        /// <param name="feeds"></param>
        public GTFSConnectionsDb(IEnumerable<IGTFSFeed> feeds)
        {
            _feeds = new List<IGTFSFeed>(feeds);

            this.BuildViews();
        }

        /// <summary>
        /// Gets a view on the connections sorted by departure time.
        /// </summary>
        /// <returns></returns>
        public override ConnectionsView GetDepartureTimeView()
        {
            return _departureTimeView;
        }

        /// <summary>
        /// Gets a view on the connections sorted by arrival time.
        /// </summary>
        /// <returns></returns>
        public override ConnectionsView GetArrivalTimeView()
        {
            return _arrivalTimeView;
        }
        
        /// <summary>
        /// Gets the function to determine if a trip is possible on a given day.
        /// </summary>
        public override Func<int, DateTime, bool> IsTripPossible
        {
            get 
            { 
                return (tripId, date) => 
                {
                    return this.DoIsTripPossible(tripId, date);
                };
            }
        }

        /// <summary>
        /// Returns true if the given trip is possible on the given date.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private bool DoIsTripPossible(int trip, DateTime date)
        {
            return true;
        }

        #region Connection Management

        /// <summary>
        /// Holds the departure time view.
        /// </summary>
        private ConnectionsView _departureTimeView;

        /// <summary>
        /// Holds the arrival time view.
        /// </summary>
        private ConnectionsView _arrivalTimeView;

        /// <summary>
        /// Builds all views.
        /// </summary>
        private void BuildViews()
        {
            var stopIds = this.BuildStopIds();
            var tripIds = this.BuildTripIds();

            var connections = this.BuildConnectionList(stopIds, tripIds);

            this.BuildDepartureTimeView(connections);
            this.BuildArrivalTimeView(new List<Connection>(connections));
        }

        /// <summary>
        /// Builds all the stop ids.
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, int>> BuildStopIds()
        {
            var result = new List<Dictionary<string, int>>(_feeds.Count);
            var stopId = 0;
            for(var idx = 0; idx < _feeds.Count; idx++)
            {
                var stops = new Dictionary<string, int>();
                foreach(var stop in _feeds[idx].GetStops())
                {
                    stops.Add(stop.Id, stopId);
                    stopId++;
                }
                result.Add(stops);
            }
            return result;
        }

        /// <summary>
        /// Builds all the trip ids.
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, int>> BuildTripIds()
        {
            var result = new List<Dictionary<string, int>>(_feeds.Count);
            var id = 0;
            for (var idx = 0; idx < _feeds.Count; idx++)
            {
                var trips = new Dictionary<string, int>();
                foreach (var trip in _feeds[idx].GetTrips())
                {
                    trips.Add(trip.Id, id);
                    id++;
                }
                result.Add(trips);
            }
            return result;
        }

        /// <summary>
        /// Builds a connections list.
        /// </summary>
        /// <param name="stopIds">The stop id's per feed.</param>
        /// <param name="tripIds">The trip id's per feed.</param>
        /// <returns></returns>
        private List<Connection> BuildConnectionList(List<Dictionary<string, int>> stopIds, List<Dictionary<string, int>> tripIds)
        {
            var connections = new List<Connection>();
            for (var idx = 0; idx < _feeds.Count; idx++)
            {
                var feed = _feeds[idx];
                var stops = stopIds[idx];
                var trips = tripIds[idx];

                StopTime previousStopTime = null;
                foreach(var stopTime in feed.GetStopTimes())
                {
                    // check if two stop times belong to the same trip.
                    if (previousStopTime != null && 
                        previousStopTime.TripId == stopTime.TripId)
                    { // we have two stops in the same trip
                        // parse arrival/departure.
                        var arrival = stopTime.ArrivalTime.TotalSeconds;
                        var departure = previousStopTime.DepartureTime.TotalSeconds;

                        // get start/end stopids.
                        var departureStop = stops[previousStopTime.StopId];
                        var arrivalStop = stops[stopTime.StopId];

                        // get trip id.
                        var trip = trips[previousStopTime.TripId];

                        connections.Add(new Connection()
                        {
                            ArrivalStop = arrivalStop,
                            ArrivalTime = arrival,
                            DepartureStop = departureStop,
                            DepartureTime = departure,
                            TripId = trip
                        });
                    }
                    previousStopTime = stopTime;
                }
            }
            return connections;
        }

        /// <summary>
        /// Builds departure time view.
        /// </summary>
        /// <param name="connections"></param>
        private void BuildDepartureTimeView(List<Connection> connections)
        {
            // sort connections.
            connections.Sort((connection1, connection2) => 
                {
                    return connection1.DepartureTime.CompareTo(connection2.DepartureTime);
                });
            _departureTimeView = new ConnectionsListView(connections);
        }

        /// <summary>
        /// Builds arrival time view.
        /// </summary>
        /// <param name="connections"></param>
        private void BuildArrivalTimeView(List<Connection> connections)
        {
            // sort connections.
            connections.Sort((connection1, connection2) => 
                {
                    return connection1.ArrivalTime.CompareTo(connection2.DepartureTime);
                });
            _arrivalTimeView = new ConnectionsListView(connections);
        }

        #endregion
    }
}
