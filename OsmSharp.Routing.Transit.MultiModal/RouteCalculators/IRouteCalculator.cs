// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Transit.Graphs;
using OsmSharp.Routing.Transit.MultiModal;
using System;

namespace OsmSharp.Routing.Transit.RouteCalculators
{
    /// <summary>
    /// Abstract representation of a multimodal route calculator.
    /// </summary>
    public interface IRouteCalculator
    {
        /// <summary>
        /// Calculates a transit route between the two given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="startTime"></param>
        /// <param name="isTripPossible"></param>
        /// <returns></returns>
        PathSegment<VertexTimeAndTrip> Calculate(IGraphReadOnly<LiveEdge> graph, uint from, uint to, DateTime startTime, Func<string, DateTime, bool> isTripPossible);
    }
}