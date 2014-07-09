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
using GTFS.Entities;
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
        /// Holds all GTFS feeds.
        /// </summary>
        private List<GTFSFeed> _feeds = new List<GTFSFeed>();

        /// <summary>
        /// Adds a new GTFS feed to this router.
        /// </summary>
        /// <param name="feed"></param>
        public void AddGTFSFeed(GTFSFeed feed)
        {
            _feeds.Add(feed);

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
            foreach(var feed  in _feeds)
            {
                var agency = feed.GetAgency(id);
                if(agency != null)
                {
                    return agency;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all agencies.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies()
        {
            if(_feeds.Count > 0)
            {
                IEnumerable<Agency> agencies = _feeds[0].Agencies;
                for (int idx = 1; idx < _feeds.Count; idx++)
                {
                    agencies = agencies.Concat(_feeds[1].Agencies);
                }
                return agencies;
            }
            return new List<Agency>();
        }

        /// <summary>
        /// Returns the agencies that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies(string query)
        {
            return this.GetAgencies().Where(x => { return x.Name != null && x.Name.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) != -1; });
        }

        /// <summary>
        /// Returns all the stops.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops()
        {
            if (_feeds.Count > 0)
            {
                IEnumerable<Stop> stops = _feeds[0].Stops;
                for (int idx = 1; idx < _feeds.Count; idx++)
                {
                    stops = stops.Concat(_feeds[1].Stops);
                }
                return stops;
            }
            return new List<Stop>();
        }

        /// <summary>
        /// Returns all the stops that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops(string query)
        {
            return this.GetStops().Where(x => { return x.Name != null && x.Name.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) != -1; });
        }

        /// <summary>
        /// Returns all stops for the given agency.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId)
        {
            return new List<Stop>();
        }

        /// <summary>
        /// Returns all stops for the given agency that contain the words in the given query.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId, string query)
        {
            return new List<Stop>();
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