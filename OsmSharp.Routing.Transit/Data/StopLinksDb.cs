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
    /// A stop links db.
    /// </summary>
    public class StopLinksDb
    {
        private readonly ArrayBase<uint> _pointers; // holds the pointers/sizes.
        private readonly ArrayBase<uint> _data; // holds the actual links.

        /// <summary>
        /// Creates a new stop links db.
        /// </summary>
        public StopLinksDb()
        {
            _pointers = new MemoryArray<uint>(1024);
            _data = new MemoryArray<uint>(1024 * 2);
        }

        private uint _nextPointer = 0;

        /// <summary>
        /// Adds a new router point for the given stop.
        /// </summary>
        public void Add(uint stopId, RouterPoint point)
        {
            var pointerSize = _pointers.Length;
            var pointerStop = stopId * 2;
            while (pointerSize <= pointerStop)
            {
                pointerSize += 1024;
            }
            _pointers.Resize(pointerSize);

            // increase count or set pointer for the first time.
            if (_pointers[pointerStop + 0] == 0)
            { // set first pointer.
                _pointers[pointerStop + 0] = _nextPointer;
                _pointers[pointerStop + 1] = 1;
            }
            else if(_pointers[pointerStop + 0] + _pointers[pointerStop + 1] * 2 !=
                _nextPointer)
            { // invalid operation, can only add data to last added stop.
                throw new ArgumentException("Can only add stop links for the last added stop.");
            }
            else
            { // increase count.
                _pointers[pointerStop + 1] += 1;
            }

            // add data at the end.
            if(_nextPointer >= _data.Length)
            {
                _data.Resize(_data.Length + 1024);
            }
            _data[_nextPointer + 0] = point.EdgeId;
            _data[_nextPointer + 1] = point.Offset;
            _nextPointer += 2;
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
            private readonly StopLinksDb _db;

            internal Enumerator(StopLinksDb db)
            {
                _db = db;
            }

            private uint _count = uint.MaxValue;
            private uint _pointer = uint.MaxValue;
            private uint _position = uint.MaxValue;

            /// <summary>
            /// Moves this enumerator to the given id.
            /// </summary>
            public void MoveTo(uint id)
            {
                _pointer = _db._pointers[id * 2 + 0];
                _count = _db._pointers[id * 2 + 1];
                _position = uint.MaxValue;
            }

            /// <summary>
            /// Move to the next link.
            /// </summary>
            public bool MoveNext()
            {
                if (_position == uint.MaxValue)
                {
                    _position = 0;
                }
                else
                {
                    _position++;
                }
                return _count > _position;
            }

            /// <summary>
            /// Returns the current # links.
            /// </summary>
            public uint Count
            {
                get
                {
                    return _count;
                }
            }

            /// <summary>
            /// Gets the edge id.
            /// </summary>
            public uint EdgeId
            {
                get
                {
                    return _db._data[_pointer + (_position * 2) + 0];
                }
            }

            /// <summary>
            /// Gets the offset.
            /// </summary>
            public ushort Offset
            {
                get
                {
                    return (ushort)_db._data[_pointer + (_position * 2) + 1];
                }
            }
        }
    }
}