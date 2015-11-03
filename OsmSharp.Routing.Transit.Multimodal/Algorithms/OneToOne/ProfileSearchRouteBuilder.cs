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
//using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
//using OsmSharp.Routing.Transit.Multimodal.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne
//{
//    /// <summary>
//    /// A class reponsable for building a route.
//    /// </summary>
//    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
//    public class ProfileSearchRouteBuilder : RouteBuilder<ProfileSearch>
//    {
//        private readonly MultimodalConnectionsDb _db;

//        /// <summary>
//        /// Creates a new route builder.
//        /// </summary>
//        public ProfileSearchRouteBuilder(ProfileSearch profileSearch, MultimodalConnectionsDb db)
//            : base(profileSearch)
//        {
//            _db = db;
//        }

//        /// <summary>
//        /// Executes the route build step.
//        /// </summary>
//        /// <returns></returns>
//        public override Route DoBuild()
//        {
//            if (this.Algorithm.HasTransit)
//            { // there is transit data; start by building that route.
//                var stops = new List<Tuple<int, Profile>>();
//                var connections = new List<Connection?>();

//                // build the route backwards from the target stop.
//                var bestTargetStop = this.Algorithm.GetBestTargetStop();

//                // choose the best profile (for now this is the profile with the best arrival time).
//                var profiles = this.Algorithm.GetStopProfiles(bestTargetStop);
//                var transfers = profiles.Count - 1;
//                var profile = profiles[transfers];
//                stops.Insert(0, new Tuple<int, Profile>(bestTargetStop, profile));

//                Connection previousConnection;
//                if(profile.PreviousConnectionId != Constants.NoConnectionId)
//                { // there is a connection to the target stop.
//                    previousConnection = this.Algorithm.GetConnection(profile.PreviousConnectionId);
//                    connections.Insert(0, previousConnection);

//                    var previousProfile = profile;
//                    while (true)
//                    { // keep looping until the best option is a profile without a previous connection.
//                        previousConnection = connections[0].Value;

//                        // choose the best option from the current profiles.
//                        profiles = this.Algorithm.GetStopProfiles(previousConnection.DepartureStop);
//                        if (profiles.Count > transfers &&
//                            profiles[transfers].Seconds != Constants.NoSeconds)
//                        { // there is a profile for the same amount of transfers.
//                            var connection = this.Algorithm.GetConnection(profiles[transfers].PreviousConnectionId);
//                            if(previousConnection.TripId == Constants.PseudoConnectionTripId)
//                            { // the previous connection was a pseudo connection, choose a connection with?
//                                // search for the best connection available (but don't take into account transfer time)
//                                profile = First(profiles, 
//                                    x => x.Seconds != Constants.NoSeconds &&
//                                        x.Seconds <= previousConnection.DepartureTime &&
//                                            previousConnection.PreviousConnectionId > x.PreviousConnectionId,
//                                            out transfers);
//                            }
//                            else if (previousConnection.TripId == connection.TripId)
//                            { // ok, this is it!
//                                profile = profiles[transfers];
//                            }
//                            else
//                            { // not the same trip. check if there is a previous connection and if the trip doesn't end here.
//                                var previousTripStatus = this.Algorithm.GetTripStatus(previousConnection.TripId);
//                                if (previousTripStatus.StopId == previousConnection.DepartureStop)
//                                { // there is not previous connection on the same trip anymore.
//                                    // search for the best connection available.
//                                    profile = First(profiles, 
//                                            x => x.Seconds != Constants.NoSeconds &&
//                                                x.Seconds <= previousConnection.DepartureTime &&
//                                                    profile.PreviousConnectionId > x.PreviousConnectionId,
//                                                    out transfers);
//                                }
//                                else
//                                { // get the previous connection on the same.
//                                    profile = new Profile()
//                                    {
//                                        PreviousConnectionId = previousConnection.PreviousConnectionId,
//                                        Seconds = Constants.NoSeconds
//                                    };
//                                }
//                            }
//                        }
//                        else
//                        { // search for the best connection available.
//                            profile = First(profiles, x => x.Seconds != Constants.NoSeconds &&
//                                    x.Seconds <= previousConnection.DepartureTime &&
//                                    profile.PreviousConnectionId > x.PreviousConnectionId,
//                                    out transfers);
//                        }

//                        if(profile.PreviousConnectionId == Constants.NoConnectionId)
//                        { // the profile of the current stop indicates this is the source-stop.
//                            // insert the stop first with the departuretime of the previous connection.
//                            stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, new Profile()
//                            {
//                                PreviousConnectionId = Constants.NoConnectionId,
//                                Seconds = previousConnection.DepartureTime
//                            }));
//                            connections.Insert(0, null);
//                            stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, new Profile()
//                            {
//                                PreviousConnectionId = Constants.NoConnectionId,
//                                Seconds = profile.Seconds
//                            }));
//                            break;
//                        }
//                        else
//                        { // the profiles of the current stop indicate there is another connection before this one.
//                            stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, profile));
//                            connections.Insert(0, this.Algorithm.GetConnection(profile.PreviousConnectionId));
//                        }

//                        previousProfile = profile;
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
//                if (!_db.TryGetVertex(stops[0].Item1, out sourceStopVertex))
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
//                var targetRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.TargetSearch, targetStopVertex);
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
//                var bestVertex = this.Algorithm.GetBestNonTransitVertex();

//                // build route from sourcestop back to source location.
//                var sourceRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.SourceSearch, bestVertex);
//                var sourceRoute = sourceRouteBuilder.Build();

//                // build route from targetstop back to target location.
//                var targetRouteBuilder = new OneToManyDykstraRouteBuilder(_db.Graph, this.Algorithm.TargetSearch, bestVertex);
//                var targetRoute = targetRouteBuilder.Build();

//                return Route.Concatenate(sourceRoute, targetRoute);
//            }
//        }

//        /// <summary>
//        /// Returns the first element in the list that validates the predicate.
//        /// </summary>
//        /// <returns></returns>
//        private static T First<T>(List<T> list, Func<T, bool> predicate, out int index)
//        {
//            for (int i = 0; i < list.Count; i++)
//            {
//                if(predicate(list[i]))
//                {
//                    index = i;
//                    return list[i];
//                }
//            }
//            throw new System.InvalidOperationException("No element validates the predicate or source sequence is empty.");
//        }
//    }
//}