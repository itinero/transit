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
//using OsmSharp.Routing.Transit;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using OsmSharp.Routing.Osm.Graphs;

//namespace OsmSharp.Transit.Test.Routing
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    [TestFixture]
//    public class GTFSReaderTests
//    {
//        /// <summary>
//        /// Test reads googles example GTFS feed.
//        /// </summary>
//        [Test]
//        public void TestReadGTFSExample()
//        {
//            double delta = 0.00001;

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
//            var stopVertices = new Dictionary<string, uint>();
//            var tripIds = new Dictionary<string, uint>();
//            var graph = GTFSGraphReader.CreateGraph(feed, stopVertices, tripIds);

//            // check vertices.
//            Assert.AreEqual(9, graph.VertexCount);
//            float latitude, longitude;
//            //FUR_CREEK_RES,Furnace Creek Resort (Demo),,36.425288,-117.133162,,
//            graph.GetVertex(1, out latitude, out longitude);
//            Assert.AreEqual(36.425288, latitude, delta);
//            Assert.AreEqual(-117.133162, longitude, delta);
//            //BEATTY_AIRPORT,Nye County Airport (Demo),,36.868446,-116.784582,,
//            graph.GetVertex(2, out latitude, out longitude);
//            Assert.AreEqual(36.868446, latitude, delta);
//            Assert.AreEqual(-116.784582, longitude, delta);
//            //BULLFROG,Bullfrog (Demo),,36.88108,-116.81797,,
//            graph.GetVertex(3, out latitude, out longitude);
//            Assert.AreEqual(36.88108, latitude, delta);
//            Assert.AreEqual(-116.81797, longitude, delta);
//            //STAGECOACH,Stagecoach Hotel & Casino (Demo),,36.915682,-116.751677,,
//            graph.GetVertex(4, out latitude, out longitude);
//            Assert.AreEqual(36.91568, latitude, delta);
//            Assert.AreEqual(-116.751677, longitude, delta);
//            //NADAV,North Ave / D Ave N (Demo),,36.914893,-116.76821,,
//            graph.GetVertex(5, out latitude, out longitude);
//            Assert.AreEqual(36.914893, latitude, delta);
//            Assert.AreEqual(-116.76821, longitude, delta);
//            //NANAA,North Ave / N A Ave (Demo),,36.914944,-116.761472,,
//            graph.GetVertex(6, out latitude, out longitude);
//            Assert.AreEqual(36.914944, latitude, delta);
//            Assert.AreEqual(-116.76147, longitude, delta);
//            //DADAN,Doing Ave / D Ave N (Demo),,36.909489,-116.768242,,
//            graph.GetVertex(7, out latitude, out longitude);
//            Assert.AreEqual(36.909489, latitude, delta);
//            Assert.AreEqual(-116.768242, longitude, delta);
//            //EMSI,E Main St / S Irving St (Demo),,36.905697,-116.76218,,
//            graph.GetVertex(8, out latitude, out longitude);
//            Assert.AreEqual(36.905697, latitude, delta);
//            Assert.AreEqual(-116.76218, longitude, delta);
//            //AMV,Amargosa Valley (Demo),,36.641496,-116.40094,,
//            graph.GetVertex(9, out latitude, out longitude);
//            Assert.AreEqual(36.641496, latitude, delta);
//            Assert.AreEqual(-116.40094, longitude, delta);

//            // check edges and schedule.

//            // check 4<->2
//            TransitEdge transitEdge;
//            Assert.IsTrue(graph.GetEdge(4, 2, out transitEdge));
//            var forwardSchedule = transitEdge.ForwardSchedule;
//            var backwardSchedule = transitEdge.BackwardSchedule;
//            if(!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["STBA"], new DateTime(2014, 1, 1, 6, 0, 0), new DateTime(2014, 1, 1, 6, 20, 0)));
//            Assert.AreEqual(0, backwardSchedule.Count);
//            Assert.IsTrue(graph.GetEdge(2, 4, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(0, forwardSchedule.Count);
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["STBA"], new DateTime(2014, 1, 1, 6, 0, 0), new DateTime(2014, 1, 1, 6, 20, 0)));

//            // check 4<->6
//            Assert.IsTrue(graph.GetEdge(4, 6, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 0, 0), new DateTime(2014, 1, 1, 6, 05, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 51, 0), new DateTime(2014, 1, 1, 6, 56, 0)));
//            Assert.IsTrue(graph.GetEdge(6, 4, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 51, 0), new DateTime(2014, 1, 1, 6, 56, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 0, 0), new DateTime(2014, 1, 1, 6, 05, 0)));

