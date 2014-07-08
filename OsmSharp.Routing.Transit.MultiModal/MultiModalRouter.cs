// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using GTFS;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Graphs.Serialization;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams.Graphs;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using OsmSharp.Routing.Transit.MultiModal.Routers;
using System;
using System.IO;

namespace OsmSharp.Routing.Transit.MultiModal
{
    /// <summary>
    /// A version of the typedrouter using edges of type LiveEdge.
    /// </summary>
    public class MultiModalRouter : Router
    {
        /// <summary>
        /// Holds the multimodal router.
        /// </summary>
        private TypedRouterMultiModal _multiModalRouter;

        /// <summary>
        /// Creates a multimodal router.
        /// </summary>
        /// <param name="router"></param>
        public MultiModalRouter(TypedRouterMultiModal router)
            : base(router)
        {
            _multiModalRouter = router;
        }

        /// <summary>
        /// Calculates a route between two given location that is possible multimodal.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="toFirstStop"></param>
        /// <param name="interModal"></param>
        /// <param name="fromLastStop"></param>
        /// <returns></returns>
        public Route CalculateTransit(DateTime departureTime, Vehicle toFirstStop, Vehicle interModal, Vehicle fromLastStop, RouterPoint from, RouterPoint to)
        {
            return _multiModalRouter.CalculateTransit(departureTime, toFirstStop, interModal, fromLastStop, from, to, null);
        }

        /// <summary>
        /// Calculates a transit route between two given stops leaving on or after the given departure time.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Route CalculateTransit(DateTime departureTime, string from, string to)
        {
            return _multiModalRouter.CalculateTransit(departureTime, from, to, null);
        }

        #region GFTS

        /// <summary>
        /// Adds a new GTFS feed to this router.
        /// </summary>
        /// <param name="feed"></param>
        public void AddGTFSFeed(GTFSFeed feed)
        {
            this._multiModalRouter.AddGTFSFeed(feed);
        }

        #endregion

        #region Static Creation Methods

        /// <summary>
        /// Creates a new router.
        /// </summary>
        /// <param name="reader">The OSM-stream reader.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static MultiModalRouter CreateFrom(OsmStreamSource reader, IOsmRoutingInterpreter interpreter)
        {
            var tagsIndex = new TagsTableCollectionIndex(); // creates a tagged index.

            // read from the OSM-stream.
            var memoryData = new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex);
            var targetData = new LiveGraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            targetData.RegisterSource(reader);
            targetData.Pull();

            // creates the live edge router.
            var multiModalEdgeRouter = new TypedRouterMultiModal(
                new MultiModalGraphRouterDataSource(memoryData), interpreter, new ReferenceCalculator());

            return new MultiModalRouter(multiModalEdgeRouter); // create the actual router.
        }

        /// <summary>
        /// Creates a new router.
        /// </summary>
        /// <param name="flatFile">The flatfile containing the graph.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static MultiModalRouter CreateFrom(Stream flatFile, IOsmRoutingInterpreter interpreter)
        {
            var serializer = new LiveEdgeFlatfileSerializer();
            var source = serializer.Deserialize(flatFile, false) as DynamicGraphRouterDataSource<LiveEdge>;

            // creates the live edge router.
            var multiModalEdgeRouter = new TypedRouterMultiModal(
                new MultiModalGraphRouterDataSource(source), interpreter, new ReferenceCalculator());

            return new MultiModalRouter(multiModalEdgeRouter); // create the actual router.
        }

        #endregion
    }
}