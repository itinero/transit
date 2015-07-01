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
    /// Contains tests for the multimodal version of the earliest arrival algorithm.
    /// </summary>
    [TestFixture]
    public class EarliestArrivalTests
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
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, 
                    new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, 
                    new PathSegmentVisitList(2), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(40 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 10, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }

        /// <summary>
        /// Tests an unsuccessful one-hop with a one-connection db.
        /// </summary>
        [Test]
        public void TestOneHopUnsuccessful()
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
            var departureTime = new DateTime(2017, 05, 10, 08, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(2), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsFalse(algorithm.HasSucceeded);
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
                        Minutes = 10
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 20
                    })),
                new OsmRoutingInterpreter());

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(3), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(50 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 20, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(2);
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 50 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(1, connection.DepartureStop);
            status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferSuccessful()
        {
            // build dummy db.
            var connectionsDb = new MultimodalConnectionsDb(
                new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex()),
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsTwoTrips(
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
                        Minutes = 15
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 25
                    })),
                new OsmRoutingInterpreter());

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(3), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(2);
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, status.Seconds);
            Assert.AreEqual(2, status.Transfers);
            Assert.AreEqual(1, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(1, connection.DepartureStop);
            status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer versus a one-hop connection without transfers with a three-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferVersusOneHopSuccessful()
        {
            // build dummy db.
            var connectionsDb = new MultimodalConnectionsDb(
                new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex()),
                new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsTransferVSNoTranfer(
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
                        Minutes = 15
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 25
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 15
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 25
                    })),
                new OsmRoutingInterpreter());

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(3), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(2);
            Assert.AreEqual(2, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(2, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
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
            connectionsDb.Graph.SetVertex(0, 0.004484f, 0f);
            connectionsDb.Graph.SetVertex(1, 0.010514f, 0f);
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
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex4), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            var duration500m = (int)(500 / ((OsmSharp.Units.Speed.MeterPerSecond)Vehicle.Pedestrian.ProbableSpeed(tagsIndex.Get(tagsId))).Value);
            var duration = 40 * 60 + duration500m;
            Assert.AreEqual(duration, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 07, 30, 00).AddSeconds(duration), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + duration500m, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
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
            connectionsDb.Graph.SetVertex(vertex2, 0.004484f, 0f);
            connectionsDb.Graph.SetVertex(vertex3, 0.139560f, 0f);
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
            var algorithm = new EarliestArrival(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex1), 1000, true),
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian, new PathSegmentVisitList(vertex4), 1000, false));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            var duration500m = (int)(500 / ((OsmSharp.Units.Speed.MeterPerSecond)Vehicle.Pedestrian.ProbableSpeed(tagsIndex.Get(tagsId))).Value);
            var duration = 40 * 60 + duration500m;
            Assert.AreEqual(duration, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 07, 30, 00).AddSeconds(duration), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + duration500m, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }
    }
}