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

using OsmSharp.Routing.Profiles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Data
{
    /// <summary>
    /// Represents a stop link db linking stops to edges.
    /// </summary>
    public class StopLinksDb
    {
        private readonly Dictionary<uint, Link<uint>> _stopsPerEdgeId;
        private readonly Link<RouterPoint>[] _routerPointsPerStop;

        /// <summary>
        /// Creates a new stop links db.
        /// </summary>
        public StopLinksDb(int stopCount)
        {
            _stopsPerEdgeId = new Dictionary<uint,Link<uint>>();
            _routerPointsPerStop = new Link<RouterPoint>[stopCount];
        }

        /// <summary>
        /// Adds a new link for the given given stop.
        /// </summary>
        public void Add(uint stopId, RouterPoint point)
        {
            // add routerpoint.
            _routerPointsPerStop[stopId] = new Link<RouterPoint>()
            {
                Next = _routerPointsPerStop[stopId],
                Item = point
            };

            // add stop id for edge id.
            Link<uint> stopIdLinkedList = null;
            _stopsPerEdgeId.TryGetValue(point.EdgeId, out stopIdLinkedList);
            _stopsPerEdgeId[point.EdgeId] = new Link<uint>()
            {
                Item = stopId,
                Next = stopIdLinkedList
            };
        }

        /// <summary>
        /// Returns the router points for the given stop.
        /// </summary>
        public Link<RouterPoint> Get(uint stopId)
        {
            return _routerPointsPerStop[stopId];
        }

        /// <summary>
        /// Tries to get a router point for the given edge id.
        /// </summary>
        public bool HasLink(uint edgeId)
        {
            return _stopsPerEdgeId.ContainsKey(edgeId);
        }

        /// <summary>
        /// Gets an enumerator for stop links per edge.
        /// </summary>
        /// <returns></returns>
        public StopLinksDbEnumerator GetEnumerator()
        {
            return new StopLinksDbEnumerator(this);
        }

        /// <summary>
        /// An enumerator for stop links.
        /// </summary>
        public class StopLinksDbEnumerator
        {
            private readonly StopLinksDb _stopLinksDb;

            internal StopLinksDbEnumerator(StopLinksDb stopLinksDb)
            {
                _stopLinksDb = stopLinksDb;
            }

            private uint _currentEdgeId = uint.MaxValue;
            private Link<uint> _stopLink = null;

            /// <summary>
            /// Moves this enumerator to the given edge id.
            /// </summary>
            public void MoveTo(uint edgeId)
            {
                _currentEdgeId = edgeId;
            }

            /// <summary>
            /// Move to the next stop/routerpoint pair.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_stopLink == null)
                {
                    return _stopLinksDb._stopsPerEdgeId.TryGetValue(_currentEdgeId, 
                        out _stopLink);
                }
                _stopLink = _stopLink.Next;
                return _stopLink != null;
            }

            /// <summary>
            /// Returns the stop id.
            /// </summary>
            public uint StopId
            {
                get
                {
                    return _stopLink.Item;
                }
            }

            /// <summary>
            /// Gets the router point.
            /// </summary>
            public RouterPoint RouterPoint
            {
                get
                {
                    var routerPointLink = _stopLinksDb._routerPointsPerStop[_stopLink.Item];
                    while(routerPointLink.Item.EdgeId != _currentEdgeId)
                    {
                        routerPointLink = routerPointLink.Next;
                    }
                    return routerPointLink.Item;
                }
            }
        }

        /// <summary>
        /// A linked list.
        /// </summary>
        public class Link<T>
        {
            /// <summary>
            /// Gets or sets the item.
            /// </summary>
            public T Item { get; set; }

            /// <summary>
            /// Gets or sets the next item.
            /// </summary>
            public Link<T> Next { get; set; }
        }
    }
}
