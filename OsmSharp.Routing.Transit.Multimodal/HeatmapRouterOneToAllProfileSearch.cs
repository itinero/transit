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
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.Resolving;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using System;

namespace OsmSharp.Routing.Transit.Multimodal
{
    /// <summary>
    /// A router to calculate a heatmap based on a one-to-all profile search.
    /// </summary>
    public class HeatmapRouterOneToAllProfileSearch : RouterBase
    {
        private readonly GeoCoordinate _sourceLocation;
        private readonly Vehicle _sourceVehicle;
        private readonly MultimodalConnectionsDbBase<Edge> _db;
        private readonly IRoutingInterpreter _routingInterpreter;
        private readonly double _max;
        private readonly int _zoom;
        private readonly DateTime _departureTime;

        /// <summary>
        /// Creates a new router.
        /// </summary>
        public HeatmapRouterOneToAllProfileSearch(MultimodalConnectionsDbBase<Edge> db, IRoutingInterpreter routingInterpreter,
            Vehicle sourceVehicle, GeoCoordinate source, DateTime departureTime, double max, int zoom)
        {
            _db = db;
            _routingInterpreter = routingInterpreter;
            _sourceLocation = source;
            _sourceVehicle = sourceVehicle;
            _max = max;
            _zoom = zoom;
            _departureTime = departureTime;
        }

        /// <summary>
        /// Gets the heatmap.
        /// </summary>
        public Builders.Heatmap Heatmap
        {
            get;
            private set;
        }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            this.Heatmap = null;

            // instantiate source search.
            var sourceResolver = new PathSegmentVisitListResolver(_db.Graph, _sourceVehicle);
            var sourcePoint = sourceResolver.Resolve(_sourceLocation);
            var source = sourceResolver.GetHook(sourcePoint);
            var sourceSearch = new OneToManyDykstra(
                _db.Graph, _routingInterpreter, _sourceVehicle, source, (int)_max, false);

            // instatiate one-to-all profile search.
            var oneToAll = new OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToAll.OneToAllProfileSearch(
                _db, _departureTime, sourceSearch, (int)_max);

            var heatmapSearch = new Builders.HeatmapBuilderAlgorithm<OneToManyDykstra>(
                sourceSearch, _zoom);
            heatmapSearch.Run();
            if (heatmapSearch.HasSucceeded)
            {
                this.HasSucceeded = true;
                this.Heatmap = heatmapSearch.Heatmap;
            }
        }
    }
}