//using GTFS;
//using NUnit.Framework;
//using OsmSharp.Collections.Tags.Index;
//using OsmSharp.Math.Geo;
//using OsmSharp.Osm.Xml.Streams;
//using OsmSharp.Routing;
//using OsmSharp.Routing.Graph;
//using OsmSharp.Routing.Osm.Graphs;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Osm.Streams.Graphs;
//using OsmSharp.Routing.Transit.MultiModal;
//using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
//using OsmSharp.Routing.Transit.MultiModal.Routers;
//using System.Collections.Generic;
//using System.Reflection;

//namespace OsmSharp.Transit.Test.Routing.MultiModal.Routes
//{
//    /// <summary>
//    /// Contains tests for the type router multimodal class.
//    /// </summary>
//    [TestFixture]
//    public class TypedRouterMultiModalTests
//    {
//        /// <summary>
//        /// Tests a simple multimodal query.
//        /// </summary>
//        [Test]
//        public void TestMultiModalOnlyTransit()
//        {
//            // build router.
//            var reader = new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.test_network.osm"));
//            var interpreter = new OsmRoutingInterpreter();
//            var tagsIndex = new TagsTableCollectionIndex(); // creates a tagged index.
//            var memoryData = new MultiModalGraphRouterDataSource(new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex));
//            var targetData = new LiveGraphOsmStreamTarget(memoryData.Graph, interpreter, tagsIndex);
//            targetData.RegisterSource(reader);
//            targetData.Pull();
//            var multiModalEdgeRouter = new TypedRouterMultiModal(
//                memoryData, interpreter, new ReferenceCalculator());

//            // read GTFS feed.
//            var gtfsReader = new GTFSReader<GTFSFeed>(false);
//            gtfsReader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = gtfsReader.Read(TestNetworkFeed.BuildSource());

//            // add the feed.
//            multiModalEdgeRouter.AddGTFSFeed(feed);

//            // test some routes.
//            var route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 05, 30, 0), "STOP1", "STOP2");
//            Assert.IsNotNull(route);
//        }

//        /// <summary>
//        /// Tests a simple multimodal query.
//        /// </summary>
//        [Test]
//        public void TestMultiModalOnlyCar()
//        {
//            // build router.
//            var reader = new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.test_network.osm"));
//            var interpreter = new OsmRoutingInterpreter();
//            var tagsIndex = new TagsTableCollectionIndex(); // creates a tagged index.
//            var source = new MultiModalGraphRouterDataSource(new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex));
//            var targetData = new LiveGraphOsmStreamTarget(source.Graph, interpreter, tagsIndex);
//            targetData.RegisterSource(reader);
//            targetData.Pull();
//            var multiModalEdgeRouter = new TypedRouterMultiModal(
//                source, interpreter, new ReferenceCalculator());

//            // read GTFS feed.
//            var gtfsReader = new GTFSReader<GTFSFeed>(false);
//            gtfsReader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = gtfsReader.Read(TestNetworkFeed.BuildSource());

//            // add the feed.
//            multiModalEdgeRouter.AddGTFSFeed(feed);

//            // test some routes.
//            var from = new GeoCoordinate(51.0582205, 3.7189946);
//            var to = new GeoCoordinate(51.0581291, 3.7205005);
//            var route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 05, 30, 0), Vehicle.Car, Vehicle.Car, Vehicle.Car, 
//                multiModalEdgeRouter.Resolve(Vehicle.Car, from, true), multiModalEdgeRouter.Resolve(Vehicle.Car, to, true), null);
//            Assert.IsNotNull(route);
//        }

//        /// <summary>
//        /// Tests a multimodal calculation with a very slow vehicle.
//        /// </summary>
//        [Test]
//        public void TestMultiModalSnail()
//        {            
//            // build router.
//            var reader = new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.test_network.osm"));
//            var interpreter = new OsmRoutingInterpreter();
//            var tagsIndex = new TagsTableCollectionIndex(); // creates a tagged index.
//            var source = new MultiModalGraphRouterDataSource(new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex));
//            var targetData = new LiveGraphOsmStreamTarget(source.Graph, interpreter, tagsIndex);
//            targetData.RegisterSource(reader);
//            targetData.Pull();
//            var multiModalEdgeRouter = new TypedRouterMultiModal(
//                source, interpreter, new ReferenceCalculator());

//            // read GTFS feed.
//            var gtfsReader = new GTFSReader<GTFSFeed>(false);
//            gtfsReader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = gtfsReader.Read(TestNetworkFeed.BuildSource());

