// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Contains extension methods related to the schedules db.
    /// </summary>
    public static class SchedulesDbExtensions
    {
        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddEntry(this SchedulesDb db, uint id, DateTime day)
        {
            db.AddEntry(id, day, day, day.Weekmask());
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddEntry(this SchedulesDb db, uint id, DateTime start, DateTime end, 
            params DayOfWeek[] days)
        {
            if (days == null || days.Length == 0) { throw new ArgumentOutOfRangeException("days", "Cannot add empty week patterns."); }

            db.AddEntry(id, start, end, Weekmask(days));
        }

        /// <summary>
        /// Generates a week mask for the given days.
        /// </summary>
        public static byte Weekmask(DayOfWeek[] dayOfWeek)
        {
            int mask = 0;
            for (var i = 0; i < dayOfWeek.Length; i++)
            {
                mask = mask | dayOfWeek[i].Weekmask();
            }
            return (byte)mask;
        }

        /// <summary>
        /// Generates a week mask for the given day.
        /// </summary>
        public static byte Weekmask(this DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 4;
                case DayOfWeek.Thursday:
                    return 8;
                case DayOfWeek.Friday:
                    return 16;
                case DayOfWeek.Saturday:
                    return 32;
                case DayOfWeek.Sunday:
                    return 64;
            }
            throw new InvalidOperationException("Day is not a day of the week.");
        }

        /// <summary>
        /// Generates a week mask for the given day.
        /// </summary>
        public static byte Weekmask(this DateTime day)
        {
            return day.DayOfWeek.Weekmask();
        }
    }
}