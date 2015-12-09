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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Contains extension methods for the transit db.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Adds a transfers db.
        /// </summary>
        public static void AddTransfersDb(this TransitDb db, Profiles.Profile profile, TagsCollectionBase defaultProfile,
            float maxTimeInSeconds)
        {
            var transfersDb = new TransfersDb(db.StopsCount);
            var factor = profile.Factor(defaultProfile);

            // add all transfers.
            var enumerator1 = db.GetStopsEnumerator();
            while (enumerator1.MoveNext())
            {
                var enumerator2 = db.GetStopsEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (enumerator1.Id < enumerator2.Id)
                    {
                        var distance = GeoCoordinate.DistanceEstimateInMeter(enumerator1.Latitude, enumerator1.Longitude,
                            enumerator2.Latitude, enumerator2.Longitude);
                        var time = (int)System.Math.Round(distance * factor.Value, 0);
                        if (time < maxTimeInSeconds)
                        {
                            transfersDb.AddTransfer(enumerator1.Id, enumerator2.Id, time);
                        }
                    }
                }
            }

            db.AddTransfersDb(profile, transfersDb);
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddScheduleEntry(this TransitDb db, uint id, DateTime day)
        {
            db.AddScheduleEntry(id, day, day, day.Weekmask());
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddScheduleEntry(this TransitDb db, uint id, DateTime start, DateTime end,
            params DayOfWeek[] days)
        {
            if (days == null || days.Length == 0) { throw new ArgumentOutOfRangeException("days", "Cannot add empty week patterns."); }

            db.AddScheduleEntry(id, start, end, SchedulesDbExtensions.Weekmask(days));
        }

        /// <summary>
        /// The default search offset when linking stops.
        /// </summary>
        public const float DefaultSearchOffset = .01f;

        /// <summary>
        /// The default max distance when linking stops.
        /// </summary>
        public const float DefaultMaxDistance = 50;

        /// <summary>
        /// The default number of closest edges to use when linking stops.
        /// </summary>
        public const int DefaultMaxRouterPoints = 3;

        /// <summary>
        /// Adds a new stop links db for the given profile.
        /// </summary>
        public static void AddStopLinksDb(this TransitDb db, RouterDb routerDb, Profile profile, float searchOffset = DefaultSearchOffset,
            float maxDistance = DefaultMaxDistance, int maxRouterPoints = DefaultMaxRouterPoints)
        {
            var stopsDbEnumerator = db.GetStopsEnumerator();
            var linksDb = new StopLinksDb();

            while(stopsDbEnumerator.MoveNext())
            {
                var stopId = stopsDbEnumerator.Id;
                var multiResolver = new ResolveMultipleAlgorithm(routerDb.Network.GeometricGraph,
                    stopsDbEnumerator.Latitude, stopsDbEnumerator.Longitude, searchOffset, maxDistance, (edge) =>
                    {
                        // get profile.
                        float distance;
                        ushort edgeProfileId;
                        OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0],
                            out distance, out edgeProfileId);
                        var edgeProfile = routerDb.EdgeProfiles.Get(edgeProfileId);
                        // get factor from profile.
                        if (profile.Factor(edgeProfile).Value <= 0)
                        { // cannot be traversed by this profile.
                            return false;
                        }
                        // verify stoppable.
                        if (!profile.CanStopOn(edgeProfile))
                        { // this profile cannot stop on this edge.
                            return false;
                        }
                        return true;
                    });
                multiResolver.Run();
                if (multiResolver.HasSucceeded)
                {
                    // get the n-closest.
                    var closest = multiResolver.Results.GetLowestN(maxRouterPoints,
                        (p) => (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(stopsDbEnumerator.Latitude, stopsDbEnumerator.Longitude,
                            p.Latitude, p.Longitude));

                    // add them as new links.
                    for (var i = 0; i < closest.Count; i++)
                    {
                        linksDb.Add((uint)stopId, closest[i]);
                    }
                }
            }

            db.AddStopLinksDb(profile, linksDb);
        }

        /// <summary>
        /// Searches for the first stop with some tags or based on some condition.
        /// </summary>
        public static uint SearchFirstStopsWithTags(this TransitDb db,
            Func<TagsCollectionBase, bool> condition)
        {
            var stops = new HashSet<uint>();
            var enumerator = db.GetStopsEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var stopTags = db.StopAttributes.Get(enumerator.MetaId);
                if (condition(stopTags))
                {
                    return enumerator.Id;
                }
            }
            return Constants.NoStopId;
        }

        /// <summary>
        /// Searches the stops with some tags or based on some condition.
        /// </summary>
        public static HashSet<uint> SearchStopsWithTags(this TransitDb db,
            Func<TagsCollectionBase, bool> condition)
        {
            var stops = new HashSet<uint>();
            var enumerator = db.GetStopsEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var stopTags = db.StopAttributes.Get(enumerator.MetaId);
                if(condition(stopTags))
                {
                    stops.Add(enumerator.Id);
                }
            }
            return stops;
        }
    }
}