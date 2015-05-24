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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Transit.Multimodal.Data
{
    /// <summary>
    /// A database containing all transit-connections and an associated road network in the form of an unoptimized graph.
    /// </summary>
    public class MultimodalConnectionsDb : MultimodalConnectionsDbBase<Edge>
    {
        /// <summary>
        /// Holds the maximum distance a station access point can be from the actual station node.
        /// </summary>
        private static int MAX_ACCESS_POINT_DISTANCE = 100;

        /// <summary>
        /// Creates a new multimodal connnections db.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="connectionsDb">The connections db.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicles">The vehicle profiles to support.</param>
        public MultimodalConnectionsDb(RouterDataSource<Edge> graph, ConnectionsDb connectionsDb,
            IRoutingInterpreter interpreter, params Vehicle[] vehicles)
            :base(graph, connectionsDb, interpreter, vehicles)
        {

        }

        /// <summary>
        /// Connects the stops in the connection db with the graph.
        /// </summary>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicles">The vehicle profiles to connect the stops for.</param>
        public override void ConnectStops(IRoutingInterpreter interpreter, Vehicle[] vehicles)
        {
            // get all stops.
            var stops = this.GetStops();

            // create vertices for each of the stops.
            var graph = this.Graph;
            var nonTransitVertexCount = graph.VertexCount;
            for(int stopId = 0; stopId < stops.Count; stopId++)
            {
                var stop = stops[stopId];
                var stopVertex = graph.AddVertex(stop.Latitude, stop.Longitude);

                // keep stop location
                var stopLocation = new GeoCoordinate(stop.Latitude, stop.Longitude);

                // neighbouring vertices.
                var arcs = graph.GetEdges(new Math.Geo.GeoCoordinateBox(new Math.Geo.GeoCoordinate(stop.Latitude - 0.005, stop.Longitude - 0.0025),
                    new Math.Geo.GeoCoordinate(stop.Latitude + 0.005, stop.Longitude + 0.0025)));

                float latitude, longitude;
                foreach (var vehicle in vehicles)
                {
                    // keep a sorted list.
                    var closestVertices = new Dictionary<uint, double>();

                    // link this station to the road network for the current vehicle.
                    foreach (var arc in arcs)
                    {
                        bool isRoutable = arc.EdgeData.Tags != 0;
                        if (isRoutable)
                        { // the arc is already a road.
                            //if (graph.TagsIndex.Contains(arc.EdgeData.Tags))
                            //{ // there is a tags collection.
                                var tags = graph.TagsIndex.Get(arc.EdgeData.Tags);
                                isRoutable = interpreter.EdgeInterpreter.CanBeTraversedBy(tags, vehicle);
                            //}
                        }
                        if (isRoutable)
                        { // this arc is a road to keep it.
                            if (arc.Vertex1 != stopVertex &&
                                graph.GetVertex(arc.Vertex1, out latitude, out longitude))
                            { // check distance.
                                var keyDistance = stopLocation.DistanceEstimate(
                                    new GeoCoordinate(latitude, longitude)).Value;
                                closestVertices[arc.Vertex1] = keyDistance;
                            }
                            if (arc.Vertex2 != stopVertex &&
                                graph.GetVertex(arc.Vertex2, out latitude, out longitude))
                            { // check distance.
                                double keyDistance = stopLocation.DistanceEstimate(
                                    new GeoCoordinate(latitude, longitude)).Value;
                                closestVertices[arc.Vertex2] = keyDistance;
                            }
                        }
                    }

                    // sort vertices.
                    int max = 2;
                    var sorted = closestVertices.OrderBy(x => x.Value).GetEnumerator();
                    for (int idx = 0; idx < max; idx++)
                    {
                        if (sorted.MoveNext())
                        {
                            if (sorted.Current.Value < MAX_ACCESS_POINT_DISTANCE)
                            { // only attach stations that are relatively close.
                                var closest = sorted.Current.Key;
                                if (!graph.ContainsEdge(stopVertex, closest, new Edge()))
                                {
                                    graph.AddEdge(stopVertex, closest, new Edge(), null);
                                }
                                if (!graph.ContainsEdge(closest, stopVertex, new Edge()))
                                {
                                    graph.AddEdge(closest, stopVertex, new Edge(), null);
                                }
                            }
                        }
                        else
                        { // no more sorted vertices.
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given vertex is a stop.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="stopId">The stop id of the stop at the vertex.</param>
        /// <returns></returns>
        public override bool IsStop(uint vertex, out int stopId)
        {
            throw new System.NotImplementedException();
        }
    }
}