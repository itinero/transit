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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Transit.Builders;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne
{
    /// <summary>
    /// A class reponsable for building a route.
    /// </summary>
    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
    public class ProfileSearchRouteBuilder : RouteBuilder<ProfileSearch>
    {
        private readonly MultimodalDb _db;

        /// <summary>
        /// Creates a new route builder.
        /// </summary>
        public ProfileSearchRouteBuilder(ProfileSearch profileSearch, MultimodalDb db)
            : base(profileSearch)
        {
            _db = db;
        }

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        public override Route DoBuild()
        {
            var stops = new List<Tuple<int, Profile>>();
            var connections = new List<Connection?>();

            // build the route backwards from the target stop.
            var bestTargetStop = this.Algorithm.GetBestTargetStop();
            var bestSourceStop = -1;

            // choose the best profile (for now this is the profile with the best arrival time).
            var profiles = this.Algorithm.GetStopProfiles(bestTargetStop);
            var transfers = profiles.Count - 1;
            var profile = profiles[transfers];
            stops.Insert(0, new Tuple<int, Profile>(bestTargetStop, profile));

            Connection previousConnection;
            if (profile.PreviousConnectionId != Constants.NoConnectionId)
            { // there is a connection to the target stop.
                previousConnection = this.Algorithm.GetConnection(profile.PreviousConnectionId);
                connections.Insert(0, previousConnection);

                var previousProfile = profile;
                while (true)
                { // keep looping until the best option is a profile without a previous connection.
                    previousConnection = connections[0].Value;

                    // choose the best option from the current profiles.
                    profiles = this.Algorithm.GetStopProfiles(previousConnection.DepartureStop);
                    if (profiles.Count > transfers &&
                        profiles[transfers].Seconds != Constants.NoSeconds)
                    { // there is a profile for the same amount of transfers.
                        var connection = this.Algorithm.GetConnection(profiles[transfers].PreviousConnectionId);
                        if (previousConnection.TripId == Constants.PseudoConnectionTripId)
                        { // the previous connection was a pseudo connection, choose a connection with?
                            // search for the best connection available (but don't take into account transfer time)
                            profile = First(profiles,
                                x => x.Seconds != Constants.NoSeconds &&
                                    x.Seconds <= previousConnection.DepartureTime &&
                                        previousConnection.PreviousConnectionId > x.PreviousConnectionId,
                                        out transfers);
                        }
                        else if (previousConnection.TripId == connection.TripId)
                        { // ok, this is it!
                            profile = profiles[transfers];
                        }
                        else
                        { // not the same trip. check if there is a previous connection and if the trip doesn't end here.
                            var previousTripStatus = this.Algorithm.GetTripStatus(previousConnection.TripId);
                            if (previousTripStatus.StopId == previousConnection.DepartureStop)
                            { // there is not previous connection on the same trip anymore.
                                // search for the best connection available.
                                profile = First(profiles,
                                        x => x.Seconds != Constants.NoSeconds &&
                                            x.Seconds <= previousConnection.DepartureTime &&
                                                profile.PreviousConnectionId > x.PreviousConnectionId,
                                                out transfers);
                            }
                            else
                            { // get the previous connection on the same.
                                profile = new Profile()
                                {
                                    PreviousConnectionId = previousConnection.PreviousConnectionId,
                                    Seconds = Constants.NoSeconds
                                };
                            }
                        }
                    }
                    else
                    { // search for the best connection available.
                        profile = First(profiles, x => x.Seconds != Constants.NoSeconds &&
                                x.Seconds <= previousConnection.DepartureTime &&
                                profile.PreviousConnectionId > x.PreviousConnectionId,
                                out transfers);
                    }

                    if (profile.PreviousConnectionId == Constants.NoConnectionId)
                    { // the profile of the current stop indicates this is the source-stop.
                        // insert the stop first with the departuretime of the previous connection.
                        stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, new Profile()
                        {
                            PreviousConnectionId = Constants.NoConnectionId,
                            Seconds = previousConnection.DepartureTime
                        }));
                        connections.Insert(0, null);
                        stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, new Profile()
                        {
                            PreviousConnectionId = Constants.NoConnectionId,
                            Seconds = profile.Seconds
                        }));
                        bestSourceStop = previousConnection.DepartureStop;
                        break;
                    }
                    else
                    { // the profiles of the current stop indicate there is another connection before this one.
                        stops.Insert(0, new Tuple<int, Profile>(previousConnection.DepartureStop, profile));
                        connections.Insert(0, this.Algorithm.GetConnection(profile.PreviousConnectionId));
                    }

                    previousProfile = profile;
                }
            }

            // convert the stop and connection sequences into an actual route.
            var transitRoute = new Route();
            transitRoute.Segments = new List<RouteSegment>(stops.Count);

            // get the first stop.
            var feedStop = _db.ConnectionsDb.GetGTFSStop(stops[0].Item1);
            var routeTags = new List<RouteTags>();
            feedStop.AddTagsTo(routeTags);
            routeTags.Add(new RouteTags()
            {
                Key = "transit.timeofday",
                Value = stops[0].Item2.Seconds.ToInvariantString()
            });
            var departureTime = stops[0].Item2.Seconds;
            transitRoute.Segments.Add(new RouteSegment()
            {
                Distance = 0,
                Latitude = (float)feedStop.Latitude,
                Longitude = (float)feedStop.Longitude,
                Time = 0,
                Tags = routeTags.ToArray()
            });
            var distance = 0.0;
            int? previousTripId = null;
            for (var idx = 1; idx < stops.Count; idx++)
            {
                var previousStopLocation = new GeoCoordinate(feedStop.Latitude, feedStop.Longitude);

                // get the next ...->connection->stop->... pair.
                var connection = connections[idx - 1];
                if (connection.HasValue &&
                    connection.Value.TripId == Constants.PseudoConnectionTripId)
                { // ignore this connection.
                    connection = null;
                }
                feedStop = _db.ConnectionsDb.GetGTFSStop(stops[idx].Item1);
                var stopLocation = new GeoCoordinate(feedStop.Latitude, feedStop.Longitude);
                distance = distance + previousStopLocation.DistanceReal(stopLocation).Value;

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

                    transitRoute.Segments.Add(new RouteSegment()
                    {
                        Distance = distance,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = OsmSharp.Routing.Transit.Constants.WaitProfile // not an actual vehicle but just waiting.
                    });
                }
                else if (previousTripId != null && connection == null)
                { // this is a transfer: current connection is null but there was a previous trip.
                    routeTags.Add(new RouteTags()
                    {
                        Key = "transit.timeofday",
                        Value = stops[idx].Item2.Seconds.ToInvariantString()
                    });

                    transitRoute.Segments.Add(new RouteSegment()
                    {
                        Distance = distance,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = OsmSharp.Routing.Transit.Constants.TransferProfile // not an actual vehicle but just a transfer.
                    });
                }
                else
                { // this is a connection: connection is not null.
                    // get route information.
                    var feedTrip = _db.ConnectionsDb.GetGTFSTrip(connection.Value.TripId);
                    var feedRoute = _db.ConnectionsDb.Feed.Routes.Get(feedTrip.RouteId);
                    var feedAgency = _db.ConnectionsDb.Feed.Agencies.Get(feedRoute.AgencyId);

                    feedTrip.AppendTagsTo(routeTags);
                    feedRoute.AppendTagsTo(routeTags);
                    feedAgency.AppendTagsTo(routeTags);
                    routeTags.Add(new RouteTags()
                    {
                        Key = "transit.timeofday",
                        Value = stops[idx].Item2.Seconds.ToInvariantString()
                    });

                    transitRoute.Segments.Add(new RouteSegment()
                    {
                        Distance = distance,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = feedRoute.Type.ToProfileName()
                    });
                }
            }

            // build route from source location to source stop.
            var sourcePath = this.Algorithm.SourceSearch.GetPath((uint)bestSourceStop);
            var sourceTargetPoint = this.Algorithm.SourceSearch.GetTargetPoint((uint)bestSourceStop);
            var sourceProfile = this.Algorithm.SourceSearch.Profile;
            var sourceSourcePoint = this.Algorithm.SourceSearch.SourcePoint;
            var sourceRoute = OsmSharp.Routing.Algorithms.RouteBuilder.Build(_db.RouterDb, sourceProfile,
                sourceSourcePoint, sourceTargetPoint, sourcePath);

            // build route from targetstop to target location.
            var targetPath = this.Algorithm.TargetSearch.GetPath((uint)bestTargetStop);
            var targetSourcePoint = this.Algorithm.TargetSearch.GetTargetPoint((uint)bestTargetStop);
            var targetProfile = this.Algorithm.TargetSearch.Profile;
            var targetTargetPoint = this.Algorithm.TargetSearch.SourcePoint;
            var targetRoute = OsmSharp.Routing.Algorithms.RouteBuilder.Build(_db.RouterDb, targetProfile,
                targetSourcePoint, targetTargetPoint, targetPath);

            // concatenate routes.
            var route = transitRoute;
            if (sourceRoute.Segments.Count > 1)
            { // route is more than just the source-stop.
                // add an extra segment to indicate a transfer to a transit vehicle.
                sourceRoute.Segments.Add(new RouteSegment()
                {
                    Distance = GeoCoordinate.DistanceEstimateInMeter(
                        sourceRoute.Segments[sourceRoute.Segments.Count - 1].Latitude,
                        sourceRoute.Segments[sourceRoute.Segments.Count - 1].Longitude,
                        transitRoute.Segments[0].Latitude,
                        transitRoute.Segments[0].Longitude) +
                        sourceRoute.Segments[sourceRoute.Segments.Count - 1].Distance,
                    Latitude = (float)transitRoute.Segments[0].Latitude,
                    Longitude = (float)transitRoute.Segments[0].Longitude,
                    Time = sourceRoute.Segments[sourceRoute.Segments.Count - 1].Time,
                    Profile = OsmSharp.Routing.Transit.Constants.TransferProfile // not an actual vehicle but just a transfer.
                });

                route = sourceRoute.Concatenate(transitRoute);
            }
            else if (sourceRoute.Segments.Count == 1)
            { // make sure the vehicle profile is set at the start point.
                route.Segments[0].Profile = sourceRoute.Segments[0].Profile;
            }
            if (targetRoute.Segments.Count > 1)
            { // route is more than just the target-stop.
                // add an extra segment to indicate a transfer from a transit vehicle.
                route.Segments.Add(new RouteSegment()
                {
                    Distance = GeoCoordinate.DistanceEstimateInMeter(
                        route.Segments[route.Segments.Count - 1].Latitude,
                        route.Segments[route.Segments.Count - 1].Longitude,
                        targetRoute.Segments[0].Latitude,
                        targetRoute.Segments[0].Longitude) +
                        route.Segments[route.Segments.Count - 1].Distance,
                    Latitude = (float)targetRoute.Segments[0].Latitude,
                    Longitude = (float)targetRoute.Segments[0].Longitude,
                    Time = route.Segments[route.Segments.Count - 1].Time,
                    Profile = OsmSharp.Routing.Transit.Constants.TransferProfile // not an actual vehicle but just a transfer.
                });

                route = route.Concatenate(targetRoute);
            }

            // add extra tags to route.
            routeTags.Clear();
            routeTags.Add(new RouteTags()
            {
                Key = "departuretime",
                Value = this.Algorithm.DepartureTime.ToString("{dd-MM-yyyy HH:mm:ss}")
            });
            route.Tags = routeTags;

            return route;
        }

        /// <summary>
        /// Returns the first element in the list that validates the predicate.
        /// </summary>
        /// <returns></returns>
        private static T First<T>(List<T> list, Func<T, bool> predicate, out int index)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    index = i;
                    return list[i];
                }
            }
            throw new System.InvalidOperationException("No element validates the predicate or source sequence is empty.");
        }
    }
}