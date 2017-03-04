// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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