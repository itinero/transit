﻿// OsmSharp - OpenStreetMap (OSM) SDK
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

using GTFS.Entities;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using System;

namespace OsmSharp.Transit.Test.Multimodal.Algorithms.OneToOne
{
    /// <summary>
    /// Contains tests for the route builder of the profile search algorithm.
    /// </summary>
    /// <remarks>It's only possible to build a complete route when there is a GTFS-feed available. A bare connections db is only enough to calculate paths.</remarks>
    [TestFixture]
    class OneToManyProfileSearchRouteBuilderTests
    {
        /// <summary>
        /// Tests a sucessful one-hop with a one-connection db.
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build dummy db.
            var connectionsDb = new MultimodalConnectionsDb(
                new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex()),
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })),
                new OsmRoutingInterpreter());

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new OneToManyProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(2), 1000, false));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new OneToManyProfileSearchRouteBuilder(algorithm, connectionsDb, 0);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.IsNull(route.Vehicle);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[0].Vehicle);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitVehicle, route.Segments[1].Vehicle);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Tram.ToVehicleUniqueName(), route.Segments[2].Vehicle);
        }

        /// <summary>
        /// Tests a successful two-hop with a two-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessful()
        {
            // build dummy db.
            var connectionsDb = new MultimodalConnectionsDb(
                new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex()),
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsOneTrip(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 11
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 20
                    })),
                new OsmRoutingInterpreter());

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new OneToManyProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(3), 1000, false));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new OneToManyProfileSearchRouteBuilder(algorithm, connectionsDb, 0);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.IsNull(route.Vehicle);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[0].Vehicle);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitVehicle, route.Segments[1].Vehicle);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Tram.ToVehicleUniqueName(), route.Segments[2].Vehicle);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Tram.ToVehicleUniqueName(), route.Segments[3].Vehicle);
        }

        /// <summary>
        /// Tests a successful one-hop with a one-connection db and with before- and after road segments.
        /// </summary>
        [Test]
        public void TestOneHopWithBeforeAndAfter()
        {
            // build dummy db.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var connectionsDb = new MultimodalConnectionsDb(
                graph,
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })),
                new OsmRoutingInterpreter());
            var tagsIndex = (TagsIndex)connectionsDb.Graph.TagsIndex;
            var tagsId = tagsIndex.Add(new TagsCollection(new Tag() { Key = "highway", Value = "residential" }));
            var vertex1 = connectionsDb.Graph.AddVertex(0.000000f, 0f);
            var vertex2 = (uint)1;
            var vertex3 = (uint)2;
            var vertex4 = connectionsDb.Graph.AddVertex(0.014998f, 0f);
            connectionsDb.Graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });
            connectionsDb.Graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new OneToManyProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex1), 1000, false),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex4), 1000, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new OneToManyProfileSearchRouteBuilder(algorithm, connectionsDb, 0);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.IsNull(route.Vehicle);
            Assert.AreEqual(5, route.Segments.Length);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[0].Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[1].Vehicle);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitVehicle, route.Segments[2].Vehicle);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Tram.ToVehicleUniqueName(), route.Segments[3].Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[4].Vehicle);
        }

        /// <summary>
        /// Tests a successful one-hop with a one-connection db and with before- and after road segments that competes with a 15km stretch of road.
        /// </summary>
        [Test]
        public void TestOneHopWithBeforeAndAfterVersus15kmRoad()
        {
            // build dummy db.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var connectionsDb = new MultimodalConnectionsDb(
                graph,
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })),
                new OsmRoutingInterpreter());
            var tagsIndex = (TagsIndex)connectionsDb.Graph.TagsIndex;
            var tagsId = tagsIndex.Add(new TagsCollection(new Tag() { Key = "highway", Value = "residential" }));
            var vertex1 = connectionsDb.Graph.AddVertex(0.000000f, 0f);
            var vertex2 = (uint)1;
            var vertex3 = (uint)2;
            var vertex4 = connectionsDb.Graph.AddVertex(0.144045f, 0f);
            //connectionsDb.Graph.SetVertex(vertex2, 0.004484f, 0f);
            //connectionsDb.Graph.SetVertex(vertex3, 0.139560f, 0f);
            connectionsDb.Graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });
            connectionsDb.Graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Distance = 15000,
                Forward = true,
                Tags = tagsId
            });
            connectionsDb.Graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new OneToManyProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex1), 1000, false),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex4), 1000, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new OneToManyProfileSearchRouteBuilder(algorithm, connectionsDb, 0);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.IsNull(route.Vehicle);
            Assert.AreEqual(5, route.Segments.Length);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[0].Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[1].Vehicle);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitVehicle, route.Segments[2].Vehicle);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteType.Tram.ToVehicleUniqueName(), route.Segments[3].Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[4].Vehicle);
        }

        /// <summary>
        /// Tests a one hop route without any transit data.
        /// </summary>
        [Test]
        public void TestOneHopNoTransit()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var connectionsDb = new MultimodalConnectionsDb(graph,
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.Empty()),
                new OsmRoutingInterpreter(), Vehicle.Pedestrian);
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new OneToManyProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex1), 1000, false),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex2), 1000, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new OneToManyProfileSearchRouteBuilder(algorithm, connectionsDb, 0);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[0].Vehicle);
            Assert.AreEqual(2, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, route.Segments[1].Vehicle);
            Assert.AreEqual(500, route.Segments[1].Distance);
            Assert.AreEqual(500 / Vehicle.Pedestrian.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
        }
    }
}