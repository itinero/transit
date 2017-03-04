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

using Reminiscence.Arrays;
using System;
using System.IO;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Represents a trip db.
    /// </summary>
    public class TripsDb
    {
        private const int SIZE = 3; // one trip is 3 uints.
        private const int BLOCK_SIZE = 1000; // the block size.
        private readonly ArrayBase<uint> _data; 

        /// <summary>
        /// Creates a new trips db.
        /// </summary>
        public TripsDb()
            : this(1024)
        {

        }

        /// <summary>
        /// Creates a new trips db.
        /// </summary>
        public TripsDb(int size)
        {
            _data = new MemoryArray<uint>(size);
        }

        /// <summary>
        /// Creates a new trips db.
        /// </summary>
        private TripsDb(ArrayBase<uint> data)
        {
            _nextId = (uint)(data.Length / SIZE);
            _data = data;
        }

        private uint _nextId = 0;

        /// <summary>
        /// Adds a new trip.
        /// </summary>
        public uint Add(uint scheduleId, uint agencyId, uint metaId)
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

            _data[id * SIZE + 0] = scheduleId;
            _data[id * SIZE + 1] = agencyId;
            _data[id * SIZE + 2] = metaId;

            return id;
        }

        /// <summary>
        /// Returns the number of trips.
        /// </summary>
        public uint Count
        {
            get
            {
                return _nextId;
            }
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
        /// Gets an enumerator.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_data, _nextId);
        }

        /// <summary>
        /// An enumerator.
        /// </summary>
        public class Enumerator
        {
            private readonly ArrayBase<uint> _data;
            private readonly uint _count;

            internal Enumerator(ArrayBase<uint> data, uint count)
            {
                _data = data;
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
            /// Moves this enumerator to the given trip.
            /// </summary>
            public bool MoveTo(uint id)
            {
                _index = id * SIZE;

                return id < _count;
            }

            /// <summary>
            /// Gets the scheduleId.
            /// </summary>
            public uint ScheduleId
            {
                get
                {
                    return _data[_index + 0];
                }
            }

            /// <summary>
            /// Gets the agencyId.
            /// </summary>
            public uint AgencyId
            {
                get
                {
                    return _data[_index + 1];
                }
            }

            /// <summary>
            /// Gets the metaId.
            /// </summary>
            public uint MetaId
            {
                get
                {
                    return _data[_index + 2];
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
        }

        /// <summary>
        /// Serializes this trips db to disk.
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
        /// Deserializes this trips db to disk.
        /// </summary>
        public static TripsDb Deserialize(Stream stream)
        {
            if (stream.ReadByte() != 1)
            {
                throw new Exception("Cannot deserialize stops db, version # doesn't match.");
            }

            var binaryReader = new BinaryReader(stream);
            var size = binaryReader.ReadInt64();

            var data = new MemoryArray<uint>(size * SIZE);
            data.CopyFrom(stream);
            return new TripsDb(data);
        }
    }
}