//            // check 5<->6
//            Assert.IsTrue(graph.GetEdge(5, 6, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 44, 0), new DateTime(2014, 1, 1, 6, 49, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 07, 0), new DateTime(2014, 1, 1, 6, 12, 0)));
//            Assert.IsTrue(graph.GetEdge(6, 5, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 07, 0), new DateTime(2014, 1, 1, 6, 12, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 44, 0), new DateTime(2014, 1, 1, 6, 49, 0)));

//            // check 5<->7
//            Assert.IsTrue(graph.GetEdge(5, 7, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 14, 0), new DateTime(2014, 1, 1, 6, 19, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 37, 0), new DateTime(2014, 1, 1, 6, 42, 0)));
//            Assert.IsTrue(graph.GetEdge(7, 5, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 37, 0), new DateTime(2014, 1, 1, 6, 42, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 14, 0), new DateTime(2014, 1, 1, 6, 19, 0)));

//            // check 8<->7
//            Assert.IsTrue(graph.GetEdge(8, 7, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 30, 0), new DateTime(2014, 1, 1, 6, 35, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 21, 0), new DateTime(2014, 1, 1, 6, 26, 0)));
//            Assert.IsTrue(graph.GetEdge(7, 8, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["CITY1"], new DateTime(2014, 1, 1, 6, 21, 0), new DateTime(2014, 1, 1, 6, 26, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["CITY2"], new DateTime(2014, 1, 1, 6, 30, 0), new DateTime(2014, 1, 1, 6, 35, 0)));

//            // check 2<->3
//            Assert.IsTrue(graph.GetEdge(2, 3, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AB1"], new DateTime(2014, 1, 1, 8, 00, 0), new DateTime(2014, 1, 1, 8, 10, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AB2"], new DateTime(2014, 1, 1, 12, 05, 0), new DateTime(2014, 1, 1, 12, 15, 0)));
//            Assert.IsTrue(graph.GetEdge(3, 2, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AB2"], new DateTime(2014, 1, 1, 12, 05, 0), new DateTime(2014, 1, 1, 12, 15, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AB1"], new DateTime(2014, 1, 1, 8, 00, 0), new DateTime(2014, 1, 1, 8, 10, 0)));

//            // check 3<->1
//            Assert.IsTrue(graph.GetEdge(3, 1, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["BFC1"], new DateTime(2014, 1, 1, 8, 20, 0), new DateTime(2014, 1, 1, 9, 20, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["BFC2"], new DateTime(2014, 1, 1, 11, 0, 0), new DateTime(2014, 1, 1, 12, 0, 0)));
//            Assert.IsTrue(graph.GetEdge(1, 3, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(1, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["BFC2"], new DateTime(2014, 1, 1, 11, 0, 0), new DateTime(2014, 1, 1, 12, 0, 0)));
//            Assert.AreEqual(1, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["BFC1"], new DateTime(2014, 1, 1, 8, 20, 0), new DateTime(2014, 1, 1, 9, 20, 0)));

//            // check 2<->9
//            Assert.IsTrue(graph.GetEdge(2, 9, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AAMV1"], new DateTime(2014, 1, 1, 8, 0, 0), new DateTime(2014, 1, 1, 9, 0, 0)));
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AAMV3"], new DateTime(2014, 1, 1, 13, 0, 0), new DateTime(2014, 1, 1, 14, 0, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AAMV2"], new DateTime(2014, 1, 1, 10, 0, 0), new DateTime(2014, 1, 1, 11, 0, 0)));
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AAMV4"], new DateTime(2014, 1, 1, 15, 0, 0), new DateTime(2014, 1, 1, 16, 0, 0)));
//            Assert.IsTrue(graph.GetEdge(9, 2, out transitEdge));
//            forwardSchedule = transitEdge.ForwardSchedule;
//            backwardSchedule = transitEdge.BackwardSchedule;
//            if (!transitEdge.Forward)
//            {
//                var other = forwardSchedule;
//                forwardSchedule = backwardSchedule;
//                backwardSchedule = other;
//            }
//            Assert.AreEqual(2, forwardSchedule.Count);
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AAMV2"], new DateTime(2014, 1, 1, 10, 0, 0), new DateTime(2014, 1, 1, 11, 0, 0)));
//            Assert.IsTrue(forwardSchedule.Contains(tripIds["AAMV4"], new DateTime(2014, 1, 1, 15, 0, 0), new DateTime(2014, 1, 1, 16, 0, 0)));
//            Assert.AreEqual(2, backwardSchedule.Count);
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AAMV1"], new DateTime(2014, 1, 1, 8, 0, 0), new DateTime(2014, 1, 1, 9, 0, 0)));
//            Assert.IsTrue(backwardSchedule.Contains(tripIds["AAMV3"], new DateTime(2014, 1, 1, 13, 0, 0), new DateTime(2014, 1, 1, 14, 0, 0)));
//        }
//    }
//}