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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Data
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
        public struct TransfersEnumerator
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
    }
}