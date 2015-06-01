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
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Transit.Multimodal.Data
{
    /// <summary>
    /// A database containing all transit-connections and an associated road network.
    /// </summary>
    public abstract class MultimodalConnectionsDbBase<TEdgeData>
        where TEdgeData : struct, IGraphEdgeData
    {
        /// <summary>
        /// Holds the connections-only db.
        /// </summary>
        protected GTFSConnectionsDb _connectionsDb;

        /// <summary>
        /// Holds the road network graph.
        /// </summary>
        private RouterDataSource<TEdgeData> _graph;

        /// <summary>
        /// Creates a new multimodal connections database.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="connectionsDb">The connections db.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicles">The vehicle profiles to support.</param>
        public MultimodalConnectionsDbBase(RouterDataSource<TEdgeData> graph, GTFSConnectionsDb connectionsDb, 
            IRoutingInterpreter interpreter, params Vehicle[] vehicles)
        {
            _graph = graph;
            _connectionsDb = connectionsDb;

            this.ConnectStops(interpreter, vehicles);
        }

        /// <summary>
        /// Gets the connections db.
        /// </summary>
        public GTFSConnectionsDb ConnectionsDb
        {
            get
            {
                return _connectionsDb;
            }
        }

        /// <summary>
        /// Connects the stops in the connection db with the graph.
        /// </summary>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicles">The vehicle profiles to connect the stops for.</param>
        public abstract void ConnectStops(IRoutingInterpreter interpreter, Vehicle[] vehicles);

        /// <summary>
        /// Returns true if the given vertex is a stop.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="stopId">The stop id of the stop at the vertex.</param>
        /// <returns></returns>
        public abstract bool TryGetStop(uint vertex, out int stopId);

        /// <summary>
        /// Returns true if the given stop has a vertex.
        /// </summary>
        /// <param name="stopId">The stop id.</param>
        /// <param name="vertex">The vertex of the stop.</param>
        /// <returns></returns>
        public abstract bool TryGetVertex(int stopId, out uint vertex);

        /// <summary>
        /// Gets the routing graph.
        /// </summary>
        public RouterDataSource<TEdgeData> Graph
        {
            get
            {
                return _graph;
            }
        }
    }
}