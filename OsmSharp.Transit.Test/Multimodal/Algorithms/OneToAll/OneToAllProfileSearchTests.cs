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
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToAll;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Routing.Vehicles;
using System;

namespace OsmSharp.Transit.Test.Multimodal.Algorithms.OneToAll
{
    /// <summary>
    /// Contains tests for the one-to-all profile search.
    /// </summary>
    [TestFixture]
    public class OneToAllProfileSearchTests
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
            var algorithm = new OneToAllProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian,
                    new PathSegmentVisitList(1), 1000, true), int.MaxValue);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var profiles = algorithm.GetStopProfiles(1);
            var profile = profiles[0];
            Assert.AreEqual(0, profile.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.ConnectionId);
            Assert.AreEqual(0, connection.TripId);
            Assert.AreEqual(0, connection.DepartureStop);
            profiles = algorithm.GetStopProfiles(0);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
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
            var algorithm = new OneToAllProfileSearch(connectionsDb, departureTime,
                new OneToManyDykstra(connectionsDb.Graph, new OsmRoutingInterpreter(), Vehicle.Pedestrian,
                    new PathSegmentVisitList(1), 1000, true), int.MaxValue);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var profiles = algorithm.GetStopProfiles(1);
            Assert.AreEqual(0, profiles.Count);
        }
    }
}
