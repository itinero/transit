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
//using OsmSharp.Routing.Algorithms.Default;
//using OsmSharp.Routing.Transit.Multimodal.Data;
//using System;

//namespace OsmSharp.Routing.Transit.Multimodal
//{
//    /// <summary>
//    /// An earliest arrival router using a CSA and Dykstra to calculate multimodal earliest arrival routes.
//    /// </summary>
//    public class EarliestArrivalRouter : RouterBase
//    {
//        private readonly Dykstra _sourceSearch;
//        private readonly Dykstra _targetSearch;
//        private readonly MultimodalDb _db;
//        private readonly DateTime _departureTime;
//        private readonly Func<float, float> _lazyness;

//        /// <summary>
//        /// Creates a new earliest arrival router.
//        /// </summary>
//        public EarliestArrivalRouter(MultimodalDb db, DateTime departureTime, Dykstra sourceSearch,
//            Dykstra targetSearch)
//        {
//            _db = db;
//            _departureTime = departureTime;
//            _sourceSearch = sourceSearch;
//            _targetSearch = targetSearch;
//            _lazyness = null;
//        }

//        /// <summary>
//        /// Creates a new earliest arrival router.
//        /// </summary>
//        public EarliestArrivalRouter(MultimodalDb db, DateTime departureTime, Dykstra sourceSearch,
//            Dykstra targetSearch, Func<float, float> lazyness)
//        {
//            _db = db;
//            _departureTime = departureTime;
//            _sourceSearch = sourceSearch;
//            _targetSearch = targetSearch;
//            _lazyness = lazyness;
//        }

//        private EarliestArrivalSearch _algorithm;

//        /// <summary>
//        /// Executes the actual run of the algorithm.
//        /// </summary>
//        protected override void DoRun()
//        {
//            // instantiate earliest arrival search and run.
//            if (_lazyness == null)
//            {
//                _algorithm = new EarliestArrivalSearch(_db, _departureTime,
//                   _sourceSearch, _targetSearch);
//            }
//            else
//            {
//                _algorithm = new EarliestArrivalSearch(_db, _departureTime,
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
//            var routeBuilder = new EarliestArrivalSearchRouteBuilder(_algorithm, _db);
//            return routeBuilder.Build();
//        }
//    }
//}