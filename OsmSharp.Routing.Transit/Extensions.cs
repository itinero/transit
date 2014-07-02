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
using System;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Contains extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the one and only arc between the two given vertices.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static TEdgeData GetArc<TEdgeData>(this IDynamicGraphReadOnly<TEdgeData> graph, uint from, uint to)
            where TEdgeData : IDynamicGraphEdgeData
        {
            var arcs = graph.GetArcs(from);
            foreach (var arc in arcs)
            {
                if (arc.Key == to)
                {
                    return arc.Value;
                }
            }
            throw new ArgumentOutOfRangeException("No arc found between the two given vertices.");
        }
    }
}
