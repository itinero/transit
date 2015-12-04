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

using Reminiscence.Arrays;
using System;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a schedules db.
    /// </summary>
    public class SchedulesDb
    {
        private const int BLOCK_SIZE = 1000; // the block size.
        private readonly ArrayBase<uint> _data;
        private DateTime REF_DATE = new DateTime(1970, 01, 01);

        /// <summary>
        /// Creates a new schedules db.
        /// </summary>
        public SchedulesDb()
            : this(1024)
        {

        }

        /// <summary>
        /// Creates a new schedules db.
        /// </summary>
        public SchedulesDb(int size)
        {
            _data = new MemoryArray<uint>(size);
        }

        private uint _nextPointer = 0;

        /// <summary>
        /// Adds a new schedule.
        /// </summary>
        public uint Add()
        {
            var id = _nextPointer;
            _nextPointer++;

            var size = _data.Length;
            if (id >= size)
            {
                _data.Resize(size + BLOCK_SIZE);
            }

            _data[id] = 0;

            return id;
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public void AddEntry(uint id, DateTime start, DateTime end, byte weekMask)
        {
            var count = _data[id];
            if (_nextPointer != id + count + 1)
            {
                throw new ArgumentException("Can only add schedule entries for the last added schedule.");
            }

            var pointer = _nextPointer;
            _nextPointer++;
            var size = _data.Length;
            if (pointer >= size)
            {
                _data.Resize(pointer + BLOCK_SIZE);
            }

            _data[pointer] = this.Encode(start, end, weekMask);
            _data[id] = count + 1;
        }

        /// <summary>
        /// Gets a new enumerator.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// An enumerator.
        /// </summary>
        public class Enumerator
        {
            private readonly SchedulesDb _db;

            internal Enumerator(SchedulesDb db)
            {
                _db = db;
            }

            private uint _count = uint.MaxValue;
            private uint _id = uint.MaxValue;

            /// <summary>
            /// Moves this enumerator to the given id.
            /// </summary>
            public bool MoveTo(uint id)
            {
                _count = _db._data[id];
                _id = id;
                if(_count >= (2^25))
                {
                    throw new InvalidOperationException("Attempt to move to a non-existing schedule id.");
                }
                return true;
            }

            /// <summary>
            /// Returns true if the given day is in the current schedule.
            /// </summary>
            public bool DateIsSet(DateTime day)
            {
                if (_count == uint.MaxValue) { throw new InvalidOperationException("No active schedule to check for dates. Move to a schedule first."); }

                var dayWeekMask = day.Weekmask();

                DateTime start;
                DateTime end;
                byte weekDays;
                for (var pointer = _id + 1; pointer < _id + 1 + _count; pointer++)
                {
                    _db.Decode(_db._data[pointer], out start, out end, out weekDays);
                    if (start <= day && end >= day && 
                       ((weekDays & dayWeekMask) == dayWeekMask))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Encodes the range and week mask.
        /// </summary>
        private uint Encode(DateTime start, DateTime end, byte weekMask)
        {
            if (weekMask == 0) { throw new ArgumentException("Cannot add empty entries."); }
            if (end < start) { throw new ArgumentException("Start date needs to be before or on end date."); }
            if (start < REF_DATE)
            {
                throw new ArgumentException(
                    string.Format("Cannot store date before {0}.", REF_DATE));
            }
            var startDays = (uint)(start.Date - REF_DATE).TotalDays;
            if (startDays >= (1 << 15))
            {
                throw new ArgumentException(
                    string.Format("Cannot store dates after {0}.", REF_DATE.AddDays((2 ^ 15) - 1)));
            }
            var intervalSize = (uint)(end.Date - start.Date).TotalDays;
            if (intervalSize >= (1 << 10))
            {
                throw new ArgumentException(
                    string.Format("Cannot store ranges bigger or equal to {0} days.", (1 << 10)));
            }

            return (uint)(weekMask << 25) + (intervalSize << 15) + startDays;
        }

        /// <summary>
        /// Decodes the range and week mask.
        /// </summary>
        private void Decode(uint value, out DateTime start, out DateTime end, out byte weekMask)
        {
            weekMask = (byte)(value >> 25);
            var intervalSize = (uint)(value << 7 >> 7 + 15);
            var startDays = (uint)(value << 17 >> 17);

            start = REF_DATE.AddDays(startDays);
            end = start.AddDays(intervalSize);
        }
    }
}