//            // add the feed.
//            multiModalEdgeRouter.AddGTFSFeed(feed);

//            // create vehicle and parameters.
//            var vehicle = new OsmSharp.Transit.Test.Routing.MultiModal.RouteCalculators.Snail();
//            var parameters = new Dictionary<string, object>();
//            parameters[ReferenceCalculator.MODAL_TRANSFER_TIME_KEY] = (float)(60 * 60); // make transfer time very high!

//            // test some routes.
//            var from = new GeoCoordinate(51.0582205, 3.7189946);
//            var to = new GeoCoordinate(51.0581291, 3.7205005);
//            var route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 05, 30, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            Assert.IsNotNull(route);
//        }

//        /// <summary>
//        /// Tests a multimodal calculation with a very slow vehicle on the realistic network.
//        /// </summary>
//        [Test]
//        public void TestMultiModalRealistic()
//        {
//            // build router.
//            var reader = new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.test_network_realistic.osm"));
//            var interpreter = new OsmRoutingInterpreter();
//            var tagsIndex = new TagsTableCollectionIndex(); // creates a tagged index.
//            var source = new MultiModalGraphRouterDataSource(new DynamicGraphRouterDataSource<LiveEdge>(tagsIndex));
//            var targetData = new LiveGraphOsmStreamTarget(source.Graph, interpreter, tagsIndex);
//            targetData.RegisterSource(reader);
//            targetData.Pull();
//            var multiModalEdgeRouter = new TypedRouterMultiModal(
//                source, interpreter, new ReferenceCalculator());

//            // read GTFS feed.
//            var gtfsReader = new GTFSReader<GTFSFeed>(false);
//            gtfsReader.DateTimeReader = (dateString) =>
//            {
//                var year = int.Parse(dateString.Substring(0, 4));
//                var month = int.Parse(dateString.Substring(4, 2));
//                var day = int.Parse(dateString.Substring(6, 2));
//                return new System.DateTime(year, month, day);
//            };
//            var feed = gtfsReader.Read(RealisticFeed.BuildSource());

//            // add the feed.
//            multiModalEdgeRouter.AddGTFSFeed(feed);

//            var features = multiModalEdgeRouter.GetNetworkFeatures();
//            var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
//            var geoJson = geoJsonWriter.Write(features);

//            // create vehicle and parameters.
//            var vehicle = Vehicle.Pedestrian;
//            var parameters = new Dictionary<string, object>();

//            // define some fixed starting points.
//            var gent = new GeoCoordinate(51.05780792236328, 3.7134780883789062);
//            var antwerpen = new GeoCoordinate(51.21505355834961, 4.414461612701416);
//            var brussel = new GeoCoordinate(50.84557342529297, 4.3511738777160645);
//            var aalst = new GeoCoordinate(50.93781280517578, 4.051488399505615);
//            var lokeren = new GeoCoordinate(51.0854377746582, 4.043256759643555);
//            var sintNiklaas = new GeoCoordinate(51.13800811767578, 4.1959381103515625);
//            var mechelen = new GeoCoordinate(51.02499008178711, 4.4458112716674805);

//            // test some routes
//            // GEN-ANT @ 01-01-2014 09:45
//            var from = gent;
//            var to = antwerpen;
//            var route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 09, 45, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            var routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            var routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // GEN-ANT @ 01-01-2014 06:00
//            from = gent;
//            to = antwerpen;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 06, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // GEN-BRU @ 01-01-2014 06:00
//            from = gent;
//            to = brussel;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 06, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // BRU-GEN @ 01-01-2014 06:00
//            from = brussel;
//            to = gent;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 06, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // BRU-ANT @ 01-01-2014 06:00
//            from = brussel;
//            to = antwerpen;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 06, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // ANT-BRU @ 01-01-2014 06:00
//            from = antwerpen;
//            to = brussel;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 09, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);

//            // ANT-AAL @ 01-01-2014 06:00
//            from = antwerpen;
//            to = aalst;
//            route = multiModalEdgeRouter.CalculateTransit(new System.DateTime(2014, 01, 01, 09, 00, 0), vehicle, vehicle, vehicle,
//                multiModalEdgeRouter.Resolve(vehicle, from, true), multiModalEdgeRouter.Resolve(vehicle, to, true), parameters);
//            route.Vehicle = Vehicle.Pedestrian.UniqueName;
//            routeFeatures = multiModalEdgeRouter.GetFeatures(route, true);
//            routeGeoJson = geoJsonWriter.Write(routeFeatures);
//            Assert.IsNotNull(route);
//        }
//    }
//}
