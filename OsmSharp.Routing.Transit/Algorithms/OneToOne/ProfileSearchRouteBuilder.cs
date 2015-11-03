﻿// OsmSharp - OpenStreetMap (OSM) SDK
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

using OsmSharp.Routing.Transit.Builders;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.UI;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// A class reponsable for building a profile search route.
    /// </summary>
    /// <remarks>This route builder uses raw GTFS data, we do not abstract away GTFS, convert other formats to GTFS.</remarks>
    public class ProfileSearchRouteBuilder : RouteBuilder<ProfileSearch>
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
        public ProfileSearchRouteBuilder(ProfileSearch earliestArrival, GTFSConnectionsDb connectionsDb)
            : base(earliestArrival)
        {
            _connectionsDb = connectionsDb;
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
            var profiles = this.Algorithm.GetStopProfiles(this.Algorithm.TargetStop);
            var profile = profiles.GetBest();
            stops.Insert(0, new Tuple<int, Profile>(
                this.Algorithm.TargetStop, profile));
            while (profile.ConnectionId >= 0)
            { // keep searching until the connection id < 0, meaning the start status, without a previous trip.
                // get connection information.
                var connection = this.Algorithm.GetConnection(profile.ConnectionId);
                profiles = this.Algorithm.GetStopProfiles(connection.DepartureStop);
                profile = profiles.GetBest(this.Algorithm, profile);
                if (profile.ConnectionId == Constants.NoConnectionId)
                { // this stop has no trip, this means that it is the first stop.
                    // insert the stop first with the departuretime of this connection.
                    var statusWithTrip = new Profile()
                    {
                        ConnectionId = Constants.NoConnectionId,
                        Seconds = connection.DepartureTime,
                        Transfers = 1
                    };
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, Profile>(
                        connection.DepartureStop, statusWithTrip));

                    // insert the first and final stop.
                    connections.Insert(0, null);
                    stops.Insert(0, new Tuple<int, Profile>(
                        connection.DepartureStop, profile));
                }
                else
                { // just insert as normal.
                    connections.Insert(0, connection);
                    stops.Insert(0, new Tuple<int, Profile>(
                        connection.DepartureStop, profile));
                }
            }

            // convert the stop and connection sequences into an actual route.
            var route = new Route();
            route.Segments = new List<RouteSegment>();

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
            route.Segments.Add(new RouteSegment()
            {
                Distance = -1, // distance is not important in transit routing.
                Latitude = (float)feedStop.Latitude,
                Longitude = (float)feedStop.Longitude,
                Time = 0,
                Tags = routeTags.ToArray()
            });
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

                    route.Segments.Add(new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = Constants.WaitVehicle // not an actual vehicle but just waiting.
                    });
                }
                else if (previousTripId != null || connection.Value.TripId == Constants.PseudoConnectionTripId)
                { // this is a transfer: current connection is null but there was a previous trip.
                    routeTags.Add(new RouteTags()
                    {
                        Key = "transit.timeofday",
                        Value = stops[idx].Item2.Seconds.ToInvariantString()
                    });

                    route.Segments.Add(new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = Constants.TransferVehicle // not an actual vehicle but just a transfer.
                    });
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

                    route.Segments.Add(new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)feedStop.Latitude,
                        Longitude = (float)feedStop.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = feedRoute.Type.ToVehicleUniqueName()
                    });
                }
            }

            return route;
        }
    }
}