//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp.Math.Geo;
//using OsmSharp.Routing.Graph.Routing;
//using OsmSharp.Routing.Interpreter;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne;
//using OsmSharp.Routing.Transit.Multimodal.Algorithms.Resolving;
//using OsmSharp.Routing.Transit.Multimodal.Data;
//using OsmSharp.Routing.Vehicles;
//using System;

//namespace OsmSharp.Routing.Transit.Multimodal
//{
//    /// <summary>
//    /// A profile router using a CSA and Dykstra to calculate multimodal earliest arrival routes.
//    /// </summary>
//    public class ProfileSearchRouter : RouterBase
//    {
//        private readonly GeoCoordinate _sourceLocation;
//        private readonly GeoCoordinate _targetLocation;
//        private readonly int _maxSource;
//        private readonly int _maxTarget;
//        private readonly Vehicle _sourceVehicle;
//        private readonly Vehicle _targetVehicle;
//        private readonly MultimodalConnectionsDb _db;
//        private readonly IRoutingInterpreter _routingInterpreter;
//        private readonly DateTime _departureTime;
//        private readonly Func<float, float> _lazyness;

//        /// <summary>
//        /// Creates a new earliest arrival router.
//        /// </summary>
//        public ProfileSearchRouter(MultimodalConnectionsDb db, IRoutingInterpreter routingInterpreter, DateTime departureTime,
//            Vehicle sourceVehicle, GeoCoordinate source, Vehicle targetVehicle, GeoCoordinate target)
//        {
//            _db = db;
//            _routingInterpreter = routingInterpreter;
//            _departureTime = departureTime;
//            _sourceVehicle = sourceVehicle;
//            _sourceLocation = source;
//            _targetVehicle = targetVehicle;
//            _targetLocation = target;
//            _lazyness = null;
//            _maxSource = 10 * 60;
//            _maxTarget = 10 * 60;
//        }

//        /// <summary>
//        /// Creates a new earliest arrival router.
//        /// </summary>
//        public ProfileSearchRouter(MultimodalConnectionsDb db, IRoutingInterpreter routingInterpreter, DateTime departureTime,
//            Vehicle sourceVehicle, GeoCoordinate source, Vehicle targetVehicle, GeoCoordinate target, Func<float, float> lazyness)
//        {
//            _db = db;
//            _routingInterpreter = routingInterpreter;
//            _departureTime = departureTime;
//            _sourceVehicle = sourceVehicle;
//            _sourceLocation = source;
//            _targetVehicle = targetVehicle;
//            _targetLocation = target;
//            _lazyness = lazyness;
//            _maxSource = 10 * 60;
//            _maxTarget = 10 * 60;
//        }

//        /// <summary>
//        /// Creates a new earliest arrival router.
//        /// </summary>
//        public ProfileSearchRouter(MultimodalConnectionsDb db, IRoutingInterpreter routingInterpreter, DateTime departureTime,
//            Vehicle sourceVehicle, GeoCoordinate source, int maxSource, Vehicle targetVehicle, GeoCoordinate target, int maxTarget, 
//            Func<float, float> lazyness)
//        {
//            _db = db;
//            _routingInterpreter = routingInterpreter;
//            _departureTime = departureTime;
//            _sourceVehicle = sourceVehicle;
//            _sourceLocation = source;
//            _targetVehicle = targetVehicle;
//            _targetLocation = target;
//            _lazyness = lazyness;
//            _maxSource = maxSource;
//            _maxTarget = maxTarget;
//        }

//        private ProfileSearch _algorithm;

//        /// <summary>
//        /// Executes the actual run of the algorithm.
//        /// </summary>
//        protected override void DoRun()
//        {
//            // resolve coordinates.
//            var sourceResolver = new PathSegmentVisitListResolver(_db.Graph, _sourceVehicle);
//            var sourcePoint = sourceResolver.Resolve(_sourceLocation);
//            var source = sourceResolver.GetHook(sourcePoint);
//            var targetResolver = new PathSegmentVisitListResolver(_db.Graph, _targetVehicle);
//            var targetPoint = targetResolver.Resolve(_targetLocation);
//            var target = sourceResolver.GetHook(targetPoint);

//            // instantiate source search.
//            var sourceSearch = new OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany.OneToManyDykstra(
//                _db.Graph, _routingInterpreter, _sourceVehicle, source, _maxSource, false);
//            // instantiate target search.
//            var targetSearch = new OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany.OneToManyDykstra(
//                _db.Graph, _routingInterpreter, _targetVehicle, target, _maxTarget, true);

//            // instantiate earliest arrival search and run.
//            if (_lazyness == null)
//            {
//                _algorithm = new ProfileSearch(_db, _departureTime,
//                    sourceSearch, targetSearch);
//            }
//            else
//            {
//                _algorithm = new ProfileSearch(_db, _departureTime,
//                    sourceSearch, targetSearch, _lazyness);
//            }

//            _algorithm.Run();
//            if (_algorithm.HasSucceeded)
//            {
//                this.HasSucceeded = true;
//            }
//        }

//        /// <summary>
//        /// Builds the route.
//        /// </summary>
//        /// <returns></returns>
//        public Route BuildRoute()
//        {
//            var routeBuilder = new ProfileSearchRouteBuilder(_algorithm, _db);
//            return routeBuilder.Build();
//        }
//    }
//}