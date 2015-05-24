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

using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Transit.Test.Data;
using System.Collections.Generic;

namespace OsmSharp.Transit.Test.Multimodal.Data
{
    /// <summary>
    /// A stub connections db.
    /// </summary>
    public class StubMultimodalConnectionsDb : MultimodalConnectionsDbBase<Edge>
    {
        /// <summary>
        /// Creates a new multimodal connections db.
        /// </summary>
        public StubMultimodalConnectionsDb()
            : base(new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex()), new StubConnectionsDb(),
                new OsmRoutingInterpreter(), Vehicle.Pedestrian)
        {
            this.Stops = new Dictionary<uint, int>();
        }

        /// <summary>
        /// Returns the connections db stub.
        /// </summary>
        public StubConnectionsDb StubConnectionsDb
        {
            get
            {
                return (StubConnectionsDb)_connectionsDb;
            }
        }

        /// <summary>
        /// Gets or sets the stop-id per vertex.
        /// </summary>
        public Dictionary<uint, int> Stops { get; private set; }

        /// <summary>
        /// Connect stops.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="vehicles"></param>
        public override void ConnectStops(IRoutingInterpreter interpreter, Vehicle[] vehicles)
        {

        }

        /// <summary>
        /// Returns true if the given vertex is a stop.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public override bool IsStop(uint vertex, out int stopId)
        {
            return this.Stops.TryGetValue(vertex, out stopId);
        }
    }
}