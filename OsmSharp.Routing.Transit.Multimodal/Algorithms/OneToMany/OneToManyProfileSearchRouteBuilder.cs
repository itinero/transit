//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp.Math.Geo;
//using OsmSharp.Routing.Transit.Builders;
//using OsmSharp.Routing.Transit.Data;
//using OsmSharp.Routing.Transit.Multimodal.Data;
//using System;
//using System.Collections.Generic;

//namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany
//{
//    /// <summary>
//    /// A class reponsable for building a route.
//    /// </summary>
//    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
//    public class OneToManyProfileSearchRouteBuilder : RouteBuilder<OneToManyProfileSearch>
//    {
//        private readonly MultimodalConnectionsDb _db;
//        private readonly int _i = 0;

//        /// <summary>
//        /// Creates a new route builder.
//        /// </summary>
//        public OneToManyProfileSearchRouteBuilder(OneToManyProfileSearch profileSearch, MultimodalConnectionsDb db, int i)
//            : base(profileSearch)
//        {
//            _db = db;
//            _i = i;
//        }

//        /// <summary>
//        /// Executes the route build step.
//        /// </summary>
//        /// <returns></returns>
//        public override Route DoBuild()
//        {
//            if(!this.Algorithm.GetHasRoute(_i))
//            { // there is no route.
//                return null;
//            }

//            if (this.Algorithm.GetHasTransit(_i))
//            { // there is transit data; start by building that route.
//                var stops = new List<Tuple<int, Profile>>();
//                var connections = new List<Connection?>();

//                // build the route backwards from the target stop.
//                var bestTargetStop = this.Algorithm.GetBestTargetStop(_i);
//                var bestSourceStop = -1;
//                var profiles = this.Algorithm.GetStopProfiles(bestTargetStop);
//                var profile = profiles.GetBest();
//                stops.Insert(0, new Tuple<int, Profile>(
//                    bestTargetStop, profile));
//                var stopsInRoute = new HashSet<int>();
//                stopsInRoute.Add(bestTargetStop);
//                while (profile.ConnectionId >= 0)
//                { // keep searching until the connection id < 0, meaning the start status, without a previous trip.
//                    // get connection information.
//                    var connection = this.Algorithm.GetConnection(profile.ConnectionId);
//                    profiles = this.Algorithm.GetStopProfiles(connection.DepartureStop);
//                    profile = profiles.GetBest(profile);
//                    if (profile.ConnectionId == Constants.NoConnectionId)
//                    { // this stop has no trip, this means that it is the first stop.
//                        // insert the stop first with the departuretime of this connection.
//                        var statusWithTrip = new Profile()
//                        {
//                            ConnectionId = Constants.NoConnectionId,
//                            Seconds = connection.DepartureTime,
//                            Transfers = 1
//                        };
//                        connections.Insert(0, connection);
//                        stops.Insert(0, new Tuple<int, Profile>(
//                            connection.DepartureStop, statusWithTrip));

//                        // insert the first and final stop.
//                        connections.Insert(0, null);
//                        stops.Insert(0, new Tuple<int, Profile>(
//                            connection.DepartureStop, profile));
//                        bestSourceStop = connection.DepartureStop;
//                    }
//                    else
//                    { // just insert as normal.
//                        connections.Insert(0, connection);
//                        stops.Insert(0, new Tuple<int, Profile>(
//                            connection.DepartureStop, profile));
//                        if (stopsInRoute.Contains(connection.DepartureStop))
//                        {
//                            throw new Exception("The same stop twice in the same route is impossible.");
//                        }
//                        stopsInRoute.Add(connection.DepartureStop);
//                    }
//                }

//                // convert the stop and connection sequences into an actual route.
//                var transitRoute = new Route();
//                transitRoute.Segments = new RouteSegment[stops.Count];

//                // get the first stop.
//                var feedStop = _db.ConnectionsDb.GetGTFSStop(stops[0].Item1);
//                var routeTags = new List<RouteTags>();
//                feedStop.AddTagsTo(routeTags);
//                routeTags.Add(new RouteTags()
//                {
//                    Key = "transit.timeofday",
//                    Value = stops[0].Item2.Seconds.ToInvariantString()
//                });
//                var departureTime = stops[0].Item2.Seconds;
//                transitRoute.Segments[0] = new RouteSegment()
//                {
//                    Distance = 0,
//                    Latitude = (float)feedStop.Latitude,
//                    Longitude = (float)feedStop.Longitude,
//                    Name = feedStop.Name,
//                    Time = 0,
//                    Type = RouteSegmentType.Start,
//                    Tags = routeTags.ToArray()
//                };
//                var distance = 0.0;
//                int? previousTripId = null;
//                for (var idx = 1; idx < stops.Count; idx++)
//                {
//                    var previousStopLocation = new GeoCoordinate(feedStop.Latitude, feedStop.Longitude);

//                    // get the next ...->connection->stop->... pair.
//                    var connection = connections[idx - 1];
//                    if (connection.HasValue &&
//                        connection.Value.TripId == Constants.PseudoConnectionTripId)
//                    { // ignore this connection.
//                        connection = null;
//                    }
//                    feedStop = _db.ConnectionsDb.GetGTFSStop(stops[idx].Item1);
//                    var stopLocation = new GeoCoordinate(feedStop.Latitude, feedStop.Longitude);
//                    distance = distance + previousStopLocation.DistanceReal(stopLocation).Value;

//                    // build tags.
//                    routeTags = new List<RouteTags>();
//                    feedStop.AddTagsTo(routeTags);

