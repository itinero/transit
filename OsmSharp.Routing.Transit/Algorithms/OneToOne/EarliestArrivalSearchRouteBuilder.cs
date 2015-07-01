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
using OsmSharp.UI;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// A class reponsable for building an earliest arrival route.
    /// </summary>
    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
    public class EarliestArrivalSearchRouteBuilder : OneToOneRouteBuilder<EarliestArrivalSearch>
    {
        /// <summary>
        /// Holds the GTFS connections db.
        /// </summary>
        private GTFSConnectionsDb _connectionsDb;

        /// <summary>
        /// Creates a new earliest arrival route builder.
        /// </summary>
        /// <param name="earliestArrival">The earliest arrival algorithm.</param>
        /// <param name="connectionsDb">The connections database.</param>
        public EarliestArrivalSearchRouteBuilder(EarliestArrivalSearch earliestArrival, GTFSConnectionsDb connectionsDb)
            :base(earliestArrival)
        {
            _connectionsDb = connectionsDb;
        }

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        public override Route DoBuild()
        {
            var stops = new List<Tuple<int, EarliestArrival>>();
            var connections = new List<Connection?>();

            // build the route backwards from the target stop.
            var status = this.Algorithm.GetStopStatus(this.Algorithm.TargetStop);
            stops.Insert(0, new Tuple<int, EarliestArrival>(
                this.Algorithm.TargetStop, status));
            while(status.ConnectionId >= 0)
            { // keep searching until the connection id < 0, meaning the start status, without a previous trip.
                // get connection information.
                var connection = this.Algorithm.GetConnection(status.ConnectionId);
                status = this.Algorithm.GetStopStatus(connection.DepartureStop);
                if (status.TripId < 0)
                { // this stop has no trip, this means that it is the first stop.
                    // insert the stop first with the departuretime of this connection.
                    var statusWithTrip = new EarliestArrival()
                    {
                        ConnectionId = Constants.NoConnectionId,
                        Seconds = connection.DepartureTime,
                        Transfers = 1,
                        TripId = connection.TripId
                    };
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, EarliestArrival>(
                        connection.DepartureStop, statusWithTrip));

                    // insert the first and final stop.
                    connections.Insert(0, null);
                    stops.Insert(0, new Tuple<int, EarliestArrival>(
                        connection.DepartureStop, status));
                }
                else
                { // just insert as normal.
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, EarliestArrival>(
                        connection.DepartureStop, status));
                }
            }

            // convert the stop and connection sequences into an actual route.
            var route = new Route();
            route.Segments = new RouteSegment[stops.Count];

            // get the first stop.
            var feedStop = _connectionsDb.GetGTFSStop(stops[0].Item1);
            var routeTags = new List<RouteTags>();
            feedStop.AddTagsTo(routeTags);
            routeTags.Add(new RouteTags()
            {
                Key = "transit.timeofday",
                Value = stops[0].Item2.Seconds.ToInvariantString()
            });
            var departureTime = stops[0].Item2.Seconds;
            route.Segments[0] = new RouteSegment()
            {
                Distance = -1, // distance is not important in transit routing.
                Latitude = (float)feedStop.Latitude,
                Longitude = (float)feedStop.Longitude,
                Name = feedStop.Name,
                Time = 0,
                Type = RouteSegmentType.Start,
                Tags = routeTags.ToArray()
            };
            int? previousTripId = null;
            for (int idx = 1; idx < stops.Count; idx++)
            {
                // get the next ...->connection->stop->... pair.
                var connection = connections[idx - 1];
                feedStop = _connectionsDb.GetGTFSStop(stops[idx].Item1);

                // build tags.
                routeTags = new List<RouteTags>();
                feedStop.AddTagsTo(routeTags);

                // descide on connection/transfer/wait.
                if (previousTripId == null && connection == null)
                { // this is waiting: current connection is null and there was no previous trip.
                    routeTags.Add(new RouteTags()
                            {
                                Key = "transit.timeofday",
                                Value = stops[idx].Item2.Seconds.ToInvariantString()
                            });

                    route.Segments[idx] = new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Name = feedStop.Name,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Type = RouteSegmentType.Along,
                        Tags = routeTags.ToArray(),
                        Vehicle = "Transit.Wait" // not an actual vehicle but just waiting.
                    };
                }
                else if (previousTripId != null && connection == null)
                { // this is a transfer: current connection is null but there was a previous trip.
                    routeTags.Add(new RouteTags()
                    {
                        Key = "transit.timeofday",
                        Value = stops[idx].Item2.Seconds.ToInvariantString()
                    });

                    route.Segments[idx] = new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Name = feedStop.Name,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Type = RouteSegmentType.Along,
                        Tags = routeTags.ToArray(),
                        Vehicle = "Transit.Transfer" // not an actual vehicle but just a transfer.
                    };
                }
                else
                { // this is a connection: connection is not null.
                    // get route information.
                    var feedTrip = _connectionsDb.GetGTFSTrip(connection.Value.TripId);
                    var feedRoute = _connectionsDb.Feed.Routes.Get(feedTrip.RouteId);
                    var feedAgency = _connectionsDb.Feed.Agencies.Get(feedRoute.AgencyId);

                    feedTrip.AppendTagsTo(routeTags);
                    feedRoute.AppendTagsTo(routeTags);
                    feedAgency.AppendTagsTo(routeTags);
                    routeTags.Add(new RouteTags()
                            {
                                Key = "transit.timeofday",
                                Value = stops[idx].Item2.Seconds.ToInvariantString()
                            });

                    route.Segments[idx] = new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Name = feedStop.Name,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Type = RouteSegmentType.Along,
                        Tags = routeTags.ToArray(),
                        Vehicle = feedRoute.Type.ToVehicleUniqueName()
                    };
                }
            }

            // mark last segment as stop.
            if (route.Segments.Length > 0)
            {
                route.Segments[route.Segments.Length - 1].Type = RouteSegmentType.Stop;
            }

            return route;
        }
    }
}