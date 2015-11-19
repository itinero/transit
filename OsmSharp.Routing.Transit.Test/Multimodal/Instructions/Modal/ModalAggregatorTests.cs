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

//using NUnit.Framework;
//using OsmSharp.Routing;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Transit.Data;
//using OsmSharp.Routing.Transit.Multimodal.Instructions.Modal;

//namespace OsmSharp.Transit.Test.Multimodal.Instructions.Modal
//{
//    /// <summary>
//    /// Tests the modal arc aggregator.
//    /// </summary>
//    [TestFixture]
//    public class ModalAggregatorTests
//    {
//        /// <summary>
//        /// Tests aggregation of a route only.
//        /// </summary>
//        [Test]
//        public void Test1RoadOnly1Segment()
//        {
//            // build test route.
//            var route = new Route()
//            {
//                Segments = new RouteSegment[] 
//                {
//                    new RouteSegment()
//                    {
//                        Distance = 0,
//                        Latitude = 0,
//                        Longitude = 0,
//                        Time = 0,
//                        Type = RouteSegmentType.Start,
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 1,
//                        Latitude = 1,
//                        Time = 30,
//                        Type = RouteSegmentType.Stop,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    }
//                }
//            };

//            // aggregate.
//            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
//            var result = aggregator.Aggregate(route);
//            var point = result;
//            Assert.IsNotNull(point);
//            Assert.AreEqual(0, point.Location.Latitude);
//            Assert.AreEqual(0, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            var arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual("Car", arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("highway", "residential"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(1, point.Location.Latitude);
//            Assert.AreEqual(1, point.Location.Longitude);
//            Assert.IsNull(point.Next);
//        }

//        /// <summary>
//        /// Tests aggregation of a route only.
//        /// </summary>
//        [Test]
//        public void Test2RoadOnly2Segments()
//        {
//            // build test route.
//            var route = new Route()
//            {
//                Segments = new RouteSegment[] 
//                {
//                    new RouteSegment()
//                    {
//                        Distance = 0,
//                        Latitude = 0,
//                        Longitude = 0,
//                        Time = 0,
//                        Type = RouteSegmentType.Start,
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 1,
//                        Latitude = 1,
//                        Time = 30,
//                        Type = RouteSegmentType.Stop,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 2,
//                        Latitude = 2,
//                        Time = 30,
//                        Type = RouteSegmentType.Stop,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    }
//                }
//            };

//            // aggregate.
//            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
//            var result = aggregator.Aggregate(route);
//            var point = result;
//            Assert.IsNotNull(point);
//            Assert.AreEqual(0, point.Location.Latitude);
//            Assert.AreEqual(0, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            var arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual("Car", arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("highway", "residential"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(2, point.Location.Latitude);
//            Assert.AreEqual(2, point.Location.Longitude);
//            Assert.IsNull(point.Next);
//        }

//        /// <summary>
//        /// Tests aggregation of a transit route only.
//        /// </summary>
//        [Test]
//        public void Test3TransitOnly1Segment()
//        {
//            // build test route.
//            var route = new Route()
//            {
//                Segments = new RouteSegment[] 
//                {
//                    new RouteSegment()
//                    {
//                        Distance = 0,
//                        Latitude = 0,
//                        Longitude = 0,
//                        Time = 0,
//                        Type = RouteSegmentType.Start,
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 1,
//                        Latitude = 1,
//                        Time = 30,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "transit.stop.id", Value ="1" }
//                        },
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    }
//                }
//            };

//            // aggregate.
//            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
//            var result = aggregator.Aggregate(route);
//            var point = result;
//            Assert.IsNotNull(point);
//            Assert.AreEqual(0, point.Location.Latitude);
//            Assert.AreEqual(0, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            var arc = result.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName(), arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("transit.stop.id", "1"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(1, point.Location.Latitude);
//            Assert.AreEqual(1, point.Location.Longitude);
//            Assert.IsNull(point.Next);
//        }

