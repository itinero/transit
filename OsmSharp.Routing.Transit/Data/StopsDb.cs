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

using OsmSharp.Collections.Sorting;
using OsmSharp.Math.Algorithms;
using OsmSharp.Routing.Algorithms.Search;
using Reminiscence.Arrays;
using System;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a stops db.
    /// </summary>
    public class StopsDb
    {
        private const int STOP_SIZE = 3; // one stop is 3 uints.
        private const int STOP_BLOCK_SIZE = 1000; // the stop block size.
        private readonly ArrayBase<uint> _stops; // holds all hilbert-sorted stops.

        /// <summary>
        /// Creates a new stops db.
        /// </summary>
        public StopsDb()
            : this(1024)
        {

        }

        /// <summary>
        /// Creates a new stops db.
        /// </summary>
        public StopsDb(int size)
        {
            _stops = new MemoryArray<uint>(size);
        }

        private uint _nextStopId = 0;

        /// <summary>
        /// Adds a new stop.
        /// </summary>
        public uint Add(float latitude, float longitude, uint metaId)
        {
            var id = _nextStopId;
            _nextStopId++;

            var size = _stops.Length;
            while ((id * STOP_SIZE + STOP_SIZE) > size)
            {
                size += STOP_BLOCK_SIZE;
            }
            if (size != _stops.Length)
            {
                _stops.Resize(size);
            }

            _stops[id * STOP_SIZE + 0] = StopsDb.Encode(latitude);
            _stops[id * STOP_SIZE + 1] = StopsDb.Encode(longitude);
            _stops[id * STOP_SIZE + 2] = metaId;

            return id;
        }

        /// <summary>
        /// Sorts the stops.
        /// </summary>
        public void Sort(Action<uint, uint> switchConnections)
        {
            if (_nextStopId > 0)
            { // sort stops, assume all stops are filled-in.
                QuickSort.Sort((stop) =>
                {
                    var latitude = StopsDb.DecodeSingle(_stops[stop * STOP_SIZE + 0]);
                    var longitude = StopsDb.DecodeSingle(_stops[stop * STOP_SIZE + 1]);
                    return HilbertCurve.HilbertDistance(latitude, longitude, Hilbert.DefaultHilbertSteps);
                },
                    (stop1, stop2) =>
                    {
                        var stop10 = _stops[stop1 * STOP_SIZE + 0];
                        var stop11 = _stops[stop1 * STOP_SIZE + 1];
                        var stop12 = _stops[stop1 * STOP_SIZE + 2];
                        _stops[stop1 * STOP_SIZE + 0] = _stops[stop2 * STOP_SIZE + 0];
                        _stops[stop1 * STOP_SIZE + 1] = _stops[stop2 * STOP_SIZE + 1];
                        _stops[stop1 * STOP_SIZE + 2] = _stops[stop2 * STOP_SIZE + 2];
                        _stops[stop2 * STOP_SIZE + 0] = stop10;
                        _stops[stop2 * STOP_SIZE + 1] = stop11;
                        _stops[stop2 * STOP_SIZE + 2] = stop12;

                        if (switchConnections != null)
                        {
                            switchConnections((uint)stop1, (uint)stop2);
                        }
                    }, 0, _nextStopId - 1);
            }
        }

        /// <summary>
        /// Returns the number of stops.
        /// </summary>
        public uint Count
        {
            get
            {
                return _nextStopId;
            }
        }

        /// <summary>
        /// Gets a stops enumerator.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_stops, _nextStopId);
        }

        /// <summary>
        /// A stop enumerator.
        /// </summary>
        public class Enumerator
        {
            private readonly ArrayBase<uint> _stops; // holds the stops-array.
            private readonly uint _count;

            internal Enumerator(ArrayBase<uint> stops, uint count)
            {
                _stops = stops;
                _count = count;
            }

            private uint _index = uint.MaxValue;

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = uint.MaxValue;
            }

            /// <summary>
            /// Moves this enumerator to the given stop.
            /// </summary>
            public bool MoveTo(uint id)
            {
                _index = id * STOP_SIZE;

                return id < _count;
            }

            /// <summary>
            /// Gets the latitude.
            /// </summary>
            public float Latitude
            {
                get
                {
                    return StopsDb.DecodeSingle(
                        _stops[_index + 0]);
                }
            }

            /// <summary>
            /// Gets the longitude.
            /// </summary>
            public float Longitude
            {
                get
                {
                    return StopsDb.DecodeSingle(
                        _stops[_index + 1]);
                }
            }

            /// <summary>
            /// Gets the meta-id.
            /// </summary>
            public uint MetaId
            {
                get
                {
                    return _stops[_index + 2];
                }
            }

            /// <summary>
            /// Gets the id.
            /// </summary>
            public uint Id
            {
                get
                {
                    return _index / STOP_SIZE;
                }
            }

            /// <summary>
            /// Moves to the next stop.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_index == uint.MaxValue)
                {
                    _index = 0;
                    return _count > 0;
                }
                _index += STOP_SIZE;

                return (_index / STOP_SIZE) < _count;
            }
        }

        /// <summary>
        /// Encodes a float into a uint.
        /// </summary>
        private static uint Encode(float latitude)
        {
            return System.BitConverter.ToUInt32(
                System.BitConverter.GetBytes(latitude), 0);
        }

        /// <summary>
        /// Encodes a float into a uint.
        /// </summary>
        private static float DecodeSingle(uint value)
        {
            return System.BitConverter.ToSingle(
                System.BitConverter.GetBytes(value), 0);
        }
    }
}
