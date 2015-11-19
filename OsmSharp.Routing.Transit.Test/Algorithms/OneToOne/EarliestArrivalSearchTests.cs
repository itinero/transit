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
    public class EarliestArrivalSearchTests
    {
        /// <summary>
        /// Tests a sucessful one-hop with a one-connection db.
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build dummy db.
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.OneConnection(
                new TimeOfDay()
                {
                    Hours = 8
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 1, departureTime);
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
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.OneConnection(
                new TimeOfDay()
                {
                    Hours = 8
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10
                }));

            // run algorithm.
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 1, new DateTime(2017, 05, 10, 08, 30, 00));
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
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsOneTrip(
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
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 2, departureTime);
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
        /// Tests a successful two-hop with a two-connection db but with identical arrival and departure times.
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessfulIdenticalTimes()
        {
            // build dummy db.
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsOneTrip(
                new TimeOfDay()
                {
                    Hours = 8
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(30 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 0, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(2);
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 30 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(1, connection.DepartureStop);
            status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 30 * 60, status.Seconds);
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
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsTwoTrips(
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
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 2, departureTime);
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
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsTransferVSNoTranfer(
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
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 2, departureTime);
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
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferCloseStopsSuccessful()
        {
            // build dummy db.
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.TwoConnectionsTwoTripsCloseStops(
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
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 3, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(3); // stop 3.
            Assert.AreEqual(2, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, status.Seconds);
            Assert.AreEqual(2, status.Transfers);
            Assert.AreEqual(1, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId); // connection 2->3.
            Assert.AreEqual(2, connection.DepartureStop);
            Assert.AreEqual(1, status.TripId);
            status = algorithm.GetStopStatus(2); // stop 2.
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, status.Seconds);
            Assert.AreEqual(2, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId); // connection 1->2.
            Assert.AreEqual(1, connection.DepartureStop);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, connection.TripId);
            status = algorithm.GetStopStatus(1); // stop 1.
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, status.TripId);
            status = algorithm.GetStopStatus(0); // stop 0.
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a three-connection db where one pseudo connection is skipped.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferCloseStopsSuccessfulSkippedPseudo()
        {
            // build dummy db.
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsCloseStops(
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
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 3, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var status = algorithm.GetStopStatus(3); // stop 3.
            Assert.AreEqual(2, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, status.Seconds);
            Assert.AreEqual(2, status.Transfers);
            Assert.AreEqual(1, status.TripId);
            var connection = algorithm.GetConnection(status.ConnectionId); // connection 2->3.
            Assert.AreEqual(2, connection.DepartureStop);
            Assert.AreEqual(1, status.TripId);
            status = algorithm.GetStopStatus(2); // stop 2.
            Assert.AreEqual(1, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, status.Seconds);
            Assert.AreEqual(2, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId); // connection 1->2.
            Assert.AreEqual(1, connection.DepartureStop);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, connection.TripId);
            status = algorithm.GetStopStatus(1); // stop 1.
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            Assert.AreEqual(0, status.TripId);
            connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, status.TripId);
            status = algorithm.GetStopStatus(0); // stop 0.
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoTripId, status.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a four-connection db where one pseudo connection is skipped.
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferVSOneTripSuccessfulSkippedPseudo()
        {
            // build dummy db.
            var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsTransferVSNoTranferWithStop(
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
                    Minutes = 20
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 20
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 25
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new EarliestArrivalSearch(connectionsDb, 0, 2, departureTime);
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
            var connection = algorithm.GetConnection(status.ConnectionId); // connection 1->2.
            Assert.AreEqual(1, connection.DepartureStop);
            Assert.AreEqual(1, connection.TripId);
            status = algorithm.GetStopStatus(1);
            Assert.AreEqual(0, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, status.Seconds);
            Assert.AreEqual(1, status.Transfers);
            connection = algorithm.GetConnection(status.ConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, connection.TripId);
            status = algorithm.GetStopStatus(0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, status.ConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, status.Seconds);
            Assert.AreEqual(0, status.Transfers);
        }
    }
}