//        /// <summary>
//        /// Tests aggregation of a transit route only.
//        /// </summary>
//        [Test]
//        public void Test4TransitOnly2Segments()
//        {
//            // build test route.
//            var route = new Route()
//            {
//                Segments = new RouteSegment[] 
//                {
//                    new RouteSegment()
//                    {
//                        Distance = 0,
//                        Latitude = 0,
//                        Longitude = 0,
//                        Time = 0,
//                        Type = RouteSegmentType.Start,
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 1,
//                        Latitude = 1,
//                        Time = 30,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "transit.stop.id", Value ="1" }
//                        },
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 2,
//                        Latitude = 2,
//                        Time = 60,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "transit.stop.id", Value ="2" }
//                        },
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    }
//                }
//            };

//            // aggregate.
//            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
//            var result = aggregator.Aggregate(route);
//            var point = result;
//            Assert.IsNotNull(point);
//            Assert.AreEqual(0, point.Location.Latitude);
//            Assert.AreEqual(0, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            var arc = result.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName(), arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("transit.stop.id", "1"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(1, point.Location.Latitude);
//            Assert.AreEqual(1, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName(), arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("transit.stop.id", "2"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(2, point.Location.Latitude);
//            Assert.AreEqual(2, point.Location.Longitude);
//            Assert.IsNull(point.Next);
//        }

//        /// <summary>
//        /// Tests aggregation of a transit route with a road part before and after.
//        /// </summary>
//        [Test]
//        public void Test5MultiModal1()
//        {
//            // build test route.
//            var route = new Route()
//            {
//                Segments = new RouteSegment[] 
//                {
//                    new RouteSegment()
//                    {
//                        Distance = 0,
//                        Latitude = 0,
//                        Longitude = 0,
//                        Time = 0,
//                        Type = RouteSegmentType.Start,
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 100,
//                        Longitude = 1,
//                        Latitude = 1,
//                        Time = 10,
//                        Type = RouteSegmentType.Along,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 200,
//                        Longitude = 2,
//                        Latitude = 2,
//                        Time = 20,
//                        Type = RouteSegmentType.Along,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 300,
//                        Longitude = 3,
//                        Latitude = 3,
//                        Time = 30,
//                        Type = RouteSegmentType.Along,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "transit.stop.id", Value ="1" }
//                        },
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 400,
//                        Longitude = 4,
//                        Latitude = 4,
//                        Time = 40,
//                        Type = RouteSegmentType.Along,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "transit.stop.id", Value ="2" }
//                        },
//                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 500,
//                        Longitude = 5,
//                        Latitude = 5,
//                        Time = 50,
//                        Type = RouteSegmentType.Along,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    },
//                    new RouteSegment()
//                    {
//                        Distance = 600,
//                        Longitude = 6,
//                        Latitude = 6,
//                        Time = 60,
//                        Type = RouteSegmentType.Stop,
//                        Tags = new RouteTags[] {
//                            new RouteTags() { Key = "highway", Value ="residential" }
//                        },
//                        Vehicle = "Car"
//                    }
//                }
//            };

//            // aggregate.
//            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
//            var result = aggregator.Aggregate(route);
//            var point = result;
//            Assert.IsNotNull(point);
//            Assert.AreEqual(0, point.Location.Latitude);
//            Assert.AreEqual(0, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            var arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual("Car", arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("highway", "residential"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(2, point.Location.Latitude);
//            Assert.AreEqual(2, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName(), arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("transit.stop.id", "1"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(3, point.Location.Latitude);
//            Assert.AreEqual(3, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName(), arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("transit.stop.id", "2"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(4, point.Location.Latitude);
//            Assert.AreEqual(4, point.Location.Longitude);
//            Assert.IsNotNull(point.Next);
//            arc = point.Next;
//            // TODO: fix distance and take this from the route instead of re-calculating.
//            // https://github.com/OsmSharp/OsmSharp/issues/248
//            // https://github.com/OsmSharp/OsmSharp/issues/249
//            //Assert.AreEqual(100, result.Next.Distance);
//            Assert.AreEqual("Car", arc.Vehicle);
//            Assert.IsNotNull(arc.Tags);
//            Assert.AreEqual(1, arc.Tags.Count);
//            Assert.IsTrue(arc.Tags.ContainsKeyValue("highway", "residential"));
//            Assert.IsNotNull(arc.Next);
//            point = arc.Next;
//            Assert.AreEqual(6, point.Location.Latitude);
//            Assert.AreEqual(6, point.Location.Longitude);
//            Assert.IsNull(point.Next);
//        }
//    }
//}