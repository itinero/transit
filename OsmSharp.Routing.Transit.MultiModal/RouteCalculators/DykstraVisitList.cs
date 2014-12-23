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

using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Transit.Graphs;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.MultiModal.RouteCalculators
{
    /// <summary>
    /// Dykstra visit list.
    /// </summary>
    public class DykstraVisitList
    {
        /// <summary>
        /// Holds the set of visited vertices.
        /// </summary>
        private HashSet<VertexTimeAndTrip> _visited;

        /// <summary>
        /// Holds the set of restricted vertices.
        /// </summary>
        private HashSet<long> _restricted;

        /// <summary>
        /// Holds the restricted visits.
        /// </summary>
        private Dictionary<long, HashSet<long>> _restrictedVisits;

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        public DykstraVisitList()
        {
            _visited = new HashSet<VertexTimeAndTrip>();
            _restricted = new HashSet<long>();
            _restrictedVisits = new Dictionary<long, HashSet<long>>();
        }

        /// <summary>
        /// Returns true if the given vertex has been visited already.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool HasBeenVisited(PathSegment<VertexTimeAndTrip> vertex)
        {
            if (vertex.From != null)
            { // there is a previous vertex, check it.
                return this.HasBeenVisited(vertex.VertexId, vertex.From.VertexId);
            }
            return _visited.Contains(vertex.VertexId);
        }

        /// <summary>
        /// Returns true if the given vertex has been visited already.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="fromVertex"></param>
        /// <returns></returns>
        public bool HasBeenVisited(VertexTimeAndTrip vertex, VertexTimeAndTrip fromVertex)
        {
            if (!_restricted.Contains(vertex.Vertex))
            { // not restricted.
                return _visited.Contains(vertex);
            }
            else
            { // check restricted.
                HashSet<long> froms;
                if (_restrictedVisits.TryGetValue(vertex.Vertex, out froms))
                {
                    return froms.Contains(fromVertex.Vertex);
                }
                return false;
            }
        }


        /// <summary>
        /// Sets the vertex as visited coming from the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public void SetVisited(PathSegment<VertexTimeAndTrip> vertex)
        {
            if (vertex.From != null)
            { // there is a previous vertex, check it.
                this.SetVisited(vertex.VertexId, vertex.From.VertexId);
            }
            else
            {
                _visited.Add(vertex.VertexId);
            }
        }

        /// <summary>
        /// Sets the vertex as visited coming from the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="fromVertex"></param>
        public void SetVisited(VertexTimeAndTrip vertex, VertexTimeAndTrip fromVertex)
        {
            if (!_restricted.Contains(vertex.Vertex))
            { // not restricted.
                _visited.Add(vertex);
            }
            else
            { // check restricted.
                HashSet<long> froms;
                if (!_restrictedVisits.TryGetValue(vertex.Vertex, out froms))
                {
                    froms = new HashSet<long>();
                    _restrictedVisits.Add(vertex.Vertex, froms);
                }
                froms.Add(fromVertex.Vertex);
            }
        }

        /// <summary>
        /// Sets the given vertex as restricted.
        /// </summary>
        /// <param name="vertex"></param>
        public void SetRestricted(long vertex)
        {
            _restricted.Add(vertex);
        }
    }
}