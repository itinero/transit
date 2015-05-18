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
using OsmSharp.Routing.Transit.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Transit.Test.Data;
using System;

namespace OsmSharp.Transit.Test.Algorithms.OneToOne
{
    /// <summary>
    /// A test fixture for the earliest arrival algorithm.
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
            // build stub db.
            var connectionsDb = new StubConnectionsDb();
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 0 * 60 + 3600 * 8, // departure at 08:00
                ArrivalStop = 1,
                ArrivalTime = 10 * 60 + 3600 * 8, // arrival at 08:10
                TripId = 0
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, 0, 1, departureTime);
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
            Assert.AreEqual(-1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(-1, status.TripId);
        }

        /// <summary>
        /// Tests an unsuccessful one-hop with a one-connection db.
        /// </summary>
        [Test]
        public void TestOneHopUnsuccessful()
        {
            // build stub db.
            var connectionsDb = new StubConnectionsDb();
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 0 * 60 + 3600 * 8, // departure at 08:00
                ArrivalStop = 1,
                ArrivalTime = 10 * 60 + 3600 * 8, // arrival at 08:10
                TripId = 0
            });

            // run algorithm.
            var algorithm = new EarliestArrival(connectionsDb, 0, 1, new DateTime(2017, 05, 10, 08, 30, 00));
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
            // build stub db.
            var connectionsDb = new StubConnectionsDb();
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 0 * 60 + 3600 * 8, // departure at 08:00
                ArrivalStop = 1,
                ArrivalTime = 10 * 60 + 3600 * 8, // arrival at 08:10
                TripId = 0
            });
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 1,
                DepartureTime = 11 * 60 + 3600 * 8, // departure at 08:11
                ArrivalStop = 2,
                ArrivalTime = 20 * 60 + 3600 * 8, // arrival at 08:20
                TripId = 0
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, 0, 2, departureTime);
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
            Assert.AreEqual(-1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(-1, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferSuccessful()
        {
            // build stub db.
            var connectionsDb = new StubConnectionsDb();
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 0 * 60 + 3600 * 8, // departure at 08:00
                ArrivalStop = 1,
                ArrivalTime = 10 * 60 + 3600 * 8, // arrival at 08:10
                TripId = 0
            });
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 1,
                DepartureTime = 15 * 60 + 3600 * 8, // departure at 08:15
                ArrivalStop = 2,
                ArrivalTime = 25 * 60 + 3600 * 8, // arrival at 08:25
                TripId = 1
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, 0, 2, departureTime);
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
            Assert.AreEqual(-1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(-1, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer versus a one-how connection without transfers with a three-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferVersusOneHowSuccessful()
        {
            // build stub db.
            var connectionsDb = new StubConnectionsDb();
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 0 * 60 + 3600 * 8, // departure at 08:00
                ArrivalStop = 1,
                ArrivalTime = 10 * 60 + 3600 * 8, // arrival at 08:10
                TripId = 0
            });
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 0,
                DepartureTime = 10 * 60 + 3600 * 8, // departure at 08:10
                ArrivalStop = 2,
                ArrivalTime = 25 * 60 + 3600 * 8, // arrival at 08:25
                TripId = 2
            });
            connectionsDb.DepartureTimeConnections.Add(new Connection()
            {
                DepartureStop = 1,
                DepartureTime = 15 * 60 + 3600 * 8, // departure at 08:15
                ArrivalStop = 2,
                ArrivalTime = 25 * 60 + 3600 * 8, // arrival at 08:25
                TripId = 1
            });

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrival(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(2);
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(2, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(-1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(-1, status.TripId);
        }
    }
}