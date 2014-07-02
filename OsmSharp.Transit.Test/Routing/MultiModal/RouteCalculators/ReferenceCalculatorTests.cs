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
using NUnit.Framework;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Primitives;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Transit.MultiModal;
using OsmSharp.Routing.Transit.MultiModal.GTFS;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using OsmSharp.Routing.Transit.MultiModal.Routers;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Transit.Test;
using System;
using System.Collections.Generic;
using System.Reflection;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Streams.Graphs;
using OsmSharp.Routing.Transit;

namespace OsmSharp.Transit.Test.Routing.MultiModal.RouteCalculators
{
    /// <summary>
    /// Does some raw routing tests.
    /// </summary>
    [TestFixture]
    public class ReferenceCalculatorTests : SimpleRoutingTests
    {
        /// <summary>
        /// Builds a router.
        /// </summary>
        /// <returns></returns>
        public override Router BuildRouter(MultiModalGraphRouterDataSource data, IRoutingInterpreter interpreter,
            ReferenceCalculator basicRouter)
        {
            // creates the live edge router.
            var liveEdgeRouter = new TypedRouterMultiModal(
                data, interpreter, basicRouter);

            return new Router(liveEdgeRouter); // create the actual router.
        }

        /// <summary>
        /// Builds a basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override ReferenceCalculator BuildBasicRouter(DynamicGraphRouterDataSource<LiveEdge> data)
        {
            return new ReferenceCalculator();
        }

        /// <summary>
        /// Builds data source.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedString"></param>
        /// <returns></returns>
        public override MultiModalGraphRouterDataSource BuildData(IOsmRoutingInterpreter interpreter,
            string embeddedString)
        {
            var tagsIndex = new TagsTableCollectionIndex();

            // do the data processing.
            var source = new MultiModalGraphRouterDataSource(new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex));
            var targetData = new LiveGraphOsmStreamTarget(source.Graph, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            return source;
        }

        /// <summary>
        /// Tests a simple shortest route calculation.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortedDefault()
        {
            this.DoTestShortestDefault();
        }

        /// <summary>
        /// Tests if the raw router preserves tags.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolvedTags()
        {
            this.DoTestResolvedTags();
        }

