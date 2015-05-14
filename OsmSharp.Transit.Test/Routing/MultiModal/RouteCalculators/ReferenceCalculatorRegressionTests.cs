//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2014 Abelshausen Ben
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

//using NUnit.Framework;
//using OsmSharp.Math.Geo;
//using OsmSharp.Osm.Xml.Streams;
//using OsmSharp.Routing;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Transit.MultiModal;
//using System.Collections.Generic;
//using System.IO;
//using System.Reflection;

//namespace OsmSharp.Transit.Test.Routing.MultiModal.RouteCalculators
//{
//    /// <summary>
//    /// Contains regression tests for bugs fixed related to routing.
//    /// </summary>
//    public class ReferenceCalculatorRegressionTests
//    {
//        /// <summary>
//        /// An issue with double paths: 
//        /// </summary>
//        public void RegressionTest1()
//        {
//            double e = 0.1; // 10 cm

//            // network:
//            //        x(1)------x(2)
//            //       /           \
//            // x(3)--x(4)---------x(5)--x(6)

//            // 1: 50.98508962508, 4.82958530756
//            // 2: 50.98509255957, 4.83009340615
//            // 3: 50.98496931078, 4.82889075077
//            // 4: 50.98496931078, 4.82939884936
//            // 5: 50.98496931078, 4.83025189562
//            // 6: 50.98496931078, 4.83079728585

//            var vertex1 = new GeoCoordinate(50.98508962508, 4.82958530756);
//            var vertex2 = new GeoCoordinate(50.98509255957, 4.83009340615);
//            var vertex3 = new GeoCoordinate(50.98496931078, 4.82889075077);
//            var vertex4 = new GeoCoordinate(50.98496931078, 4.82939884936);
//            var vertex5 = new GeoCoordinate(50.98496931078, 4.83025189562);
//            var vertex6 = new GeoCoordinate(50.98496931078, 4.83079728585);

//            var source = new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.regression_test_1_network.osm"));
//            var router = MultiModalRouter.CreateLiveFrom(source, new OsmRoutingInterpreter());

//            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
//            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);

//            var route = router.Calculate(Vehicle.Car, resolved3, resolved6);
//            var points = new List<GeoCoordinate>(route.GetPoints());

//            Assert.AreEqual(4, points.Count);
//            Assert.AreEqual(0, points[0].DistanceReal(vertex3).Value, e);
//            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
//            Assert.AreEqual(0, points[2].DistanceReal(vertex5).Value, e);
//            Assert.AreEqual(0, points[3].DistanceReal(vertex6).Value, e);

//            route = router.Calculate(Vehicle.Car, resolved6, resolved3);
//            points = new List<GeoCoordinate>(route.GetPoints());

//            Assert.AreEqual(4, points.Count);
//            Assert.AreEqual(0, points[0].DistanceReal(vertex6).Value, e);
//            Assert.AreEqual(0, points[1].DistanceReal(vertex5).Value, e);
//            Assert.AreEqual(0, points[2].DistanceReal(vertex4).Value, e);
//            Assert.AreEqual(0, points[3].DistanceReal(vertex3).Value, e);
//        }
//    }
//}
