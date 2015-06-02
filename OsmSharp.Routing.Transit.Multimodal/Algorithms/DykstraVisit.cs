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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using System;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms
{
    /// <summary>
    /// Represents a dykstra edge.
    /// </summary>
    public class DykstraVisit
    {
        /// <summary>
        /// Creates a new dykstra vertex state for the last vertex in the given path.
        /// </summary>
        /// <param name="path"></param>
        public DykstraVisit(PathSegment<long> path)
        {
            this.Vertex = path.VertexId;
            this.Weight = (float)path.Weight;
            this.From = 0;
            if (path.From != null)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Creates a new dykstra vertex state.
        /// </summary>
        /// <param name="vertex">The vertex id.</param>
        public DykstraVisit(uint vertex)
        {
            this.Vertex = vertex;
            this.From = 0;
            this.Weight = 0;
        }

        /// <summary>
        /// Creates a new dykstra vertex state.
        /// </summary>
        /// <param name="vertex">The vertex id.</param>
        /// <param name="from">The from vertex id.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="edge">The edge.</param>
        /// <param name="coordinates">The coordinates</param>
        public DykstraVisit(long vertex, long from, float weight, Edge edge, ICoordinateCollection coordinates)
        {
            this.Vertex = vertex;
            this.From = from;
            this.Weight = weight;
            this.Edge = edge;
            this.Coordinates = coordinates;
        }

        /// <summary>
        /// The id of this vertex.
        /// </summary>
        public long Vertex;

        /// <summary>
        /// The if of the vertex right before this vertex.
        /// </summary>
        public long From;

        /// <summary>
        /// The weight to the current vertex.
        /// </summary>
        public float Weight;

        /// <summary>
        /// The edge.
        /// </summary>
        public Edge Edge;

        /// <summary>
        /// The coordinates.
        /// </summary>
        public ICoordinateCollection Coordinates;
    }
}