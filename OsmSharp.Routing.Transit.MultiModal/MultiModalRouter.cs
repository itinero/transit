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

using GeoAPI.Geometries;
using GTFS;
using GTFS.Entities;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Graphs.Serialization;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams.Graphs;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using OsmSharp.Routing.Transit.MultiModal.Routers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <param name="from"></param>
        /// <param name="to"></param>
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

        /// <summary>
        /// Calculates the time to all the vertices within the given range.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="toFirstStop"></param>
        /// <param name="interModal"></param>
        /// <param name="fromLastStop"></param>
        /// <param name="from"></param>
        /// <param name="max"></param>
        /// <param name="sampleZoom"></param>
        /// <returns>A collection of tuples containing the sample position, sample id and sample value.</returns>
        public IEnumerable<Tuple<GeoCoordinate, ulong, double>> CalculateTransitWithin(DateTime departureTime, Vehicle toFirstStop, Vehicle interModal, Vehicle fromLastStop, RouterPoint from, double max, int sampleZoom)
        {
            return _multiModalRouter.CalculateAllWithin(departureTime, toFirstStop, interModal, fromLastStop, from, null, max, sampleZoom);
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

        #region Queries

        /// <summary>
        /// Returns the agency with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Agency GetAgency(string id)
        {
            return this._multiModalRouter.GetAgency(id);
        }

        /// <summary>
        /// Returns all agencies.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies()
        {
            return this._multiModalRouter.GetAgencies();
        }

        /// <summary>
        /// Returns the agencies that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies(string query)
        {
            return this._multiModalRouter.GetAgencies(query);
        }

        /// <summary>
        /// Returns all the stops.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops()
        {
            return this._multiModalRouter.GetStops();
        }

        /// <summary>
        /// Returns all the stops that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops(string query)
        {
            return this._multiModalRouter.GetStops(query);
        }

        /// <summary>
        /// Returns all stops for the given agency.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId)
        {
            return this._multiModalRouter.GetStopsForAgency(agencyId);
        }

        /// <summary>
        /// Returns all stops for the given agency that contain the words in the given query.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId, string query)
        {
            return this._multiModalRouter.GetStopsForAgency(agencyId, query);
        }

        #endregion

        #region Network & Route Geography

        /// <summary>
        /// Returns all network features that exist.
        /// </summary>
        /// <returns></returns>
        public FeatureCollection GetNeworkFeatures()
        {
            return _multiModalRouter.GetNetworkFeatures();
        }

        /// <summary>
        /// Returns all network features that exist (or overlap the edge) inside the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public FeatureCollection GetNeworkFeatures(GeoCoordinateBox box)
        {
            return _multiModalRouter.GetNetworkFeatures(box);
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route">The route to convert.</param>
        /// <param name="aggregated">Aggregate similar edge together.</param>
        /// <returns></returns>
        public FeatureCollection GetFeatures(Route route, bool aggregated)
        {
            return _multiModalRouter.GetFeatures(route, aggregated);
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

        /// <summary>
        /// Creates a new router from a PBF file.
        /// </summary>
        /// <param name="pbf"></param>
        /// <param name="interpreter"></param>
        /// <returns></returns>
        public static MultiModalRouter CreateFromPBF(string pbf, IOsmRoutingInterpreter interpreter)
        {
            using(var pbfStream = new FileInfo(pbf).OpenRead())
            {
                return MultiModalRouter.CreateFrom(new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(pbfStream), interpreter);
            }
        }

        #endregion
    }
}