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
        /// 
        /// Departure (0)@00:50:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @01:00          @01:40
        /// 
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddConnection(0, 1, 0, 3600, 3600 + 40 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 00, 50, 00);
            var algorithm = new ProfileSearch(db, 0, 1, departureTime,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(50 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 01, 40, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);
            var profiles = algorithm.GetStopProfiles(1);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 50 * 60, profile.Seconds);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.TripId);
            Assert.AreEqual(0, connections.DepartureStop);
            profiles = algorithm.GetStopProfiles(0);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
        }

        /// <summary>
        /// Tests an unsuccessful one-hop with a one-connection db.
        /// 
        /// Departure (0)@08:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10
        /// 
        /// </summary>
        [Test]
        public void TestOneHopUnsuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var algorithm = new ProfileSearch(db, 0, 1, new DateTime(2017, 05, 10, 08, 30, 00),
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsFalse(algorithm.HasSucceeded);
        }

        /// <summary>
        /// Tests a successful two-hop with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)-->---0--->--(2)
        /// @08:00          @08:10          @08:20
        /// 
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddStop(2, 2, 2);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 11 * 60, 8 * 3600 + 20 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, 0, 2, departureTime,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(50 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 20, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

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
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 50 * 60, profile.Seconds);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.TripId);
            Assert.AreEqual(1, connections.DepartureStop);

            // get preceding profile and check if stop 1 profile.
            profile = algorithm.GetPreceding(profiles, 2, out precedingStop, out transfers);
            Assert.AreEqual(2, transfers);
            Assert.AreEqual(1, precedingStop);
            profiles = algorithm.GetStopProfiles(precedingStop);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            Assert.AreEqual(0, profile.PreviousConnectionId);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.DepartureStop);
            Assert.AreEqual(0, connections.TripId);

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
        /// <summary>
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10      
        /// 
        ///                   (1)-->---1--->--(2)
        ///                 @08:15          @08:25      
        /// </summary>
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddStop(2, 2, 2);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, 0, 2, departureTime,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

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
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[3];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[4];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(1, connections.TripId);
            Assert.AreEqual(1, connections.DepartureStop);

            // get preceding profile and check if stop 1 profile.
            profile = algorithm.GetPreceding(profiles, 4, out precedingStop, out transfers);
            Assert.AreEqual(2, transfers);
            Assert.AreEqual(1, precedingStop);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            Assert.AreEqual(0, profile.PreviousConnectionId);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.DepartureStop);
            Assert.AreEqual(0, connections.TripId);
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
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10      
        /// 
        ///                   (1)-->---1--->--(2)
        ///                 @08:15          @08:25   
        ///   (0)------2------->-------2------(2)
        /// @08:15                          @08:25
        /// 
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferVersusOneHopSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddStop(2, 2, 2);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);
            db.AddConnection(0, 2, 2, 8 * 3600 + 16 * 60, 8 * 3600 + 25 * 60);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, 0, 2, departureTime,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

            var precedingStop = OsmSharp.Routing.Transit.Constants.NoStopId;
            var transfers = OsmSharp.Routing.Transit.Constants.NoTransfers;

            // get profiles at stop 2.
            var profiles = algorithm.GetStopProfiles(2);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(2, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.DepartureStop);
            Assert.AreEqual(2, connections.TripId);

            // get previous profile and check this is stop 0.
            profile = algorithm.GetPreceding(profiles, 2, out precedingStop, out transfers);
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
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);
            connections.MoveTo(profile.PreviousConnectionId);
            Assert.AreEqual(0, connections.DepartureStop);
            Assert.AreEqual(0, connections.TripId);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10      
        ///                     \ (-> transfer: 100 sec)
        ///                     (2)-->---1--->--(3)
        ///                   @08:15          @08:25  
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferCloseStopsSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddStop(2, 2, 2);
            db.AddStop(3, 3, 3);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(2, 3, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            // build dummy transfers db.
            var transfersDb = new TransfersDb(1024);
            transfersDb.AddTransfer(1, 2, 100);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, 0, 3, departureTime, transfersDb,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

            var profiles = algorithm.GetStopProfiles(3);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[3];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[4];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(2);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[3];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60 + 100, profile.Seconds);

            profiles = algorithm.GetStopProfiles(1);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(0);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a three-connection db where one transfer connection is skipped.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)-->---0--->--(2)-->---0--->--(3)
        /// @08:00          @08:10          @08:15          @08:25  
        ///                     \             /   
        ///                      ------------
        ///       (transfer time smaller/bigger than 5 mins total)
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferCloseStopsSuccessfulSkippedPseudo()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, 0);
            db.AddStop(1, 1, 1);
            db.AddStop(2, 2, 2);
            db.AddStop(3, 3, 3);
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 10 * 60, 8 * 3600 + 15 * 60);
            db.AddConnection(2, 3, 0, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            // build dummy transfers db.
            var transfersDb = new TransfersDb(1024);
            transfersDb.AddTransfer(1, 2, 60); // this leads to a transfer time faster than the actual connection.

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, 0, 3, departureTime, transfersDb,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            var connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

            var profiles = algorithm.GetStopProfiles(3);
            Assert.AreEqual(3, profiles.Count);
            var profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(2, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(2);
            Assert.AreEqual(4, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, profile.Seconds);
            profile = profiles[3];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60 + 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(1);
            Assert.AreEqual(3, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(0);
            Assert.AreEqual(1, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);

            // build dummy transfers db.
            transfersDb = new TransfersDb(1024);
            transfersDb.AddTransfer(1, 2, 6 * 60); // this leads to a transfer time slower than the actual connection.

            // run algorithm.
            departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            algorithm = new ProfileSearch(db, 0, 3, departureTime, transfersDb,
                (profileId, day) => true);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(55 * 60, algorithm.Duration());
            Assert.AreEqual(new DateTime(2017, 05, 10, 08, 25, 00), algorithm.ArrivalTime());

            connections = db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);

            profiles = algorithm.GetStopProfiles(3);
            Assert.AreEqual(3, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(2, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 55 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(2);
            Assert.AreEqual(3, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(1, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 45 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(1);
            Assert.AreEqual(3, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[1];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoSeconds, profile.Seconds);
            profile = profiles[2];
            Assert.AreEqual(0, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds + 40 * 60, profile.Seconds);

            profiles = algorithm.GetStopProfiles(0);
            Assert.AreEqual(1, profiles.Count);
            profile = profiles[0];
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.NoConnectionId, profile.PreviousConnectionId);
            Assert.AreEqual((int)(departureTime - departureTime.Date).TotalSeconds, profile.Seconds);
        }
    }
}