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

using NUnit.Framework;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Instructions.Modal;
using System.Reflection;
using System.Xml.Serialization;

namespace OsmSharp.Transit.Test.Multimodal.Instructions.Modal
{
    /// <summary>
    /// Contains tests for the modal instruction generation.
    /// </summary>
    [TestFixture]
    class ModalInstructionsTests
    {
        /// <summary>
        /// Tests aggregation of a route only.
        /// </summary>
        [Test]
        public void Test1RoadOnly1Segment()
        {
            // build test route.
            var route = new Route()
            {
                Segments = new RouteSegment[] 
                {
                    new RouteSegment()
                    {
                        Distance = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Time = 0,
                        Type = RouteSegmentType.Start,
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 100,
                        Longitude = 1,
                        Latitude = 1,
                        Time = 30,
                        Type = RouteSegmentType.Stop,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    }
                }
            };

            // aggregate.
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual(0, instructions[0].FirstSegmentIdx);
            Assert.AreEqual(1, instructions[0].LastSegmentIdx);
            Assert.AreEqual(0, instructions[0].Location.MinLat);
            Assert.AreEqual(1, instructions[0].Location.MaxLat);
            Assert.AreEqual(0, instructions[0].Location.MinLon);
            Assert.AreEqual(1, instructions[0].Location.MaxLon);

            Assert.IsTrue(instructions[0].MetaData.ContainsKey("highway") &&
                instructions[0].MetaData["highway"].Equals("residential"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[0].MetaData["osmsharp.instruction.time"] == 30);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[0].MetaData["osmsharp.instruction.type"].Equals("anything_but_transit"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[0].MetaData["osmsharp.instruction.duration"] == 30);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.distance"] == 100);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.total_distance"] == 100);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[0].MetaData["osmsharp.instruction.vehicle"].Equals("Car"));
        }

        /// <summary>
        /// Tests aggregation of a route only.
        /// </summary>
        [Test]
        public void Test2RoadOnly2Segments()
        {
            // build test route.
            var route = new Route()
            {
                Segments = new RouteSegment[] 
                {
                    new RouteSegment()
                    {
                        Distance = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Time = 0,
                        Type = RouteSegmentType.Start,
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 100,
                        Longitude = 1,
                        Latitude = 1,
                        Time = 10,
                        Type = RouteSegmentType.Stop,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 200,
                        Longitude = 2,
                        Latitude = 2,
                        Time = 20,
                        Type = RouteSegmentType.Stop,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    }
                }
            };

            // aggregate.
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual(0, instructions[0].FirstSegmentIdx);
            Assert.AreEqual(2, instructions[0].LastSegmentIdx);
            Assert.AreEqual(0, instructions[0].Location.MinLat);
            Assert.AreEqual(2, instructions[0].Location.MaxLat);
            Assert.AreEqual(0, instructions[0].Location.MinLon);
            Assert.AreEqual(2, instructions[0].Location.MaxLon);

            Assert.IsTrue(instructions[0].MetaData.ContainsKey("highway") &&
                instructions[0].MetaData["highway"].Equals("residential"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[0].MetaData["osmsharp.instruction.time"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[0].MetaData["osmsharp.instruction.type"].Equals("anything_but_transit"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[0].MetaData["osmsharp.instruction.duration"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.total_distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[0].MetaData["osmsharp.instruction.vehicle"].Equals("Car"));
        }

        /// <summary>
        /// Tests aggregation of a transit route only.
        /// </summary>
        [Test]
        public void Test3TransitOnly1Segment()
        {
            // build test route.
            var route = new Route()
            {
                Segments = new RouteSegment[] 
                {
                    new RouteSegment()
                    {
                        Distance = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Time = 0,
                        Type = RouteSegmentType.Start,
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 100,
                        Longitude = 1,
                        Latitude = 1,
                        Time = 10,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "transit.stop.id", Value ="1" },
                            new RouteTags() { Key = "transit.trip.id", Value ="1" }
                        },
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    }
                }
            };

            // aggregate.
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual(0, instructions[0].FirstSegmentIdx);
            Assert.AreEqual(1, instructions[0].LastSegmentIdx);
            Assert.AreEqual(0, instructions[0].Location.MinLat);
            Assert.AreEqual(1, instructions[0].Location.MaxLat);
            Assert.AreEqual(0, instructions[0].Location.MinLon);
            Assert.AreEqual(1, instructions[0].Location.MaxLon);

            Assert.IsTrue(instructions[0].MetaData.ContainsKey("transit.trip.id") &&
                instructions[0].MetaData["transit.trip.id"].Equals("1"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[0].MetaData["osmsharp.instruction.time"] == 10);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[0].MetaData["osmsharp.instruction.type"].Equals("transit"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[0].MetaData["osmsharp.instruction.duration"] == 10);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.distance"] == 100);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.total_distance"] == 100);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[0].MetaData["osmsharp.instruction.vehicle"].Equals(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()));
        }

        /// <summary>
        /// Tests aggregation of a transit route only.
        /// </summary>
        [Test]
        public void Test4TransitOnly2Segments()
        {
            // build test route.
            var route = new Route()
            {
                Segments = new RouteSegment[] 
                {
                    new RouteSegment()
                    {
                        Distance = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Time = 0,
                        Type = RouteSegmentType.Start,
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 100,
                        Longitude = 1,
                        Latitude = 1,
                        Time = 10,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "transit.stop.id", Value ="1" },
                            new RouteTags() { Key = "transit.trip.id", Value ="1" }
                        },
                        Points = new RoutePoint[]
                        {
                            new RoutePoint() {
                                Latitude = -1,
                                Longitude = -1,
                                Name = "Stop1",
                                Tags =new RouteTags[] {
                                    new RouteTags() { Key = "transit.stop.id", Value ="1" },
                                    new RouteTags() { Key = "transit.stop.name", Value ="Stop1" }
                                }
                            }
                        },
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 200,
                        Longitude = 2,
                        Latitude = 2,
                        Time = 20,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "transit.stop.id", Value ="2" },
                            new RouteTags() { Key = "transit.trip.id", Value ="1" }
                        },
                        Points = new RoutePoint[]
                        {
                            new RoutePoint() {
                                Latitude = -2,
                                Longitude = -2,
                                Name = "Stop2",
                                Tags =new RouteTags[] {
                                    new RouteTags() { Key = "transit.stop.id", Value ="2" },
                                    new RouteTags() { Key = "transit.stop.name", Value ="Stop2" }
                                }
                            }
                        },
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    }
                }
            };

            // aggregate.
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            Assert.IsNotNull(instructions);
            Assert.AreEqual(1, instructions.Count);
            Assert.AreEqual(0, instructions[0].FirstSegmentIdx);
            Assert.AreEqual(2, instructions[0].LastSegmentIdx);
            Assert.AreEqual(0, instructions[0].Location.MinLat);
            Assert.AreEqual(2, instructions[0].Location.MaxLat);
            Assert.AreEqual(0, instructions[0].Location.MinLon);
            Assert.AreEqual(2, instructions[0].Location.MaxLon);

            Assert.IsTrue(instructions[0].MetaData.ContainsKey("transit.trip.id") &&
                instructions[0].MetaData["transit.trip.id"].Equals("1"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[0].MetaData["osmsharp.instruction.time"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[0].MetaData["osmsharp.instruction.type"].Equals("transit"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[0].MetaData["osmsharp.instruction.duration"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.total_distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[0].MetaData["osmsharp.instruction.vehicle"].Equals(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()));
        }

        /// <summary>
        /// Tests aggregation of a transit route with a road part before and after.
        /// </summary>
        [Test]
        public void Test5MultiModal1()
        {
            // build test route.
            var route = new Route()
            {
                Segments = new RouteSegment[] 
                {
                    new RouteSegment()
                    {
                        Distance = 0,
                        Latitude = 0,
                        Longitude = 0,
                        Time = 0,
                        Type = RouteSegmentType.Start,
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 100,
                        Longitude = 1,
                        Latitude = 1,
                        Time = 10,
                        Type = RouteSegmentType.Along,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 200,
                        Longitude = 2,
                        Latitude = 2,
                        Time = 20,
                        Type = RouteSegmentType.Along,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 300,
                        Longitude = 3,
                        Latitude = 3,
                        Time = 30,
                        Type = RouteSegmentType.Along,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "transit.stop.id", Value ="1" },
                            new RouteTags() { Key = "transit.trip.id", Value ="1" }
                        },
                        Points = new RoutePoint[]
                        {
                            new RoutePoint() {
                                Latitude = -1,
                                Longitude = -1,
                                Name = "Stop1",
                                Tags =new RouteTags[] {
                                    new RouteTags() { Key = "transit.stop.id", Value ="1" },
                                    new RouteTags() { Key = "transit.stop.name", Value ="Stop1" }
                                }
                            }
                        },
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 400,
                        Longitude = 4,
                        Latitude = 4,
                        Time = 40,
                        Type = RouteSegmentType.Along,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "transit.stop.id", Value ="2" },
                            new RouteTags() { Key = "transit.trip.id", Value ="1" }
                        },
                        Points = new RoutePoint[]
                        {
                            new RoutePoint() {
                                Latitude = -2,
                                Longitude = -2,
                                Name = "Stop2",
                                Tags =new RouteTags[] {
                                    new RouteTags() { Key = "transit.stop.id", Value ="2" },
                                    new RouteTags() { Key = "transit.stop.name", Value ="Stop2" }
                                }
                            }
                        },
                        Vehicle = GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()
                    },
                    new RouteSegment()
                    {
                        Distance = 500,
                        Longitude = 5,
                        Latitude = 5,
                        Time = 50,
                        Type = RouteSegmentType.Along,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    },
                    new RouteSegment()
                    {
                        Distance = 600,
                        Longitude = 6,
                        Latitude = 6,
                        Time = 60,
                        Type = RouteSegmentType.Stop,
                        Tags = new RouteTags[] {
                            new RouteTags() { Key = "highway", Value ="residential" }
                        },
                        Vehicle = "Car"
                    }
                }
            };

            // aggregate.
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            Assert.IsNotNull(instructions);
            Assert.AreEqual(3, instructions.Count);
            Assert.AreEqual(0, instructions[0].FirstSegmentIdx);
            Assert.AreEqual(2, instructions[0].LastSegmentIdx);
            Assert.AreEqual(0, instructions[0].Location.MinLat);
            Assert.AreEqual(2, instructions[0].Location.MaxLat);
            Assert.AreEqual(0, instructions[0].Location.MinLon);
            Assert.AreEqual(2, instructions[0].Location.MaxLon);

            Assert.AreEqual(2, instructions[1].FirstSegmentIdx);
            Assert.AreEqual(4, instructions[1].LastSegmentIdx);
            Assert.AreEqual(2, instructions[1].Location.MinLat);
            Assert.AreEqual(4, instructions[1].Location.MaxLat);
            Assert.AreEqual(2, instructions[1].Location.MinLon);
            Assert.AreEqual(4, instructions[1].Location.MaxLon);

            Assert.AreEqual(4, instructions[2].FirstSegmentIdx);
            Assert.AreEqual(6, instructions[2].LastSegmentIdx);
            Assert.AreEqual(4, instructions[2].Location.MinLat);
            Assert.AreEqual(6, instructions[2].Location.MaxLat);
            Assert.AreEqual(4, instructions[2].Location.MinLon);
            Assert.AreEqual(6, instructions[2].Location.MaxLon);

            Assert.IsTrue(instructions[0].MetaData.ContainsKey("highway") &&
                instructions[0].MetaData["highway"].Equals("residential"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[0].MetaData["osmsharp.instruction.time"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[0].MetaData["osmsharp.instruction.type"].Equals("anything_but_transit"));
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[0].MetaData["osmsharp.instruction.duration"] == 20);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[0].MetaData["osmsharp.instruction.total_distance"] == 200);
            Assert.IsTrue(instructions[0].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[0].MetaData["osmsharp.instruction.vehicle"].Equals("Car"));

            Assert.IsTrue(instructions[1].MetaData.ContainsKey("transit.trip.id") &&
                instructions[1].MetaData["transit.trip.id"].Equals("1"));
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[1].MetaData["osmsharp.instruction.time"] == 40);
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[1].MetaData["osmsharp.instruction.type"].Equals("transit"));
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[1].MetaData["osmsharp.instruction.duration"] == 20);
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[1].MetaData["osmsharp.instruction.distance"] == 200);
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[1].MetaData["osmsharp.instruction.total_distance"] == 400);
            Assert.IsTrue(instructions[1].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[1].MetaData["osmsharp.instruction.vehicle"].Equals(GTFS.Entities.Enumerations.RouteType.Bus.ToVehicleUniqueName()));

            Assert.IsTrue(instructions[2].MetaData.ContainsKey("highway") &&
                instructions[2].MetaData["highway"].Equals("residential"));
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.time") &&
                (double)instructions[2].MetaData["osmsharp.instruction.time"] == 60);
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.type") &&
                instructions[2].MetaData["osmsharp.instruction.type"].Equals("anything_but_transit"));
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.duration") &&
                (double)instructions[2].MetaData["osmsharp.instruction.duration"] == 20);
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.distance") &&
                (double)instructions[2].MetaData["osmsharp.instruction.distance"] == 200);
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.total_distance") &&
                (double)instructions[2].MetaData["osmsharp.instruction.total_distance"] == 600);
            Assert.IsTrue(instructions[2].MetaData.ContainsKey("osmsharp.instruction.vehicle") &&
                instructions[2].MetaData["osmsharp.instruction.vehicle"].Equals("Car"));
        }

        /// <summary>
        /// Tests a route that used make the micro planner crash.
        /// </summary>
        [Test]
        public void Test6Regression1()
        {
            var xmlSerializer = new XmlSerializer(typeof(Route));
            var route = xmlSerializer.Deserialize(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Transit.Test.test_data.routes.regression1.route")) as Route;

            // generate instructions
            var aggregator = new ModalAggregator(new OsmRoutingInterpreter());
            var microPlanner = new ModalMicroPlanner(new ModalLanguageGenerator(), new OsmRoutingInterpreter());
            var aggregated = aggregator.Aggregate(route);
            var instructions = InstructionGenerator.Generate(microPlanner, route, aggregated);

            // it's enough this works without exceptions, just check for a result.
            Assert.IsNotNull(instructions);
            Assert.IsTrue(instructions.Count > 0);
        }
    }
}
