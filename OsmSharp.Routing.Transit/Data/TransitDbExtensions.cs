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
using System;

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
    }
}