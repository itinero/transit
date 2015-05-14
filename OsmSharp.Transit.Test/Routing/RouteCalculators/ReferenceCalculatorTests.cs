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

//using GTFS;
//using NUnit.Framework;
//using OsmSharp.Routing.Transit.GTFS;
//using OsmSharp.Routing.Transit.RouteCalculators;

//namespace OsmSharp.Transit.Test.Routing.RouteCalculators
//{
//    /// <summary>
//    /// Contains tests for the reference calculator.
//    /// </summary>
//    [TestFixture]
//    public class ReferenceCalculatorTests
//    {
//        /// <summary>
//        /// Tests routes along the CITY1 sample route.
//        /// </summary>
//        [Test]
//        public void TestCITY1()
//        {
//            // read the sample feed.
//            var reader = new GTFSReader<GTFSFeed>(false);
//            reader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = reader.Read(SampleFeed.BuildSource());

//            // read the graph.
//            var graph = GTFSGraphReader.CreateGraph(feed);

//            // create the router.
//            var router = new ReferenceCalculator();

//            // calculate some routes.

//            // 4->6 @ 05:30
//            var path = router.Calculate(graph, 4, 6, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
//            Assert.AreEqual(2, path.Length());
//            Assert.AreEqual(1800, path.From.VertexId.Seconds); // 30 mins waiting time.
//            Assert.AreEqual(4, path.From.VertexId.Vertex);
//            Assert.AreEqual(2100, path.VertexId.Seconds); // 30 mins + 5 min trip.
//            Assert.AreEqual(6, path.VertexId.Vertex);

//            // 4-8 @ 05:30
//            path = router.Calculate(graph, 4, 8, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
//            Assert.AreEqual(5, path.Length());
//            Assert.AreEqual(6, path.From.From.From.VertexId.Vertex);
//            Assert.AreEqual(2100, path.From.From.From.VertexId.Seconds); // 30 mins + 5 min trip.
//            Assert.AreEqual(5, path.From.From.VertexId.Vertex);
//            Assert.AreEqual(2520, path.From.From.VertexId.Seconds); // 30 mins + 5 min trip + 2 min wait + 5 min trip.
//            Assert.AreEqual(7, path.From.VertexId.Vertex);
//            Assert.AreEqual(2940, path.From.VertexId.Seconds); // 30 mins + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip.
//            Assert.AreEqual(8, path.VertexId.Vertex);
//            Assert.AreEqual(3360, path.VertexId.Seconds); // 30 mins + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip.
//        }

//        /// <summary>
//        /// Tests routes along that tranfer from STBA to AAMV1.
//        /// </summary>
//        [Test]
//        public void TestTransferSTBAToAAMV1()
//        {
//            // read the sample feed.
//            var reader = new GTFSReader<GTFSFeed>(false);
//            reader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = reader.Read(SampleFeed.BuildSource());

//            // read the graph.
//            var graph = GTFSGraphReader.CreateGraph(feed);

//            // create the router.
//            var router = new ReferenceCalculator();

//            // calculate some routes.

//            // 4-9 @ 05:30
//            var path = router.Calculate(graph, 4, 9, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
//            Assert.AreEqual(4, path.Length());
//            Assert.AreEqual(4, path.From.From.From.VertexId.Vertex);
//            Assert.AreEqual(1800, path.From.From.From.VertexId.Seconds); // 30 mins wait
//            Assert.AreEqual(2, path.From.From.VertexId.Vertex);
//            Assert.AreEqual(3000, path.From.From.VertexId.Seconds); // 30 mins wait + 20 mins trip.
//            Assert.AreEqual(2, path.From.VertexId.Vertex);
//            Assert.AreEqual(9000, path.From.VertexId.Seconds); // 30 mins wait + 20 mins trip + 1h40mins waiting for transfer.
//            Assert.AreEqual(9, path.VertexId.Vertex);
//            Assert.AreEqual(12600, path.VertexId.Seconds); // 30 mins wait + 20 mins trip + 1h40mins waiting for transfer + 1h trip
//        }

//        /// <summary>
//        /// Tests routes along the SLOW1 vs QUICK1 sample route.
//        /// </summary>
//        [Test]
//        public void TestSLOW1vsQUICK1()
//        {
//            // read the sample feed.
//            var reader = new GTFSReader<GTFSFeed>(false);
//            reader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = reader.Read(SampleFeed.BuildSource());

//            // read the graph.
//            var graph = GTFSGraphReader.CreateGraph(feed);

//            // create the router.
//            var router = new ReferenceCalculator();

//            // calculate some routes.

//            // 4->6 @ 05:30
//            var path = router.Calculate(graph, 4, 8, new System.DateTime(2014, 01, 01, 15, 45, 0), (x, y) => { return true; });
//            Assert.AreEqual(2, path.Length()); // this one must take the short path!
//        }
//    }
//}