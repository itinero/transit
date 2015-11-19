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
    /// A test fixture for the profile search algorithm.
    /// </summary>
    [TestFixture]
    public class ProfileSearchTests
    {
        /// <summary>
        /// Tests a successful one-hop with a one-connection db.
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 1, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(40 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 10, 00), algorithm.ArrivalTime());

            var profiles = algorithm.GetStopProfiles(1);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.TripId);
            Assert.AreEqual(0, connection.DepartureStop);
            profiles = algorithm.GetStopProfiles(0);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 1, new DateTime(2017, 05, 10, 08, 30, 00));
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(50 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 20, 00), algorithm.ArrivalTime());

            int precedingStop = OsmSharp.Routing.Transit.Constants.NoStopId;
            int transfers = OsmSharp.Routing.Transit.Constants.NoTransfers;

            // get stop 2 profiles.
            var profiles = algorithm.GetStopProfiles(2);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 50 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.TripId);
            Assert.AreEqual(1, connection.DepartureStop);

            // get preceding profile and check if stop 1 profile.
            profile = algorithm.GetPreceding(profiles, 1, out precedingStop, out transfers);
            Assert.AreEqual(1, transfers);
            Assert.AreEqual(1, precedingStop);
            profiles = algorithm.GetStopProfiles(precedingStop);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            Assert.AreEqual(0, profile.PreviousConnectionId);
            connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, connection.TripId);

            // get preceding profile and check if stop 0 profile.
            profile = algorithm.GetPreceding(profiles, transfers, out precedingStop, out transfers);
            Assert.AreEqual(0, transfers);
            Assert.AreEqual(0, precedingStop);
            profiles = algorithm.GetStopProfiles(precedingStop);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
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
                },
                new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0
                }));

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(30 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 0, 00), algorithm.ArrivalTime());

            var precedingStop = OsmSharp.Routing.Transit.Constants.NoStopId;
            var transfers = OsmSharp.Routing.Transit.Constants.NoTransfers;

            // get stop 2 profiles.
            var profiles = algorithm.GetStopProfiles(2);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 30 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.TripId);
            Assert.AreEqual(1, connection.DepartureStop);

            // get preceding profile and check if stop 1 profile.
            profile = algorithm.GetPreceding(profiles, 1, out precedingStop, out transfers);
            Assert.AreEqual(1, transfers);
            Assert.AreEqual(1, precedingStop);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 30 * 60, profile.Seconds);
            Assert.AreEqual(0, profile.PreviousConnectionId);
            connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, connection.TripId);
            profiles = algorithm.GetStopProfiles(precedingStop);

            // get preceding profile and check if stop 0 profile.
            profile = algorithm.GetPreceding(profiles, transfers, out precedingStop, out transfers);
            Assert.AreEqual(0, transfers);
            Assert.AreEqual(0, precedingStop);
            profiles = algorithm.GetStopProfiles(precedingStop);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var precedingStop = OsmSharp.Routing.Transit.Constants.NoStopId;
            var transfers = OsmSharp.Routing.Transit.Constants.NoTransfers;

            // get stop 2 profiles.
            var profiles = algorithm.GetStopProfiles(2);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(1, connection.TripId);
            Assert.AreEqual(1, connection.DepartureStop);

            // get preceding profile and check if stop 1 profile.
            profile = algorithm.GetPreceding(profiles, 2, out precedingStop, out transfers);
            Assert.AreEqual(1, transfers);
            Assert.AreEqual(1, precedingStop);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            Assert.AreEqual(0, profile.PreviousConnectionId);
            connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, connection.TripId);
            profiles = algorithm.GetStopProfiles(precedingStop);

            // get preceding profile and check if stop 0 profile.
            profile = algorithm.GetPreceding(profiles, transfers, out precedingStop, out transfers);
            Assert.AreEqual(0, transfers);
            Assert.AreEqual(0, precedingStop);
            profiles = algorithm.GetStopProfiles(precedingStop);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var precedingStop = OsmSharp.Routing.Transit.Constants.NoStopId;
            var transfers = OsmSharp.Routing.Transit.Constants.NoTransfers;

            // get profiles at stop 2.
            var profiles = algorithm.GetStopProfiles(2);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(2, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(2, connection.TripId);

            // get previous profile and check this is stop 0.
            profile = algorithm.GetPreceding(profiles, 1, out precedingStop, out transfers);
            profiles = algorithm.GetStopProfiles(0);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);

            // check the profiles at stop 1.
            profiles = algorithm.GetStopProfiles(1);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            connection = algorithm.GetConnection(profile.PreviousConnectionId);
            Assert.AreEqual(0, connection.DepartureStop);
            Assert.AreEqual(0, connection.TripId);
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 3, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var profiles = algorithm.GetStopProfiles(3);
            //var profile = profiles.GetBest();
            //Assert.AreEqual(2, profile.ConnectionId);
            //Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
            //Assert.AreEqual(1, profile.Transfers);
            //var connection = algorithm.GetConnection(profile.ConnectionId); // connection 2->3.
            //Assert.AreEqual(2, connection.DepartureStop);
            //Assert.AreEqual(1, connection.TripId);
            //profiles = algorithm.GetStopProfiles(2);
            //profile = profiles.GetBest();
            //Assert.AreEqual(1, profile.ConnectionId);
            //Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, profile.Seconds);
            //Assert.AreEqual(1, profile.Transfers);
            //connection = algorithm.GetConnection(profile.ConnectionId); // connection 1->2.
            //Assert.AreEqual(1, connection.DepartureStop);
            //Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, connection.TripId);
            //profiles = algorithm.GetStopProfiles(1);
            //profile = profiles.GetBest();
            //Assert.AreEqual(0, profile.ConnectionId);
            //Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            //Assert.AreEqual(0, profile.Transfers);
            //connection = algorithm.GetConnection(profile.ConnectionId);
            //Assert.AreEqual(0, connection.DepartureStop);
            //Assert.AreEqual(0, connection.TripId);
            //profiles = algorithm.GetStopProfiles(0);
            //profile = profiles.GetBest();
            //Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.ConnectionId);
            //Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
            //Assert.AreEqual(0, profile.Transfers);
        }

        ///// <summary>
        ///// Tests a successful two-hop, one transfer with a three-connection db where one pseudo connection is skipped.
        ///// </summary>
        //[Test]
        //public void TestTwoHopsOneTransferCloseStopsSuccessfulSkippedPseudo()
        //{
        //    // build dummy db.
        //    var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsCloseStops(
        //        new TimeOfDay()
        //        {
        //            Hours = 8
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 10
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 15
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 25
        //        }));

        //    // run algorithm.
        //    var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
        //    var algorithm = new ProfileSearch(connectionsDb, 0, 3, departureTime);
        //    algorithm.Run();

        //    // test results.
        //    Assert.IsTrue(algorithm.HasRun);
        //    Assert.IsTrue(algorithm.HasSucceeded);
        //    Assert.AreEqual(55 * 60, algorithm.Duration());
        //    Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

        //    var profiles = algorithm.GetStopProfiles(3);
        //    var profile = profiles.GetBest();
        //    Assert.AreEqual(2, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
        //    Assert.AreEqual(1, profile.Transfers);
        //    var connection = algorithm.GetConnection(profile.ConnectionId); // connection 2->3.
        //    Assert.AreEqual(2, connection.DepartureStop);
        //    Assert.AreEqual(1, connection.TripId);
        //    profiles = algorithm.GetStopProfiles(2);
        //    profile = profiles.GetBest();
        //    Assert.AreEqual(1, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, profile.Seconds);
        //    Assert.AreEqual(1, profile.Transfers);
        //    connection = algorithm.GetConnection(profile.ConnectionId); // connection 1->2.
        //    Assert.AreEqual(1, connection.DepartureStop);
        //    Assert.AreEqual(OsmSharp.Routing.Transit.Constants.PseudoConnectionTripId, connection.TripId);
        //    profiles = algorithm.GetStopProfiles(1);
        //    profile = profiles.GetBest();
        //    Assert.AreEqual(0, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
        //    Assert.AreEqual(0, profile.Transfers);
        //    connection = algorithm.GetConnection(profile.ConnectionId);
        //    Assert.AreEqual(0, connection.DepartureStop);
        //    Assert.AreEqual(0, connection.TripId);
        //    profiles = algorithm.GetStopProfiles(0);
        //    profile = profiles.GetBest();
        //    Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
        //    Assert.AreEqual(0, profile.Transfers);
        //}

        ///// <summary>
        ///// Tests a successful two-hop, one transfer with a four-connection db where one pseudo connection is skipped.
        ///// </summary>
        //[Test]
        //public void TestTwoHopsOneTransferVSOneTripSuccessfulSkippedPseudo()
        //{
        //    // build dummy db.
        //    var connectionsDb = new GTFSConnectionsDb(Data.GTFS.GTFSConnectionsDbBuilder.ThreeConnectionsThreeTripsTransferVSNoTranferWithStop(
        //        new TimeOfDay()
        //        {
        //            Hours = 8
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 10
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 15
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 25
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 15
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 20
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 20
        //        },
        //        new TimeOfDay()
        //        {
        //            Hours = 8,
        //            Minutes = 25
        //        }));

        //    // run algorithm.
        //    var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
        //    var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
        //    algorithm.Run();

        //    // test results.
        //    Assert.IsTrue(algorithm.HasRun);
        //    Assert.IsTrue(algorithm.HasSucceeded);
        //    Assert.AreEqual(55 * 60, algorithm.Duration());
        //    Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

        //    var profiles = algorithm.GetStopProfiles(2);
        //    var profile = profiles.GetBest();
        //    Assert.AreEqual(3, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
        //    Assert.AreEqual(0, profile.Transfers);
        //    var connection = algorithm.GetConnection(profile.ConnectionId); // connection 1->2.
        //    Assert.AreEqual(1, connection.DepartureStop);
        //    Assert.AreEqual(2, connection.TripId);
        //    profiles = algorithm.GetStopProfiles(1);
        //    profile = profiles.GetBest(algorithm, profile);
        //    Assert.AreEqual(2, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 50 * 60, profile.Seconds);
        //    Assert.AreEqual(0, profile.Transfers);
        //    connection = algorithm.GetConnection(profile.ConnectionId);
        //    Assert.AreEqual(0, connection.DepartureStop);
        //    Assert.AreEqual(2, connection.TripId);
        //    profiles = algorithm.GetStopProfiles(0);
        //    profile = profiles.GetBest(algorithm, profile);
        //    Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.ConnectionId);
        //    Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
        //    Assert.AreEqual(0, profile.Transfers);
        //}
    }
}