        /// <summary>
        /// Tests if the raw router preserves tags on arcs/ways.
        /// </summary>
        [Test]
        public void TestDykstraLiveArcTags()
        {
            this.DoTestArcTags();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortest1()
        {
            this.DoTestShortest1();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortest2()
        {
            this.DoTestShortest2();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortest3()
        {
            this.DoTestShortest3();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortest4()
        {
            this.DoTestShortest4();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortest5()
        {
            this.DoTestShortest5();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortestResolved1()
        {
            this.DoTestShortestResolved1();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraLiveShortestResolved2()
        {
            this.DoTestShortestResolved2();
        }

        /// <summary>
        /// Test if the raw router many-to-many weights correspond to the point-to-point weights.
        /// </summary>
        [Test]
        public void TestDykstraLiveManyToMany1()
        {
            this.DoTestManyToMany1();
        }

        /// <summary>
        /// Test if the raw router handles connectivity questions correctly.
        /// </summary>
        [Test]
        public void TestDykstraLiveConnectivity1()
        {
            this.DoTestConnectivity1();
        }

        /// <summary>
        /// Tests a simple shortest route calculation.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolveAllNodes()
        {
            this.DoTestResolveAllNodes();
        }

        /// <summary>
        /// Regression test on routing resolved nodes.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolveBetweenRouteToSelf()
        {
            this.DoTestResolveBetweenRouteToSelf();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolveBetweenClose()
        {
            this.DoTestResolveBetweenClose();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolveCase1()
        {
            this.DoTestResolveCase1();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraLiveResolveCase2()
        {
            this.DoTestResolveCase2();
        }

        /// <summary>
        /// Tests routes along the CITY1 sample route.
        /// </summary>
        [Test]
        public void TestCITY1()
        {
            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(SampleFeed.BuildSource());

            // read the graph.
            var stopVertices = new Dictionary<string, uint>();
            var tripIds = new Dictionary<string, uint>();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(new TagsTableCollectionIndex());
            var schedules = new List<TransitEdgeSchedulePair>();
            GTFSGraphReader.AddToGraph(graph, feed, stopVertices, tripIds, schedules);

            // create the router.
            var router = new ReferenceCalculator();
            var interpreter = new OsmRoutingInterpreter();

            // create parameters.
            var parameters = new Dictionary<string, object>();
            parameters[ReferenceCalculator.START_TIME_KEY] = new System.DateTime(2014, 01, 01, 05, 30, 0);
            Func<uint, DateTime, bool> isTripPossible = (x, y) => { return true; };
            parameters[ReferenceCalculator.IS_TRIP_POSSIBLE_KEY] = isTripPossible;
            parameters[ReferenceCalculator.SCHEDULES_KEY] = schedules;

            // calculate some routes.

            // 4->6 @ 05:30
            var source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(4));
            var target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(6));
            var path = router.Calculate(graph, interpreter, Vehicle.Car, source, target, double.MaxValue, parameters);
            //var path = router.Calculate(graph, 4, 6, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Length());
            Assert.AreEqual(0, path.From.From.Weight); // 0 min start.
            Assert.AreEqual(4, path.From.From.VertexId);
            Assert.AreEqual(1800, path.From.Weight); // 30 mins waiting time.
            Assert.AreEqual(4, path.From.VertexId);
            Assert.AreEqual(2100, path.Weight); // 30 mins + 5 min trip.
            Assert.AreEqual(6, path.VertexId);

            // 4-8 @ 05:30
            source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(4));
            target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(8));
            path = router.Calculate(graph, interpreter, Vehicle.Car, source, target, double.MaxValue, parameters);
            //path = router.Calculate(graph, 4, 8, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
            Assert.AreEqual(6, path.Length());
            Assert.AreEqual(6, path.From.From.From.VertexId);
            Assert.AreEqual(2100, path.From.From.From.Weight); // 30 mins + 5 min trip.
            Assert.AreEqual(5, path.From.From.VertexId);
            Assert.AreEqual(2520, path.From.From.Weight); // 30 mins + 5 min trip + 2 min wait + 5 min trip.
            Assert.AreEqual(7, path.From.VertexId);
            Assert.AreEqual(2940, path.From.Weight); // 30 mins + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip.
            Assert.AreEqual(8, path.VertexId);
            Assert.AreEqual(3360, path.Weight); // 30 mins + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip + 2 min wait + 5 min trip.
        }

        /// <summary>
        /// Tests routes along that tranfer from STBA to AAMV1.
        /// </summary>
        [Test]
        public void TestTransferSTBAToAAMV1()
        {
            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(SampleFeed.BuildSource());

            // read the graph.
            var stopVertices = new Dictionary<string, uint>();
            var tripIds = new Dictionary<string, uint>();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(new TagsTableCollectionIndex());
            var schedules = new List<TransitEdgeSchedulePair>();
            GTFSGraphReader.AddToGraph(graph, feed, stopVertices, tripIds, schedules);

            // create the router.
            var router = new ReferenceCalculator();
            var interpreter = new OsmRoutingInterpreter();

            // create parameters.
            var parameters = new Dictionary<string, object>();
            parameters[ReferenceCalculator.START_TIME_KEY] = new System.DateTime(2014, 01, 01, 05, 30, 0);
            Func<uint, DateTime, bool> isTripPossible = (x, y) => { return true; };
            parameters[ReferenceCalculator.IS_TRIP_POSSIBLE_KEY] = isTripPossible;
            parameters[ReferenceCalculator.SCHEDULES_KEY] = schedules;

            // calculate some routes.

            // 4-9 @ 05:30
            var source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(4));
            var target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(9));
            var path = router.Calculate(graph, interpreter, Vehicle.Car, source, target, double.MaxValue, parameters);
            // var path = router.Calculate(graph, 4, 9, new System.DateTime(2014, 01, 01, 05, 30, 0), (x, y) => { return true; });
            Assert.AreEqual(5, path.Length());
            Assert.AreEqual(4, path.From.From.From.VertexId);
            Assert.AreEqual(1800, path.From.From.From.Weight); // 30 mins wait
            Assert.AreEqual(2, path.From.From.VertexId);
            Assert.AreEqual(3000, path.From.From.Weight); // 30 mins wait + 20 mins trip.
            Assert.AreEqual(2, path.From.VertexId);
            Assert.AreEqual(9000, path.From.Weight); // 30 mins wait + 20 mins trip + 1h40mins waiting for transfer.
            Assert.AreEqual(9, path.VertexId);
            Assert.AreEqual(12600, path.Weight); // 30 mins wait + 20 mins trip + 1h40mins waiting for transfer + 1h trip
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route but fully multimodal this time!
        /// </summary>
        [Test]
        public void TestMultiModalSimple()
        {
            var vehicle = new Snail();
            var interpreter = new OsmRoutingInterpreter();
            var sourceGraph = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(TestNetworkFeed.BuildSource());

            // read the graph.
            var stopVertices = new Dictionary<string, uint>();
            var tripIds = new Dictionary<string, uint>();
            var schedules = new List<TransitEdgeSchedulePair>();
            GTFSGraphReader.AddToGraph(sourceGraph.Graph, feed, stopVertices, tripIds, schedules);

            // create the router.
            var router = new ReferenceCalculator();

            // create parameters.
            var parameters = new Dictionary<string, object>();
            parameters[ReferenceCalculator.START_TIME_KEY] = new System.DateTime(2014, 01, 01, 05, 30, 0);
            Func<uint, DateTime, bool> isTripPossible = (x, y) => { return true; };
            parameters[ReferenceCalculator.IS_TRIP_POSSIBLE_KEY] = isTripPossible;
            parameters[ReferenceCalculator.MODAL_TRANSFER_TIME_KEY] = (float)(60 * 60); // make transfer time very high!
            parameters[ReferenceCalculator.SCHEDULES_KEY] = schedules;

            // calculate some routes.

            // STOP1-STOP5 @ 05:30
            var source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(stopVertices["STOP1"]));
            var target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(stopVertices["STOP5"]));
            var path = router.Calculate(sourceGraph.Graph, interpreter, vehicle, source, target, double.MaxValue, parameters);
            Assert.AreEqual(5, path.Length());
            Assert.AreEqual(11, path.From.From.From.From.VertexId);
            Assert.AreEqual(12, path.From.From.From.VertexId);
            Assert.AreEqual(13, path.From.From.VertexId);
            Assert.AreEqual(14, path.From.VertexId);
            Assert.AreEqual(15, path.VertexId);

            // (51.0582205, 3.7189946)-STOP5 @ 05:30
            source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(sourceGraph.Graph.GetVertexAt(new GeoCoordinate(51.0582205, 3.7189946))));
            target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(stopVertices["STOP5"]));
            path = router.Calculate(sourceGraph.Graph, interpreter, vehicle, source, target, double.MaxValue, parameters);
            Assert.AreEqual(6, path.Length());
            Assert.AreEqual(7, path.From.From.From.From.From.VertexId);
            Assert.AreEqual(11, path.From.From.From.From.VertexId);
            Assert.AreEqual(12, path.From.From.From.VertexId);
            Assert.AreEqual(13, path.From.From.VertexId);
            Assert.AreEqual(14, path.From.VertexId);
            Assert.AreEqual(15, path.VertexId);

            // (51.0582205, 3.7189946)-(51.0581291, 3.7205005) @ 05:30
            source = new PathSegmentVisitList();
            source.UpdateVertex(new PathSegment<long>(sourceGraph.Graph.GetVertexAt(new GeoCoordinate(51.0582205, 3.7189946))));
            target = new PathSegmentVisitList();
            target.UpdateVertex(new PathSegment<long>(sourceGraph.Graph.GetVertexAt(new GeoCoordinate(51.0581291, 3.7205005))));
            path = router.Calculate(sourceGraph.Graph, interpreter, vehicle, source, target, double.MaxValue, parameters);
            Assert.AreEqual(8, path.Length());
            Assert.AreEqual(7, path.From.From.From.From.From.From.From.VertexId);
            Assert.AreEqual(11, path.From.From.From.From.From.From.VertexId);
            Assert.AreEqual(12, path.From.From.From.From.From.VertexId);
            Assert.AreEqual(13, path.From.From.From.From.VertexId);
            Assert.AreEqual(14, path.From.From.From.VertexId);
            Assert.AreEqual(15, path.From.From.VertexId);
            Assert.AreEqual(2, path.From.VertexId);
            Assert.AreEqual(1, path.VertexId);
        }
    }

