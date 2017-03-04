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

using System.IO;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Represents a database with potential transfers between stops.
    /// </summary>
    public class TransfersDb
    {
        private readonly Graphs.Graph _transfers; // Holds a graph with all transfers.

        /// <summary>
        /// Creates a new tranfers db.
        /// </summary>
        public TransfersDb(uint size)
        {
            _transfers = new Graphs.Graph(1, size);

            for(uint i = 0; i < size; i++)
            {
                _transfers.AddVertex(i);
            }
        }

        /// <summary>
        /// Creates a new transfers db.
        /// </summary>
        private TransfersDb(Graphs.Graph transfers)
        {
            _transfers = transfers;
        }

        /// <summary>
        /// Adds a new transfer.
        /// </summary>
        public void AddTransfer(uint stop1, uint stop2, int seconds)
        {
            _transfers.AddEdge(stop1, stop2, (uint)seconds);
        }

        /// <summary>
        /// Gets an enumerator for transfers.
        /// </summary>
        /// <returns></returns>
        public TransfersEnumerator GetTransferEnumerator()
        {
            return new TransfersEnumerator(_transfers.GetEdgeEnumerator());
        }

        /// <summary>
        /// A transfer enumerator.
        /// </summary>
        public class TransfersEnumerator
        {
            private readonly Graphs.Graph.EdgeEnumerator _enumerator;

            /// <summary>
            /// Creates a new enumerator.
            /// </summary>
            public TransfersEnumerator(Graphs.Graph.EdgeEnumerator enumerator)
            {
                _enumerator = enumerator;
            }

            /// <summary>
            /// Moves to the given stop.
            /// </summary>
            public bool MoveTo(uint stop)
            {
                return _enumerator.MoveTo(stop);
            }

            /// <summary>
            /// Returns true if this enumerator has data.
            /// </summary>
            public bool HasData
            {
                get
                {
                    return _enumerator.HasData;
                }
            }

            /// <summary>
            /// Moves to the next transfer.
            /// </summary>
            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            /// <summary>
            /// Gets the seconds.
            /// </summary>
            public uint Seconds
            {
                get
                {
                    return _enumerator.Data0;
                }
            }

            /// <summary>
            /// Gets the other stop.
            /// </summary>
            public uint Stop
            {
                get
                {
                    return _enumerator.To;
                }
            }
        }

        /// <summary>
        /// Serializes to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            return _transfers.Serialize(stream);
        }

        /// <summary>
        /// Deserializes from the given stream.
        /// </summary>
        public static TransfersDb Deserialize(Stream stream)
        {
            var graph = Graphs.Graph.Deserialize(stream, null);

            return new TransfersDb(graph);
        }
    }
}