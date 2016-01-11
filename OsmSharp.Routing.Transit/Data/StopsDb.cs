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
using System.IO;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a stops db.
    /// </summary>
    public class StopsDb
    {
        private const int SIZE = 3; // one stop is 3 uints.
        private const int BLOCK_SIZE = 1000; // the stop block size.
        private readonly ArrayBase<uint> _data; // holds all hilbert-sorted stops.

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
            _data = new MemoryArray<uint>(size);
        }

        /// <summary>
        /// Creates a new stops db.
        /// </summary>
        private StopsDb(ArrayBase<uint> data)
        {
            _nextId = (uint)(data.Length / SIZE);
            _data = data;
        }

        private uint _nextId = 0;

        /// <summary>
        /// Adds a new stop.
        /// </summary>
        public uint Add(float latitude, float longitude, uint metaId)
        {
            var id = _nextId;
            _nextId++;

            var size = _data.Length;
            while ((id * SIZE + SIZE) > size)
            {
                size += BLOCK_SIZE;
            }
            if (size != _data.Length)
            {
                _data.Resize(size);
            }

            _data[id * SIZE + 0] = StopsDb.Encode(latitude);
            _data[id * SIZE + 1] = StopsDb.Encode(longitude);
            _data[id * SIZE + 2] = metaId;

            return id;
        }

        /// <summary>
        /// Sorts the stops.
        /// </summary>
        public void Sort(Action<uint, uint> switchConnections)
        {
            if (_nextId > 0)
            { // sort stops, assume all stops are filled-in.
                QuickSort.Sort((stop) =>
                {
                    var latitude = StopsDb.DecodeSingle(_data[stop * SIZE + 0]);
                    var longitude = StopsDb.DecodeSingle(_data[stop * SIZE + 1]);
                    return HilbertCurve.HilbertDistance(latitude, longitude, Hilbert.DefaultHilbertSteps);
                },
                    (stop1, stop2) =>
                    {
                        var stop10 = _data[stop1 * SIZE + 0];
                        var stop11 = _data[stop1 * SIZE + 1];
                        var stop12 = _data[stop1 * SIZE + 2];
                        _data[stop1 * SIZE + 0] = _data[stop2 * SIZE + 0];
                        _data[stop1 * SIZE + 1] = _data[stop2 * SIZE + 1];
                        _data[stop1 * SIZE + 2] = _data[stop2 * SIZE + 2];
                        _data[stop2 * SIZE + 0] = stop10;
                        _data[stop2 * SIZE + 1] = stop11;
                        _data[stop2 * SIZE + 2] = stop12;

                        if (switchConnections != null)
                        {
                            switchConnections((uint)stop1, (uint)stop2);
                        }
                    }, 0, _nextId - 1);
            }
        }

        /// <summary>
        /// Returns the number of stops.
        /// </summary>
        public uint Count
        {
            get
            {
                return _nextId;
            }
        }

        /// <summary>
        /// Gets a stops enumerator.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_data, _nextId);
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
                _index = id * SIZE;

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
                    return _index / SIZE;
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
                _index += SIZE;

                return (_index / SIZE) < _count;
            }

            /// <summary>
            /// Returns the number of stops.
            /// </summary>
            public uint Count
            {
                get
                {
                    return _count;
                }
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
        
        /// <summary>
        /// Returns the size in bytes as if serialized.
        /// </summary>
        /// <returns></returns>
        public long SizeInBytes
        {
            get
            {
                return 1 + 8 + // the header: the length of the array and a version-byte.
                    ((long)_nextId * SIZE) * 4; // the bytes for the actual data.
            }
        }

        /// <summary>
        /// Serializes this stops db to disk.
        /// </summary>
        public long Serialize(Stream stream)
        {
            var position = stream.Position;
            stream.WriteByte(1); // write version #.

            var binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write((long)_nextId); // write size.
            // write data.
            for (var i = 0; i < (long)_nextId * SIZE; i++)
            {
                binaryWriter.Write(_data[i]);
            }
            return stream.Position - position;
        }

        /// <summary>
        /// Deserializes this stops db to disk.
        /// </summary>
        public static StopsDb Deserialize(Stream stream)
        {
            if(stream.ReadByte() != 1)
            {
                throw new Exception("Cannot deserialize stops db, version # doesn't match.");
            }

            var binaryReader = new BinaryReader(stream);
            var size = binaryReader.ReadInt64();

            var data = new MemoryArray<uint>(size * SIZE);
            data.CopyFrom(stream);
            return new StopsDb(data);
        }
    }
}
