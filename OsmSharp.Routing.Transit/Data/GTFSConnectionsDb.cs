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
using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A database containing all transit-connections.
    /// </summary>
    public class GTFSConnectionsDb
    {
        private readonly IGTFSFeed _feed; // the source GTFS-feed.
        private readonly int MAX_TRANSFER_DISTANCE = 50; // maximum transfer distance in meter.
        private readonly int MIN_TRANSFER_TIME = 3 * 60; // maximum transfer time in seconds.

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
            var routeIds = this.BuildRouteIds();

            var connections = this.BuildConnectionList(stopIds, tripIds, routeIds);

            this.BuildDepartureTimeView(connections);
            this.BuildArrivalTimeView(new List<Connection>(connections));
        }

        /// <summary>
        /// Builds all the route ids.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> BuildRouteIds()
        {
            var routes = new Dictionary<string, int>();
            var routeId = 0;
            foreach (var route in _feed.Routes)
            {
                routes.Add(route.Id, routeId);
                routeId++;
            }
            return routes;
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
        /// <returns></returns>
        private List<Connection> BuildConnectionList(Dictionary<string, int> stopIds, Dictionary<string, int> tripIds,
            Dictionary<string, int> routeIds)
        {
            var connections = new List<Connection>();
            StopTime previousStopTime = null;
            var arrivalTimes = new Dictionary<int, List<int>>();
            var departureTimes = new Dictionary<int, List<int>>();
            var tripIdx = 0;
            var routeId = -1;
            foreach (var stopTime in _feed.StopTimes)
            {
                // get target stop id and arrival time.
                var arrivalStop = stopIds[stopTime.StopId];
                var arrival = stopTime.ArrivalTime.TotalSeconds;

                // add to list of stop times.
                List<int> localStopTimes;
                if (!arrivalTimes.TryGetValue(arrivalStop, out localStopTimes))
                { // create new list.
                    localStopTimes = new List<int>();
                    arrivalTimes.Add(arrivalStop, localStopTimes);
                }
                localStopTimes.Add(arrival);

                // check if two stop times belong to the same trip.
                if (previousStopTime != null &&
                    previousStopTime.TripId == stopTime.TripId)
                { // we have two stops in the same trip
                    // get source stop id and departure time.
                    var departure = previousStopTime.DepartureTime.TotalSeconds;
                    var departureStop = stopIds[previousStopTime.StopId];

                    // add to list of stop times.
                    if (!departureTimes.TryGetValue(departureStop, out localStopTimes))
                    { // create new list.
                        localStopTimes = new List<int>();
                        departureTimes.Add(departureStop, localStopTimes);
                    }
                    localStopTimes.Add(departure);

                    // get trip id.
                    var trip = tripIds[previousStopTime.TripId];

                    // add connection.
                    if (arrivalStop == departureStop)
                    { // an invalid connection.
                        OsmSharp.Logging.Log.TraceEvent("GTFSConnectionsDB.BuildConnectionList", Logging.TraceEventType.Warning,
                            string.Format("A connection with the same departure and arrival stop detected on trip: {0}.", previousStopTime.TripId));
                    }
                    else
                    { // we have a valid connection here!
                        connections.Add(new Connection()
                        {
                            ArrivalStop = arrivalStop,
                            ArrivalTime = arrival,
                            DepartureStop = departureStop,
                            DepartureTime = departure,
                            TripId = trip,
                            TripIdx = tripIdx,
                            RouteId = routeId
                        });
                    }
                    tripIdx++;
                }
                else
                { // reset trip idx.
                    tripIdx = 0;
                    var trip = _feed.Trips.Get(tripIds[stopTime.TripId]);
                    routeId = routeIds[trip.RouteId];
                }
                previousStopTime = stopTime;
            }

            // build transfers db.
            foreach (var sourceKeyValue in stopIds)
            {
                var sourceStop = _feed.Stops.Get(sourceKeyValue.Value);
                var sourceLocation = new GeoCoordinate(sourceStop.Latitude, sourceStop.Longitude);
                foreach (var targetKeyValue in stopIds)
                {
                    if (targetKeyValue.Value == sourceKeyValue.Value)
                    { // do not transfer between identical stops.
                        continue;
                    }

                    var targetStop = _feed.Stops.Get(targetKeyValue.Value);
                    var targetLocation = new GeoCoordinate(targetStop.Latitude, targetStop.Longitude);
                    if (targetLocation.DistanceEstimate(sourceLocation).Value < MAX_TRANSFER_DISTANCE)
                    { // ok, between these two stops we should create transfers.
                        // build departure times.
                        List<int> targetTimes, sourceTimes;
                        if (arrivalTimes.TryGetValue(sourceKeyValue.Value, out sourceTimes) &&
                           departureTimes.TryGetValue(targetKeyValue.Value, out targetTimes))
                        { // both lists are available.
                            sourceTimes.Sort();
                            targetTimes.Sort();

                            var sourceIdx = 0;
                            var targetIdx = 0;
                            while (sourceIdx < sourceTimes.Count &&
                                targetIdx < targetTimes.Count)
                            {
                                var diff = targetTimes[targetIdx] - sourceTimes[sourceIdx];
                                if (diff >= MIN_TRANSFER_TIME)
                                { // ok a valid tranfer found here.
                                    connections.Add(new Connection()
                                        {
                                            ArrivalStop = targetKeyValue.Value,
                                            ArrivalTime = targetTimes[targetIdx],
                                            DepartureStop = sourceKeyValue.Value,
                                            DepartureTime = sourceTimes[sourceIdx],
                                            TripId = Constants.PseudoConnectionTripId,
                                            RouteId = Constants.NoRouteId
                                        });
                                    sourceIdx++;
                                }
                                if (sourceIdx < sourceTimes.Count)
                                {
                                    // move target until after source time.
                                    while (targetIdx < targetTimes.Count &&
                                        targetTimes[targetIdx] - sourceTimes[sourceIdx] <= MIN_TRANSFER_TIME)
                                    { // transfer still not possible.
                                        targetIdx++;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //// remove duplicates by first sorting and the removing consequitive duplicates.
            //connections.Sort((connection1, connection2) =>
            //{
            //    if (connection1.DepartureTime == connection2.DepartureTime)
            //    {
            //        if (connection1.TripId == connection2.TripId)
            //        {
            //            return connection1.TripIdx.CompareTo(connection2.TripIdx);
            //        }
            //        return connection1.TripId.CompareTo(connection2.TripId);
            //    }
            //    return connection1.DepartureTime.CompareTo(connection2.DepartureTime);
            //});
            //var 
            //for (var i = connections.Count - 1; i >= 1; i--)
            //{
            //    var connection1 = connections[i];
            //    var connection2 = connections[i - 1];
            //    if(Connection.Equals(connection1, connection2))
            //    { // connections are equal.
            //        connections.RemoveAt(i);
            //    }
            //}

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
                    if(connection1.DepartureTime == connection2.DepartureTime)
                    {
                        if (connection1.TripId == connection2.TripId)
                        {
                            return connection1.TripIdx.CompareTo(connection2.TripIdx);
                        }
                        return connection1.TripId.CompareTo(connection2.TripId);
                    }
                    return connection1.DepartureTime.CompareTo(connection2.DepartureTime);
                });

            // set previousconnection id's.
            var currentConnection = new Dictionary<int, int>();
            for (var connectionId = 0; connectionId < connections.Count; connectionId++)
            {
                var connection = connections[connectionId];
                if(connection.TripId != Constants.NoConnectionId &&
                    connection.TripId != Constants.PseudoConnectionTripId)
                {
                    int previousConnectionId;
                    if (currentConnection.TryGetValue(connection.TripId, out previousConnectionId))
                    {
                        connection.PreviousConnectionId = previousConnectionId;
                        connections[connectionId] = connection;
                    }
                    currentConnection[connection.TripId] = connectionId;
                }
            }
                
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
                if (connection1.DepartureTime == connection2.DepartureTime)
                {
                    if (connection1.TripId == connection2.TripId)
                    {
                        return connection2.TripIdx.CompareTo(connection1.TripIdx);
                    }
                    return connection2.TripId.CompareTo(connection1.TripId);
                }
                return connection2.ArrivalTime.CompareTo(connection1.ArrivalTime);
            });
            _arrivalTimeView = new ConnectionsListView(connections);
        }

        #endregion
    }
}