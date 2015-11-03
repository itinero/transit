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
using OsmSharp.Routing;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.Default;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Transit.Test.Data.GTFS;
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
                {
                    Distance = 100,
                    MetaId = 0,
                    Profile = 0
                }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build connections db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })));

            // build links db.
            var linksDb = new StopLinksDb(2);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(0, 1));
            linksDb.Add(1, routerDb.Network.CreateRouterPointForVertex(3, 2));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb, 
                    routerDb.Network.CreateRouterPointForVertex(0, 1), 
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb, 
                    routerDb.Network.CreateRouterPointForVertex(3, 2), 
                        float.MaxValue, true));
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build dummy db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })));

            // build links db.
            var linksDb = new StopLinksDb(2);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(0, 1));
            linksDb.Add(1, routerDb.Network.CreateRouterPointForVertex(3, 2));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 08, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build dummy db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.TwoConnectionsOneTrip(
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
                    })));

            // build links db.
            var linksDb = new StopLinksDb(3);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(0, 1));
            linksDb.Add(2, routerDb.Network.CreateRouterPointForVertex(3, 2));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build dummy db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.TwoConnectionsTwoTrips(
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
                    })));

            // build links db.
            var linksDb = new StopLinksDb(3);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(0, 1));
            linksDb.Add(2, routerDb.Network.CreateRouterPointForVertex(3, 2));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 100,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build dummy db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsTransferVSNoTranfer(
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
                    })));

            // build links db.
            var linksDb = new StopLinksDb(4);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(0, 1));
            linksDb.Add(2, routerDb.Network.CreateRouterPointForVertex(3, 2));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
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
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0.1f, 0.1f);
            routerDb.Network.AddEdge(0, 1, new Routing.Network.Data.EdgeData()
            {
                Distance = 500,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 500,
                MetaId = 0,
                Profile = 0
            }, null);
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());

            // build dummy db.
            var connectionsDb = new MultimodalDb(
                routerDb,
                new GTFSConnectionsDb(GTFSConnectionsDbBuilder.OneConnection(
                    new TimeOfDay()
                    {
                        Hours = 8
                    },
                    new TimeOfDay()
                    {
                        Hours = 8,
                        Minutes = 10
                    })));

            // build links db.
            var linksDb = new StopLinksDb(2);
            linksDb.Add(0, routerDb.Network.CreateRouterPointForVertex(1, 0));
            linksDb.Add(1, routerDb.Network.CreateRouterPointForVertex(2, 3));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            var duration500m = 500 * MockProfile.CarMock().Factor(null).Value;
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