    /// <summary>
    /// Base class with tests around the Router object.
    /// </summary>
    public abstract class SimpleRoutingTests
    {
        /// <summary>
        /// Builds the router.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpreter"></param>
        /// <param name="basicRouter"></param>
        /// <returns></returns>
        public abstract OsmSharp.Routing.Router BuildRouter(MultiModalGraphRouterDataSource data,
            IRoutingInterpreter interpreter, ReferenceCalculator basicRouter);

        /// <summary>
        /// Builds the basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract ReferenceCalculator BuildBasicRouter(DynamicGraphRouterDataSource<LiveEdge> data);

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedString"></param>
        /// <returns></returns>
        public abstract MultiModalGraphRouterDataSource BuildData(IOsmRoutingInterpreter interpreter,
            string embeddedString);

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestDefault()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Entries.Length);

            // float latitude, longitude;
            // data.GetVertex(20, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Entries[0].Latitude, 0.00001);
            Assert.AreEqual(3.71922278, route.Entries[0].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Start, route.Entries[0].Type);

            // data.GetVertex(21, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Entries[1].Latitude, 0.00001);
            Assert.AreEqual(3.71956539, route.Entries[1].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Along, route.Entries[1].Type);

            // data.GetVertex(16, out latitude, out longitude);
            Assert.AreEqual(51.05773, route.Entries[2].Latitude, 0.00001);
            Assert.AreEqual(3.719745, route.Entries[2].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Along, route.Entries[2].Type);

