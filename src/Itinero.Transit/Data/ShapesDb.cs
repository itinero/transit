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

using Itinero.Algorithms.Sorting;
using Itinero.Graphs.Geometric.Shapes;
using Reminiscence.Arrays;
using System;
using System.IO;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// A shapes database linking each pair of stops to a shape.
    /// </summary>
    public sealed class ShapesDb
    {
        // 1: stop1.
        // 2: stop2.
        // 3: shapeid
        private readonly ArrayBase<uint> _index;
        private readonly ShapesArray _shapes;

        /// <summary>
        /// Creates a new shape db.
        /// </summary>
        public ShapesDb()
        {
            _shapes = new ShapesArray(1024);
            _index = new MemoryArray<uint>(1024);
        }

        /// <summary>
        /// Creates a new shape db.
        /// </summary>
        private ShapesDb(long nextIdx, ArrayBase<uint> index, ShapesArray shapes)
        {
            _nextIdx = nextIdx;
            _index = index;
            _shapes = shapes;
        }

        private long _nextIdx = 0;
        private long _nextShapeId = 0;

        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public void Add(uint stop1, uint stop2, ShapeBase shape)
        {
            // add shape.
            if (_nextShapeId >= _shapes.Length)
            {
                _shapes.Resize(_shapes.Length + 1024);
            }
            _shapes[_nextShapeId] = shape;
            var shapeId = _nextShapeId;
            _nextShapeId++;

            if (_nextIdx + 3 >= _index.Length)
            {
                _index.Resize(_index.Length + 1024);
            }

            // add index.
            _index[_nextIdx + 0] = stop1;
            _index[_nextIdx + 1] = stop2;
            _index[_nextIdx + 2] = (uint)shapeId;
            _nextIdx += 3;
        }

        /// <summary>
        /// Gets a shape. 
        /// </summary>
        public ShapeBase Get(uint stop1, uint stop2)
        {
            var idx = this.TryGetIndex(stop1, stop2);
            if (idx == long.MaxValue)
            {
                return null;
            }
            return _shapes[_index[idx * 3 + 2]];
        }

        /// <summary>
        /// Sorts the index, making it searchable.
        /// </summary>
        public void Sort()
        {
            QuickSort.Sort((idx) =>
            {
                var actualIdx = idx * 3;

                return (long)_index[actualIdx + 0] * uint.MaxValue + (long)_index[actualIdx + 1];
            },
            (idx1, idx2) =>
            {
                var actualIdx1 = idx1 * 3;
                var actualIdx2 = idx2 * 3;

                var stop1 = _index[actualIdx1 + 0];
                var stop2 = _index[actualIdx1 + 1];
                var shapeId = _index[actualIdx1 + 2];
                _index[actualIdx1 + 0] = _index[actualIdx2 + 0];
                _index[actualIdx1 + 1] = _index[actualIdx2 + 1];
                _index[actualIdx1 + 2] = _index[actualIdx2 + 2];
                _index[actualIdx2 + 0] = stop1;
                _index[actualIdx2 + 1] = stop2;
                _index[actualIdx2 + 2] = shapeId;
            }, 0, (_nextIdx / 3) - 1);
        }


        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public long TryGetIndex(uint stop1, uint stop2)
        {
            if (_nextIdx == 0)
            {
                return long.MaxValue;
            }

            var idx = _nextIdx / 3;
            
            // do a binary search.
            long bottom = 0;
            uint bottom1Id = _index[bottom * 3 + 0];
            uint bottom2Id = _index[bottom * 3 + 1];
            if (bottom1Id == stop1 &&
                bottom2Id == stop2)
            {
                return bottom;
            }
            long top = idx - 1;
            uint top1Id = _index[top * 3 + 0];
            uint top2Id = _index[top * 3 + 1];
            if (top1Id == stop1 &&
                top2Id == stop2)
            {
                return top;
            }

            while (top - bottom > 1)
            {
                var middle = (((top - bottom) / 2) + bottom);
                uint middle1Id = _index[middle * 3 + 0];
                uint middle2Id = _index[middle * 3 + 1];
                if (middle1Id == stop1 &&
                    middle2Id == stop2)
                {
                    return middle;
                }
                if (middle1Id > stop1 || (middle1Id == stop1 &&
                    middle2Id > stop2))
                {
                    top = middle;
                }
                else
                {
                    bottom = middle;
                }
            }

            return long.MaxValue;
        }

        /// <summary>
        /// Serializes to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            _index.Resize(_nextIdx);
            _shapes.Resize(_nextShapeId);

            var position = stream.Position;
            stream.WriteByte(1); // write version #.

            // write size.
            var bytes = BitConverter.GetBytes(_nextIdx);
            stream.Write(bytes, 0, 8);

            // write index.
            _index.CopyTo(stream);

            // write shapes.
            _shapes.CopyTo(stream);

            return stream.Position - position;
        }

        /// <summary>
        /// Deserializes from the given stream.
        /// </summary>
        public static ShapesDb Deserialize(Stream stream)
        {
            var version = stream.ReadByte();
            if (version != 1)
            {
                throw new Exception("Cannot deserialize stops db, version # doesn't match.");
            }

            // read size.
            var sizeBytes = new byte[8];
            stream.Read(sizeBytes, 0, 8);
            var size = BitConverter.ToInt64(sizeBytes, 0);

            // read index.
            var index = new MemoryArray<uint>(size);
            index.CopyFrom(stream);

            // read shapes.
            var shapes = ShapesArray.CreateFrom(stream, true);

            return new ShapesDb(size, index, shapes);
        }
        
        /// <summary>
        /// Returns the number of shapes.
        /// </summary>
        public long Count
        {
            get
            {
                return _nextIdx / 3;
            }
        }

        /// <summary>
        /// Updates the stops at the position of the enumerator.
        /// </summary>
        public void UpdateStops(Enumerator position, uint stop1, uint stop2)
        {
            var pointer = position.Pointer;
            this._index[pointer * 3 + 0] = stop1;
            this._index[pointer * 3 + 1] = stop2;
        }

        /// <summary>
        /// Gets a the enumerator.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// The enumerator.
        /// </summary>
        public class Enumerator
        {
            private readonly ShapesDb _shapes;

            internal Enumerator(ShapesDb shapes)
            {
                _shapes = shapes;
            }

            private long _pointer = uint.MaxValue;

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                _pointer = uint.MaxValue;
            }
            
            /// <summary>
            /// Gets the pointer.
            /// </summary>
            internal long Pointer
            {
                get
                {
                    return _pointer;
                }
            }
                        
            /// <summary>
            /// Gets the stop1.
            /// </summary>
            public uint Stop1
            {
                get
                {
                    return _shapes._index[_pointer * 3 + 0];
                }
            }

            /// <summary>
            /// Gets the stop2.
            /// </summary>
            public uint Stop2
            {
                get
                {
                    return _shapes._index[_pointer * 3 + 1];
                }
            }

            /// <summary>
            /// Gets the shape.
            /// </summary>
            public ShapeBase Shape
            {
                get
                {
                    return _shapes._shapes[_shapes._index[_pointer * 3 + 2]];
                }
            }

            /// <summary>
            /// Moves to the next stop.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_pointer == uint.MaxValue)
                {
                    _pointer = 0;
                    return _shapes.Count > 0;
                }
                _pointer++;

                return _pointer < _shapes.Count;
            }
        }
    }
}