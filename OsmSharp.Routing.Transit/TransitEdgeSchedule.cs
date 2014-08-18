// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using System;
using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Represents the schedule for one transit edge.
    /// </summary>
    public class TransitEdgeSchedule
    {
        /// <summary>
        /// Holds all schedule entries.
        /// </summary>
        private List<TransitEdgeScheduleEntry> _entries;

        /// <summary>
        /// Creates a new transite edge schedule.
        /// </summary>
        public TransitEdgeSchedule()
        {
            _entries = new List<TransitEdgeScheduleEntry>();
        }

        /// <summary>
        /// Returns all entries in this schedule.
        /// </summary>
        public List<TransitEdgeScheduleEntry> Entries
        {
            get
            {
                return _entries;
            }
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        public void Add(uint trip, int departure, int arrival)
        {
            _entries.Add(new TransitEdgeScheduleEntry(trip, departure, arrival));
        }

        /// <summary>
        /// Returns true if the given entry exists in this schedule.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        /// <returns></returns>
        public bool Contains(uint trip, DateTime departure, DateTime arrival)
        {
            var entry = new TransitEdgeScheduleEntry(trip, departure, arrival);
            return _entries.Exists(x =>
            {
                return x.Trip == entry.Trip &&
                    x.DepartureTime == entry.DepartureTime &&
                    x.ArrivalTime == entry.ArrivalTime;
            });
        }

        /// <summary>
        /// Returns true if the given entry exists in this schedule.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        /// <returns></returns>
        public bool Contains(uint trip, int departure, int arrival)
        {
            var entry = new TransitEdgeScheduleEntry(trip, departure, arrival);
            return _entries.Exists(x => {
                return x.Trip == entry.Trip &&
                    x.DepartureTime == entry.DepartureTime &&
                    x.ArrivalTime == entry.ArrivalTime;
            });
        }

        /// <summary>
        /// Returns the number of entries in this schedule.
        /// </summary>
        public int Count
        {
            get
            {
                return _entries.Count;
            }
        }

        /// <summary>
        /// Returns all entries with a departure equal to or after the given time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public IEnumerable<TransitEdgeScheduleEntry> GetAfter(DateTime dateTime)
        {
            var earliestDeparture = dateTime.TimeOfDay.TotalSeconds;
            return _entries.Where(x => { return x.DepartureTime >= earliestDeparture; });
        }

        /// <summary>
        /// Returns the entry for the given trip.
        /// </summary>
        /// <param name="trip"></param>
        /// <returns></returns>
        public TransitEdgeScheduleEntry? GetForTrip(uint trip)
        {
            return _entries.Find(x => { return x.Trip == trip; });
        }

        /// <summary>
        /// Returns the entry for the given trip.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public TransitEdgeScheduleEntry? GetForTrip(uint trip, DateTime start)
        {
            int startIdx = this.FindEntryRightOnOrBefore((uint)start.TimeOfDay.TotalSeconds);
            for (int idx = startIdx; idx < _entries.Count; idx++)
            {
                if(_entries[idx].Trip == trip)
                {
                    return _entries[idx];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the first entry available after the given time.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="trip"></param>
        /// <returns></returns>
        public TransitEdgeScheduleEntry? GetNext(DateTime dateTime, uint trip)
        {
            var earliestDeparture = dateTime.TimeOfDay.TotalSeconds;
            TransitEdgeScheduleEntry? earliest = null;
            int startIdx = this.FindEntryRightOnOrBefore((uint)earliestDeparture);
            for (int idx = startIdx; idx < _entries.Count; idx++)
            {
                var entry = _entries[idx];
                if (entry.DepartureTime >= earliestDeparture &&
                    entry.Trip != trip)
                { // departure is definetly on or after earliest time and from a different trip.
                    if (earliest == null)
                    { // pick this one, if valid.
                        earliest = entry;
                    }
                    else if (entry.DepartureTime <= earliest.Value.DepartureTime)
                    { // this entry is earlier.
                        if (entry.DepartureTime == earliest.Value.DepartureTime)
                        { // same times.
                            if (entry.Trip.CompareTo(earliest.Value.Trip) > 0)
                            { // trip on entry is considered 'smaller'.
                                earliest = entry;
                            }
                        }
                        earliest = entry;
                    }
                }
            }
            return earliest;
        }

        /// <summary>
        /// Searches for the entry right on or after the given timestamp.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private int FindEntryRightOnOrBefore(uint seconds)
        {
            int minIdx = 0;
            int maxIdx = _entries.Count - 1;
            while(maxIdx - minIdx > 1)
            {
                int middleIdx = (int)((maxIdx - minIdx) / 2.0) + minIdx;
                var entry = _entries[middleIdx];
                if(entry.DepartureTime >= seconds)
                {
                    maxIdx = middleIdx;
                }
                else
                {
                    minIdx = middleIdx;
                }
            }
            return minIdx;
        }
    }

    /// <summary>
    /// Represents a schedule entry, a departure and arrival time.
    /// </summary>
    public struct TransitEdgeScheduleEntry : IComparable<TransitEdgeScheduleEntry>
    {
        /// <summary>
        /// Creates a transit edge schedule entry from two date times.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        public TransitEdgeScheduleEntry(uint trip, DateTime departure, DateTime arrival)
            : this()
        {
            this.Trip = trip;
            this.DepartureTime = (uint)departure.TimeOfDay.TotalSeconds;
            this.ArrivalTime = (uint)arrival.TimeOfDay.TotalSeconds;
        }

        /// <summary>
        /// Creates a transit edge schedule entry from two date times.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        public TransitEdgeScheduleEntry(uint trip, int departure, int arrival)
            : this()
        {
            this.Trip = trip;
            this.DepartureTime = (uint)departure;
            this.ArrivalTime = (uint)arrival;
        }

        /// <summary>
        /// Gets or sets the trip.
        /// </summary>
        public uint Trip { get; set; }

        /// <summary>
        /// Gets or sets the departure time in seconds since the beginning of the day.
        /// </summary>
        public uint DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the arrival time in seconds since the end of the day.
        /// </summary>
        public uint ArrivalTime { get; set; }

        /// <summary>
        /// Returns the duration in seconds (max 24h).
        /// </summary>
        public uint Duration
        {
            get
            {
                if (this.ArrivalTime > this.DepartureTime)
                { // no midnight in between!
                    return this.ArrivalTime - this.DepartureTime;
                }
                return this.ArrivalTime + ((24 * 60 * 60) - this.DepartureTime);
            }
        }

        /// <summary>
        /// Returns the time difference between the given time and the departure time (max 24h).
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int DepartsIn(DateTime time)
        {
            var earliestDeparture = (int)time.TimeOfDay.TotalSeconds;
            if (this.DepartureTime >= earliestDeparture)
            { // no midnight in between!
                return (int)(this.DepartureTime - earliestDeparture);
            }
            return (int)(earliestDeparture + ((24 * 60 * 60) - this.DepartureTime));
        }

        /// <summary>
        /// Returns a representation of this entry in text.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{3}@{0}->{1} ({2})",
                (new TimeSpan(0, 0, (int)this.DepartureTime)).ToInvariantString(),
                (new TimeSpan(0, 0, (int)this.ArrivalTime)).ToInvariantString(),
                (new TimeSpan(0, 0, (int)this.Duration)).ToInvariantString(),
                this.Trip);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(TransitEdgeScheduleEntry other)
        {
            return this.DepartureTime.CompareTo(other.DepartureTime);
        }
    }
}