            // data.GetVertex(22, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Entries[3].Latitude, 0.00001);
            Assert.AreEqual(3.71965814, route.Entries[3].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Along, route.Entries[3].Type);

            // data.GetVertex(23, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Entries[4].Latitude, 0.00001);
            Assert.AreEqual(3.71918, route.Entries[4].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Stop, route.Entries[4].Type);
        }

        /// <summary>
        /// Tests that a router preserves tags given to resolved points.
        /// </summary>
        protected void DoTestResolvedTags()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            source.Tags.Add(new KeyValuePair<string, string>("name", "source"));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            target.Tags.Add(new KeyValuePair<string, string>("name", "target"));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Entries.Length);

            // float latitude, longitude;
            // data.GetVertex(20, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Entries[0].Latitude, 0.00001);
            Assert.AreEqual(3.71922278, route.Entries[0].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Start, route.Entries[0].Type);
            Assert.IsNotNull(route.Entries[0].Points[0].Tags);
            Assert.AreEqual(1, route.Entries[0].Points[0].Tags.Length);
            Assert.AreEqual("source", route.Entries[0].Points[0].Tags[0].Value);

            // data.GetVertex(23, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Entries[4].Latitude, 0.00001);
            Assert.AreEqual(3.71918, route.Entries[4].Longitude, 0.00001);
            Assert.AreEqual(RoutePointEntryType.Stop, route.Entries[4].Type);
            Assert.IsNotNull(route.Entries[4].Points[0].Tags);
            Assert.AreEqual(1, route.Entries[4].Points[0].Tags.Length);
            Assert.AreEqual("target", route.Entries[4].Points[0].Tags[0].Value);
        }

        /// <summary>
        /// Tests that a router preserves tags that are located on ways/arcs in the route.
        /// </summary>
        protected void DoTestArcTags()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            source.Tags.Add(new KeyValuePair<string, string>("name", "source"));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            target.Tags.Add(new KeyValuePair<string, string>("name", "target"));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Entries.Length);

            Assert.AreEqual("highway", route.Entries[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Entries[1].Tags[0].Value);

            Assert.AreEqual("highway", route.Entries[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Entries[2].Tags[0].Value);

            Assert.AreEqual("highway", route.Entries[3].Tags[0].Key);
            Assert.AreEqual("residential", route.Entries[3].Tags[0].Value);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest2()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest3()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest4()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest5()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basic_router = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basic_router);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(7, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestResolved1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578153, 3.7193937));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0582408, 3.7194636));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(10, route.Entries.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestResolved2()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581843, 3.7201209)); // between 2 - 3
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581484, 3.7194957)); // between 9 - 8

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Entries.Length);
        }

        /// <summary>
        /// Tests if the many-to-many weights are the same as the point-to-point weights.
        /// </summary>
        protected void DoTestManyToMany1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(
                data, interpreter, basicRouter);

            var resolvedPoints = new RouterPoint[3];
            resolvedPoints[0] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            resolvedPoints[1] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            resolvedPoints[2] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            var weights = router.CalculateManyToManyWeight(Vehicle.Car, resolvedPoints, resolvedPoints);

            for (int x = 0; x < weights.Length; x++)
            {
                for (int y = 0; y < weights.Length; y++)
                {
                    var manyToMany = weights[x][y];
                    var pointToPoint = router.CalculateWeight(Vehicle.Car, resolvedPoints[x], resolvedPoints[y]);

                    Assert.AreEqual(pointToPoint, manyToMany);
                }
            }
        }

        /// <summary>
        /// Test if the connectivity test succeed/fail.
        /// </summary>
        protected void DoTestConnectivity1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            var resolvedPoints = new RouterPoint[3];
            resolvedPoints[0] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            resolvedPoints[1] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            resolvedPoints[2] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            // test connectivity succes.
            Assert.IsTrue(router.CheckConnectivity(Vehicle.Car, resolvedPoints[0], 5));
            //Assert.IsTrue(router.CheckConnectivity(VehicleEnum.Car, resolved_points[1], 5));
            Assert.IsTrue(router.CheckConnectivity(Vehicle.Car, resolvedPoints[2], 5));

            // test connectivity failiure.
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[0], 1000));
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[1], 1000));
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[2], 1000));
        }

        /// <summary>
        /// Test if the resolving of nodes returns those same nodes.
        /// 
        /// (does not work on a lazy loading data source!)
        /// </summary>
        protected void DoTestResolveAllNodes()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            for (int idx = 1; idx < data.Graph.VertexCount; idx++)
            {
                float latitude, longitude;
                if (data.Graph.GetVertex((uint)idx, out latitude, out longitude))
                {
                    RouterPoint point = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
                    Assert.AreEqual(idx, (point as RouterPoint).Id);
                }
            }
        }

        /// <summary>
        /// Test if routes from a resolved node to itself is correctly calculated.
        /// 
        /// Regression Test: Routing to self with a resolved node returns a route to the nearest real node and back.
        /// </summary>
        protected void DoTestResolveBetweenRouteToSelf()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            // first test a non-between node.
            var resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            var route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Entries.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);

            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578761, 3.7193972)); //,-103,  -4,  -8
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Entries.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);


            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576510, 3.7194124)); //,-104, -14, -12
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Entries.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);

            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576829, 3.7196791)); //,-105, -12, -10
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Entries.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);
        }

        /// <summary>
        /// Test if routes between two resolved nodes are correctly calculated.
        /// </summary>
        protected void DoTestResolveBetweenClose()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            for (double position1 = 0.1; position1 < 0.91; position1 = position1 + 0.1)
            {
                var point = vertex20 + ((vertex21 - vertex20) * position1);
                var vertex2021 = new GeoCoordinate(point[1], point[0]);
                for (double position2 = 0.1; position2 < 0.91; position2 = position2 + 0.1)
                {
                    point = vertex21 + ((vertex16 - vertex21) * position2);
                    var vertex2116 = new GeoCoordinate(point[1], point[0]);

                    // calculate route.
                    var basicRouter = this.BuildBasicRouter(data.Graph);
                    var router = this.BuildRouter(data, interpreter, basicRouter);

                    var route = router.Calculate(Vehicle.Car,
                        router.Resolve(Vehicle.Car, vertex2021),
                        router.Resolve(Vehicle.Car, vertex2116));

                    Assert.AreEqual(3, route.Entries.Length);
                    Assert.AreEqual(vertex2021.Latitude, route.Entries[0].Latitude, 0.0001);
                    Assert.AreEqual(vertex2021.Longitude, route.Entries[0].Longitude, 0.0001);

                    Assert.AreEqual(vertex21.Latitude, route.Entries[1].Latitude, 0.0001);
                    Assert.AreEqual(vertex21.Longitude, route.Entries[1].Longitude, 0.0001);

                    Assert.AreEqual(vertex2116.Latitude, route.Entries[2].Latitude, 0.0001);
                    Assert.AreEqual(vertex2116.Longitude, route.Entries[2].Longitude, 0.0001);
                }
            }
        }

        /// <summary>
        /// Test if routes between two resolved nodes are correctly calculated.
        /// </summary>
        protected void DoTestResolveBetweenTwo()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);

            for (double position1 = 0.1; position1 < 0.91; position1 = position1 + 0.1)
            {
                PointF2D point = vertex20 + ((vertex21 - vertex20) * position1);
                var vertex2021 = new GeoCoordinate(point[1], point[0]);

                point = vertex21 + ((vertex20 - vertex21) * position1);
                var vertex2120 = new GeoCoordinate(point[1], point[0]);

                // calculate route.
                var basicRouter = this.BuildBasicRouter(data.Graph);
                Router router = this.BuildRouter(data, interpreter, basicRouter);

                Route route = router.Calculate(Vehicle.Car,
                    router.Resolve(Vehicle.Car, vertex2021),
                    router.Resolve(Vehicle.Car, vertex2120));

                if (vertex2021.Latitude != vertex2120.Latitude &&
                    vertex2021.Longitude != vertex2120.Longitude)
                {
                    Assert.AreEqual(2, route.Entries.Length);
                    Assert.AreEqual(vertex2021.Latitude, route.Entries[0].Latitude, 0.0001);
                    Assert.AreEqual(vertex2021.Longitude, route.Entries[0].Longitude, 0.0001);

                    Assert.AreEqual(vertex2120.Latitude, route.Entries[1].Latitude, 0.0001);
                    Assert.AreEqual(vertex2120.Longitude, route.Entries[1].Longitude, 0.0001);
                }
            }
        }

        /// <summary>
        /// Test if routes between resolved nodes are correctly calculated.
        /// 
        /// 20----x----21----x----16
        /// </summary>
        protected void DoTestResolveCase1()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            var point = vertex20 + ((vertex21 - vertex20) * 0.5);
            var vertex2021 = new GeoCoordinate(point[1], point[0]);

            point = vertex21 + ((vertex16 - vertex21) * 0.5);
            var vertex2116 = new GeoCoordinate(point[1], point[0]);

            // calculate route.
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var route = router.Calculate(Vehicle.Car,
                router.Resolve(Vehicle.Car, vertex2021),
                router.Resolve(Vehicle.Car, vertex2116));

            Assert.AreEqual(3, route.Entries.Length);
            Assert.AreEqual(vertex2021.Latitude, route.Entries[0].Latitude, 0.0001);
            Assert.AreEqual(vertex2021.Longitude, route.Entries[0].Longitude, 0.0001);

            Assert.AreEqual(vertex21.Latitude, route.Entries[1].Latitude, 0.0001);
            Assert.AreEqual(vertex21.Longitude, route.Entries[1].Longitude, 0.0001);

            Assert.AreEqual(vertex2116.Latitude, route.Entries[2].Latitude, 0.0001);
            Assert.AreEqual(vertex2116.Longitude, route.Entries[2].Longitude, 0.0001);
        }

        /// <summary>
        /// Test if routes between resolved nodes are correctly calculated.
        /// 
        /// 20--x---x--21---------16
        /// </summary>
        protected void DoTestResolveCase2()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
            //            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            var point = vertex20 + ((vertex21 - vertex20) * 0.25);
            var vertex20211 = new GeoCoordinate(point[1], point[0]);

            point = vertex20 + ((vertex21 - vertex20) * 0.75);
            var vertex20212 = new GeoCoordinate(point[1], point[0]);

            // calculate route.
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var route = router.Calculate(Vehicle.Car,
                router.Resolve(Vehicle.Car, vertex20211),
                router.Resolve(Vehicle.Car, vertex20212));

            Assert.AreEqual(2, route.Entries.Length);
            Assert.AreEqual(vertex20211.Latitude, route.Entries[0].Latitude, 0.0001);
            Assert.AreEqual(vertex20211.Longitude, route.Entries[0].Longitude, 0.0001);

            Assert.AreEqual(vertex20212.Latitude, route.Entries[1].Latitude, 0.0001);
            Assert.AreEqual(vertex20212.Longitude, route.Entries[1].Longitude, 0.0001);
        }

        /// <summary>
        /// Resolves coordinates at the same locations and checks tag preservation.
        /// </summary>
        protected void DoTestResolveSameLocation()
        {
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Transit.Test.test_network.osm");

            // create router.
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            // define test tags.
            var tags1 = new Dictionary<string, string>();
            tags1.Add("test1", "yes");
            var tags2 = new Dictionary<string, string>();
            tags2.Add("test2", "yes");

            // resolve points.
            var point1 = router.Resolve(Vehicle.Car, vertex16);
            point1.Tags.Add(new KeyValuePair<string, string>("test1", "yes"));

            // test presence of tags.
            Assert.AreEqual(1, point1.Tags.Count);
            Assert.AreEqual("test1", point1.Tags[0].Key);
            Assert.AreEqual("yes", point1.Tags[0].Value);

            // resolve point again.
            RouterPoint point2 = router.Resolve(Vehicle.Car, vertex16);

            // the tags should be here still!
            Assert.AreEqual(1, point2.Tags.Count);
            Assert.AreEqual("test1", point2.Tags[0].Key);
            Assert.AreEqual("yes", point2.Tags[0].Value);
        }

        /// <summary>
        /// Tests many-to-many routing.
        /// </summary>
        protected void DoTestManyToMany(string filename)
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, string.Format("OsmSharp.Test.Unittests.{0}", filename));

            // create router.
            var basicRouter = this.BuildBasicRouter(data.Graph);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var resolved = new RouterPoint[data.Graph.VertexCount - 1];
            for (uint idx = 1; idx < data.Graph.VertexCount; idx++)
            { // resolve each vertex.
                float latitude, longitude;
                if (data.Graph.GetVertex(idx, out latitude, out longitude))
                {
                    resolved[idx - 1] = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude).OffsetRandom(20), true);
                }

                // reference and resolved have to exist.
                Assert.IsNotNull(resolved[idx - 1]);
            }

            // limit tests to a fixed number.
            int pointSize = 100;
            int testEveryOther = resolved.Length / pointSize;
            testEveryOther = System.Math.Max(testEveryOther, 1);

            // check all the routes having the same weight(s).
            var points = new List<RouterPoint>();
            for (int idx = 0; idx < resolved.Length; idx++)
            {
                int testNumber = idx;
                if (testNumber % testEveryOther == 0)
                {
                    points.Add(resolved[idx]);
                }
            }

            // calculate many-to-many weights.
            var weights = router.CalculateManyToManyWeight(Vehicle.Car, points.ToArray(), points.ToArray());
            for (int fromIdx = 0; fromIdx < points.Count; fromIdx++)
            {
                for (int toIdx = 0; toIdx < points.Count; toIdx++)
                {
                    var weight = router.CalculateWeight(Vehicle.Car, points[fromIdx], points[toIdx]);
                    Assert.AreEqual(weight, weights[fromIdx][toIdx]);
                }
            }
        }
    }
}