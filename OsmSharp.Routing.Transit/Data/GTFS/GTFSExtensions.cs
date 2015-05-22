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

using GTFS.Entities;
using GTFS.Entities.Enumerations;
using OsmSharp.UI;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data.GTFS
{
    /// <summary>
    /// Contains some extensions to GTFS objects that are OsmSharp-specific.
    /// </summary>
    public static class GTFSExtensions
    {
        /// <summary>
        /// Converts the given GTFS-route type into a vehicle name in the OsmSharp-space.
        /// </summary>
        /// <param name="routeType"></param>
        /// <returns></returns>
        public static string ToVehicleUniqueName(this RouteType routeType)
        {
            switch(routeType)
            {
                case RouteType.Bus:
                    return "Transit.Bus";
                case RouteType.CableCar:
                    return "Transit.CableCar";
                case RouteType.Ferry:
                    return "Transit.Ferry";
                case RouteType.Funicular:
                    return "Transit.Funicular";
                case RouteType.Gondola:
                    return "Transit.Gondola";
                case RouteType.Rail:
                    return "Transit.Rail";
                case RouteType.SubwayMetro:
                    return "Transit.SubwayMetro";
                case RouteType.Tram:
                    return "Transit.Tram";
            }
            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Adds relevant tags to the given tags list for this entity.
        /// </summary>
        /// <param name="agency"></param>
        /// <param name="routeTags"></param>
        public static void AppendTagsTo(this Agency agency, List<RouteTags> routeTags)
        {
            if (agency == null) { return; }

            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.agency.name",
                                Value = agency.Name
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.agency.id",
                                Value = agency.Id
                            });
        }

        /// <summary>
        /// Adds relevant tags to the given tags list for this entity.
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="routeTags"></param>
        public static void AddTagsTo(this global::GTFS.Entities.Stop stop, List<RouteTags> routeTags)
        {
            if (stop == null) { return; }

            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.stop.id",
                                Value = stop.Id
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.stop.name",
                                Value = stop.Name
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.stop.code",
                                Value = stop.Code
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.stop.description",
                                Value = stop.Description
                            });
        }

        /// <summary>
        /// Adds relevant tags to the given tags list for this entity.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="routeTags"></param>
        public static void AppendTagsTo(this Trip trip, List<RouteTags> routeTags)
        {
            if (trip == null) { return; }

            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.trip.id",
                                Value = trip.Id
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.trip.headsign",
                                Value = trip.Headsign
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.trip.shortname",
                                Value = trip.ShortName
                            });
        }

        /// <summary>
        /// Adds relevant tags to the given tags list for this entity.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="routeTags"></param>
        public static void AppendTagsTo(this global::GTFS.Entities.Route route, List<RouteTags> routeTags)
        {
            if (route == null) { return; }

            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.route.id",
                                Value = route.Id
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.route.shortname",
                                Value = route.ShortName
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.route.longname",
                                Value = route.LongName
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.route.textcolor",
                                Value = route.TextColor.HasValue ?
                                    SimpleColor.FromArgb(route.TextColor.Value).HexRgb : string.Empty
                            });
            routeTags.Add(new RouteTags()
                            {
                                Key = "transit.route.color",
                                Value = route.Color.HasValue ?
                                    SimpleColor.FromArgb(route.Color.Value).HexRgb : string.Empty
                            });
        }
    }
}
