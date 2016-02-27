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
    /// Contains tests for the route builder of the profile search algorithm.
    /// </summary>
    /// <remarks>It's only possible to build a complete route when there is a GTFS-feed available. A bare connections db is only enough to calculate paths.</remarks>
    [TestFixture]
    class ProfileSearchRouteBuilderTests
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
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, connectionsDb);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Count);
            Assert.AreEqual(MockProfile.CarMock().Name, route.Segments[0].Profile);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, route.Segments[1].Profile);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteTypeExtended.BusService.ToProfileName(), 
                route.Segments[2].Profile);
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
            routerDb.Network.AddVertex(2, 2.2f, 2.2f);
            routerDb.Network.AddVertex(3, 2.1f, 2.1f);
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
            var algorithm = new ProfileSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, connectionsDb);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.AreEqual(4, route.Segments.Count);
            Assert.AreEqual(MockProfile.CarMock().Name, route.Segments[0].Profile);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, route.Segments[1].Profile);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteTypeExtended.BusService.ToProfileName(), 
                route.Segments[2].Profile);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteTypeExtended.BusService.ToProfileName(), 
                route.Segments[3].Profile);
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
                MetaId = 1,
                Profile = 1
            }, null);
            routerDb.Network.AddVertex(2, 1, 1);
            routerDb.Network.AddVertex(3, 1.1f, 1.1f);
            routerDb.Network.AddEdge(2, 3, new Routing.Network.Data.EdgeData()
            {
                Distance = 500,
                MetaId = 1,
                Profile = 1
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
            var algorithm = new ProfileSearch(connectionsDb, departureTime,
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(0, 1),
                        float.MaxValue, false),
                new ClosestStopSearch(routerDb, MockProfile.CarMock(), linksDb,
                    routerDb.Network.CreateRouterPointForVertex(3, 2),
                        float.MaxValue, true));
            algorithm.Run();

            // run routebuilder.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, connectionsDb);
            var route = routeBuilder.Build();

            Assert.IsNotNull(route);
            Assert.AreEqual(7, route.Segments.Count);
            Assert.AreEqual(MockProfile.CarMock().Name, route.Segments[0].Profile);
            Assert.AreEqual(MockProfile.CarMock().Name, route.Segments[1].Profile);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferProfile, route.Segments[2].Profile);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, route.Segments[3].Profile);
            Assert.AreEqual(GTFS.Entities.Enumerations.RouteTypeExtended.BusService.ToProfileName(), 
                route.Segments[4].Profile);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferProfile, route.Segments[5].Profile);
            Assert.AreEqual(MockProfile.CarMock().Name, route.Segments[6].Profile);
        }
    }
}