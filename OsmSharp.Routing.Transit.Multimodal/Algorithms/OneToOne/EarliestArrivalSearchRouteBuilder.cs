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

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne
{
    /// <summary>
    /// A class reponsable for building an earliest arrival route.
    /// </summary>
    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
    public class EarliestArrivalSearchRouteBuilder : RouteBuilder<EarliestArrivalSearch>
    {
        private readonly MultimodalDb _db;

        /// <summary>
        /// Creates a new earliest arrival route builder.
        /// </summary>
        /// <param name="earliestArrival">The earliest arrival algorithm.</param>
        /// <param name="db">The connection database.</param>
        public EarliestArrivalSearchRouteBuilder(EarliestArrivalSearch earliestArrival, MultimodalDb db)
            : base(earliestArrival)
        {
            _db = db;
        }

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        public override Route DoBuild()
        {
            var stops = new List<Tuple<int, EarliestArrivalSearch.StopStatus>>();
            var connections = new List<Connection?>();

            // build the route backwards from the target stop.
            var bestTargetStop = this.Algorithm.GetBestTargetStop();
            var bestSourceStop = -1;
            var status = this.Algorithm.GetStopStatus(bestTargetStop);
            stops.Insert(0, new Tuple<int, EarliestArrivalSearch.StopStatus>(bestTargetStop, status));
            while (status.ConnectionId >= 0)
            { // keep searching until the connection id < 0, meaning the start status, without a previous trip.
                // get connection information.
                var connection = this.Algorithm.GetConnection(status.ConnectionId);
                status = this.Algorithm.GetStopStatus(connection.DepartureStop);
                if (status.TripId < 0)
                { // this stop has no trip, this means that it is the first stop.
                    // insert the stop first with the departuretime of this connection.
                    var statusWithTrip = new EarliestArrivalSearch.StopStatus()
                    {
                        ConnectionId = Constants.NoConnectionId,
                        Seconds = connection.DepartureTime,
                        Transfers = 1,
                        TripId = connection.TripId
                    };
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, EarliestArrivalSearch.StopStatus>(
                        connection.DepartureStop, statusWithTrip));

                    // insert the first and final stop.
                    connections.Insert(0, null);
                    stops.Insert(0, new Tuple<int, EarliestArrivalSearch.StopStatus>(
                        connection.DepartureStop, status));
                    bestSourceStop = connection.DepartureStop;
                }
                else
                { // just insert as normal.
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, EarliestArrivalSearch.StopStatus>(
                        connection.DepartureStop, status));
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
    }
}