//                    // descide on connection/transfer/wait.
//                    if (previousTripId == null && connection == null)
//                    { // this is waiting: current connection is null and there was no previous trip.
//                        routeTags.Add(new RouteTags()
//                        {
//                            Key = "transit.timeofday",
//                            Value = stops[idx].Item2.Seconds.ToInvariantString()
//                        });

//                        transitRoute.Segments[idx] = new RouteSegment()
//                        {
//                            Distance = distance,
//                            Latitude = (float)feedStop.Latitude,
//                            Longitude = (float)feedStop.Longitude,
//                            Name = feedStop.Name,
//                            Time = stops[idx].Item2.Seconds - departureTime,
//                            Type = RouteSegmentType.Along,
//                            Tags = routeTags.ToArray(),
//                            Vehicle = OsmSharp.Routing.Transit.Constants.WaitVehicle // not an actual vehicle but just waiting.
//                        };
//                    }
//                    else if (previousTripId != null && connection == null)
//                    { // this is a transfer: current connection is null but there was a previous trip.
//                        routeTags.Add(new RouteTags()
//                        {
//                            Key = "transit.timeofday",
//                            Value = stops[idx].Item2.Seconds.ToInvariantString()
//                        });

//                        transitRoute.Segments[idx] = new RouteSegment()
//                        {
//                            Distance = distance,
//                            Latitude = (float)feedStop.Latitude,
//                            Longitude = (float)feedStop.Longitude,
//                            Name = feedStop.Name,
//                            Time = stops[idx].Item2.Seconds - departureTime,
//                            Type = RouteSegmentType.Along,
//                            Tags = routeTags.ToArray(),
//                            Vehicle = OsmSharp.Routing.Transit.Constants.TransferVehicle // not an actual vehicle but just a transfer.
//                        };
//                    }
//                    else
//                    { // this is a connection: connection is not null.
//                        // get route information.
//                        var feedTrip = _db.ConnectionsDb.GetGTFSTrip(connection.Value.TripId);
//                        var feedRoute = _db.ConnectionsDb.Feed.Routes.Get(feedTrip.RouteId);
//                        var feedAgency = _db.ConnectionsDb.Feed.Agencies.Get(feedRoute.AgencyId);

//                        feedTrip.AppendTagsTo(routeTags);
//                        feedRoute.AppendTagsTo(routeTags);
//                        feedAgency.AppendTagsTo(routeTags);
//                        routeTags.Add(new RouteTags()
//                        {
//                            Key = "transit.timeofday",
//                            Value = stops[idx].Item2.Seconds.ToInvariantString()
//                        });

//                        transitRoute.Segments[idx] = new RouteSegment()
//                        {
//                            Distance = distance,
//                            Latitude = (float)feedStop.Latitude,
//                            Longitude = (float)feedStop.Longitude,
//                            Name = feedStop.Name,
//                            Time = stops[idx].Item2.Seconds - departureTime,
//                            Type = RouteSegmentType.Along,
//                            Tags = routeTags.ToArray(),
//                            Vehicle = feedRoute.Type.ToVehicleUniqueName()
//                        };
//                    }
//                }

//                // mark last segment as stop.
//                if (transitRoute.Segments.Length > 0)
//                {
//                    transitRoute.Segments[transitRoute.Segments.Length - 1].Type = RouteSegmentType.Stop;
//                }

//                // build route from sourcestop back to source location.
//                uint sourceStopVertex;
//                if (!_db.TryGetVertex(bestSourceStop, out sourceStopVertex))
//                { // vertex not found.
//                    throw new Exception("No vertex found for sourcestop.");
//                }
//                var sourceRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.SourceSearch, sourceStopVertex);
//                var sourceRoute = sourceRouteBuilder.Build();

//                // build route from targetstop back to target location.
//                uint targetStopVertex;
//                if (!_db.TryGetVertex(bestTargetStop, out targetStopVertex))
//                { // vertex not found.
//                    throw new Exception("No vertex found for sourcestop.");
//                }
//                var targetRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.GetTargetSearch(_i), targetStopVertex);
//                var targetRoute = targetRouteBuilder.Build();

//                // concatenate routes.
//                var route = transitRoute;
//                if (sourceRoute.Segments.Length > 1)
//                { // route is more than just the source-stop.
//                    route = Route.Concatenate(sourceRoute, transitRoute);
//                }
//                else if (sourceRoute.Segments.Length == 1)
//                { // make sure the vehicle profile is set at the start point.
//                    route.Segments[0].Vehicle = sourceRoute.Segments[0].Vehicle;
//                }
//                if (targetRoute.Segments.Length > 1)
//                { // route is more than just the target-stop.
//                    route = Route.Concatenate(route, targetRoute);
//                }

//                // add extra tags to route.
//                routeTags.Clear();
//                routeTags.Add(new RouteTags()
//                {
//                    Key = "departuretime",
//                    Value = this.Algorithm.DepartureTime.ToString("{dd-MM-yyyy HH:mm:ss}")
//                });
//                route.Tags = routeTags.ToArray();

//                return route;
//            }
//            else
//            { // no transit route, just use the shortest path.
//                var bestVertex = this.Algorithm.GetBestNonTransitVertex(_i);

//                // build route from sourcestop back to source location.
//                var sourceRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.SourceSearch, bestVertex);
//                var sourceRoute = sourceRouteBuilder.Build();

//                // build route from targetstop back to target location.
//                var targetRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.GetTargetSearch(_i), bestVertex);
//                var targetRoute = targetRouteBuilder.Build();

//                return Route.Concatenate(sourceRoute, targetRoute);
//            }
//        }
//    }
//}
