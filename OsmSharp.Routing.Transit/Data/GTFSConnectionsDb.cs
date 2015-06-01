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
using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A database containing all transit-connections.
    /// </summary>
    public class GTFSConnectionsDb
    {
        /// <summary>
        /// Holds the feed.
        /// </summary>
        private readonly IGTFSFeed _feed;

        /// <summary>
        /// Creates a new GTFS connections db based on the given feed.
        /// </summary>
        /// <param name="feed"></param>
        /// <remarks>The use multiple feeds they need to be merged into one feed first.</remarks>
        public GTFSConnectionsDb(IGTFSFeed feed)
        {
            _feed = feed;

            this.BuildViews();
        }

        /// <summary>
        /// Gets a view on the stops.
        /// </summary>
        /// <returns></returns>
        public GTFSStopsView GetStops()
        {
            return _stopsView;
        }

        /// <summary>
        /// Gets a view on the connections sorted by departure time.
        /// </summary>
        /// <returns></returns>
        public ConnectionsView GetDepartureTimeView()
        {
            return _departureTimeView;
        }

        /// <summary>
        /// Gets a view on the connections sorted by arrival time.
        /// </summary>
        /// <returns></returns>
        public ConnectionsView GetArrivalTimeView()
        {
            return _arrivalTimeView;
        }
        
        /// <summary>
        /// Gets the function to determine if a trip is possible on a given day.
        /// </summary>
        public Func<int, DateTime, bool> IsTripPossible
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

        /// <summary>
        /// Gets the feed.
        /// </summary>
        public IGTFSFeed Feed
        {
            get
            {
                return _feed;
            }
        }

        /// <summary>
        /// Gets the GTFS stop for the given stop id.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public global::GTFS.Entities.Stop GetGTFSStop(int stopId)
        {
            return _feed.Stops.Get(stopId);
        }

        /// <summary>
        /// Gets the GTFS trip the given trip id.
        /// </summary>
        /// <param name="tripId"></param>
        /// <returns></returns>
        public global::GTFS.Entities.Trip GetGTFSTrip(int tripId)
        {
            return _feed.Trips.Get(tripId);
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
        /// Holds the stops view.
        /// </summary>
        private GTFSStopsView _stopsView;

        /// <summary>
        /// Builds all views.
        /// </summary>
        private void BuildViews()
        {
            _stopsView = new GTFSStopsView(_feed.Stops);

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
        private Dictionary<string, int> BuildStopIds()
        {
            var stops = new Dictionary<string, int>();
            var stopId = 0;
            foreach (var stop in _feed.Stops)
            {
                stops.Add(stop.Id, stopId);
                stopId++;
            }
            return stops;
        }

        /// <summary>
        /// Builds all the trip ids.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> BuildTripIds()
        {
            var trips = new Dictionary<string, int>();
            var tripId = 0;
            foreach (var trip in _feed.Trips)
            {
                trips.Add(trip.Id, tripId);
                tripId++;
            }
            return trips;
        }

        /// <summary>
        /// Builds a connections list.
        /// </summary>
        /// <param name="stopIds">The stop id's per feed.</param>
        /// <param name="tripIds">The trip id's per feed.</param>
        /// <returns></returns>
        private List<Connection> BuildConnectionList(Dictionary<string, int> stopIds, Dictionary<string, int> tripIds)
        {
            var connections = new List<Connection>();
            StopTime previousStopTime = null;
            foreach (var stopTime in _feed.StopTimes)
            {
                // check if two stop times belong to the same trip.
                if (previousStopTime != null &&
                    previousStopTime.TripId == stopTime.TripId)
                { // we have two stops in the same trip
                    // parse arrival/departure.
                    var arrival = stopTime.ArrivalTime.TotalSeconds;
                    var departure = previousStopTime.DepartureTime.TotalSeconds;

                    // get start/end stopids.
                    var departureStop = stopIds[previousStopTime.StopId];
                    var arrivalStop = stopIds[stopTime.StopId];

                    // get trip id.
                    var trip = tripIds[previousStopTime.TripId];

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