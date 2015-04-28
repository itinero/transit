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

using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Transit.Graphs;
using System;

namespace OsmSharp.Routing.Transit.RouteCalculators.CSA
{
    /// <summary>
    /// An implementation of a connection scan algorithm-based transit route calculator.
    /// </summary>
    public class CSACalculator : IRouteCalculator
    {
        /// <summary>
        /// Calculates a transit route between the two stops.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="startTime"></param>
        /// <param name="isTripPossible"></param>
        /// <returns></returns>
        public PathSegment<VertexTimeAndTrip> Calculate(IGraphReadOnly<TransitEdge> graph, uint from, uint to, DateTime startTime, Func<uint, DateTime, bool> isTripPossible)
        {
            throw new NotImplementedException();
        }
    }
}
