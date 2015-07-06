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

using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using NUnit.Framework;
using OsmSharp.Routing;
using OsmSharp.Routing.Transit.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Data;
using System;

namespace OsmSharp.Transit.Test.Algorithms.OneToMany
{
    /// <summary>
    /// Contains tests for the route builder of the profile search algorithm.
    /// </summary>
    /// <remarks>It's only possible to build a complete route when there is a GTFS-feed available. A bare connections db is only enough to calculate paths.</remarks>
    [TestFixture]
    public class ProfileSearchRouteBuilderTests
    {
        /// <summary>
        /// Tests a successful one-hop with a one-connection db.
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build GTFS data.
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.Stops.Add(new global::GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "STOP_0",
                Description = "Stop 0",
                Latitude = 0,
                Longitude = 1,
                Name = "The one of two stops in this feed.",
                LocationType = LocationType.Stop
            });
            feed.Stops.Add(new global::GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "STOP_1",
                Description = "Stop 1",
                Latitude = 1,
                Longitude = 0,
                Name = "The one of two stops in this feed.",
                LocationType = LocationType.Stop
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                AgencyId = "0",
                Description = "The one and only route in this feed.",
                Id = "0",
                LongName = "The one and only route in this feed.",
                ShortName = "Route 0",
                Type = RouteType.Bus
            });
            feed.Trips.Add(new Trip()
            {
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 0",
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0",
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                StopHeadsign = "Stop 0",
                StopId = "0",
                StopSequence = 0,
                TripId = "0"
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                StopHeadsign = "Stop 1",
                StopId = "1",
                StopSequence = 1,
                TripId = "0"
            });

            // build connections db from GTFS-data.
            var connectionsDb = new GTFSConnectionsDb(feed);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(connectionsDb, 0, 1, departureTime);
            algorithm.Run();

            // check results first.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            // run the route builder.
            var routeBuilder = new ProfileSearchRouteBuilder(
                algorithm, connectionsDb);
            var route = routeBuilder.Build();

            // check route.
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            var segment0 = route.Segments[0];
            Assert.AreEqual(0, segment0.Time);
            Assert.AreEqual(RouteSegmentType.Start, segment0.Type);
            Assert.AreEqual(feed.Stops.Get(0).Name, segment0.Name);
            var segment1 = route.Segments[1];
            Assert.AreEqual(30 * 60, segment1.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment1.Type);
            Assert.AreEqual(feed.Stops.Get(0).Name, segment1.Name);
            var segment2 = route.Segments[2];
            Assert.AreEqual(40 * 60, segment2.Time);
            Assert.AreEqual(RouteSegmentType.Stop, segment2.Type);
            Assert.AreEqual(feed.Stops.Get(1).Name, segment2.Name);
        }

        /// <summary>
        /// Tests a successful two-hop with a two-connection db.
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessful()
        {
            // build GTFS data.
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.Stops.Add(new global::GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "STOP_0",
                Description = "Stop 0",
                Latitude = 0,
                Longitude = 1,
                Name = "The first stop in this feed.",
                LocationType = LocationType.Stop
            });
            feed.Stops.Add(new global::GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "STOP_1",
                Description = "Stop 1",
                Latitude = 1,
                Longitude = 0,
                Name = "The second stop in this feed.",
                LocationType = LocationType.Stop
            });
            feed.Stops.Add(new global::GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "STOP_2",
                Description = "Stop 2",
                Latitude = 2,
                Longitude = 0,
                Name = "The third step in this feed.",
                LocationType = LocationType.Stop
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                AgencyId = "0",
                Description = "The one and only route in this feed.",
                Id = "0",
                LongName = "The one and only route in this feed.",
                ShortName = "Route 0",
                Type = RouteType.Bus
            });
            feed.Trips.Add(new Trip()
            {
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 0",
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0",
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 0,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                StopHeadsign = "Stop 0",
                StopId = "0",
                StopSequence = 0,
                TripId = "0"
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 10,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                StopHeadsign = "Stop 1",
                StopId = "1",
                StopSequence = 1,
                TripId = "0"
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 20,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 20,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                StopHeadsign = "Stop 2",
                StopId = "2",
                StopSequence = 2,
                TripId = "0"
            });

            // build connections db from GTFS-data.
            var connectionsDb = new GTFSConnectionsDb(feed);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(connectionsDb, 0, 2, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            // run the route builder.
            var routeBuilder = new ProfileSearchRouteBuilder(
                algorithm, connectionsDb);
            var route = routeBuilder.Build();

            // check route.
            Assert.IsNotNull(route);
            Assert.AreEqual(4, route.Segments.Length);
            var segment0 = route.Segments[0];
            Assert.AreEqual(0, segment0.Time);
            Assert.AreEqual(RouteSegmentType.Start, segment0.Type);
            Assert.AreEqual(feed.Stops.Get(0).Name, segment0.Name);
            var segment1 = route.Segments[1];
            Assert.AreEqual(30 * 60, segment1.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment1.Type);
            Assert.AreEqual(feed.Stops.Get(0).Name, segment1.Name);
            var segment2 = route.Segments[2];
            Assert.AreEqual(40 * 60, segment2.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment2.Type);
            Assert.AreEqual(feed.Stops.Get(1).Name, segment2.Name);
            var segment3 = route.Segments[3];
            Assert.AreEqual(50 * 60, segment3.Time);
            Assert.AreEqual(RouteSegmentType.Stop, segment3.Type);
            Assert.AreEqual(feed.Stops.Get(2).Name, segment3.Name);
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
            var algorithm = new ProfileSearch(connectionsDb, 0, 3, departureTime);
            algorithm.Run();

            // test results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            // run the route builder.
            var routeBuilder = new ProfileSearchRouteBuilder(
                algorithm, connectionsDb);
            var route = routeBuilder.Build();

            // check route.
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Segments.Length);
            var segment0 = route.Segments[0];
            Assert.AreEqual(0, segment0.Time);
            Assert.AreEqual(RouteSegmentType.Start, segment0.Type);
            Assert.AreEqual(connectionsDb.Feed.Stops.Get(0).Name, segment0.Name);
            Assert.IsNull(segment0.Vehicle);
            var segment1 = route.Segments[1];
            Assert.AreEqual(30 * 60, segment1.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment1.Type);
            Assert.AreEqual(connectionsDb.Feed.Stops.Get(0).Name, segment1.Name);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitVehicle, segment1.Vehicle);
            var segment2 = route.Segments[2];
            Assert.AreEqual(40 * 60, segment2.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment2.Type);
            Assert.AreEqual(connectionsDb.Feed.Stops.Get(1).Name, segment2.Name);
            Assert.AreEqual(RouteType.Tram.ToVehicleUniqueName(), segment2.Vehicle);
            var segment3 = route.Segments[3];
            Assert.AreEqual(45 * 60, segment3.Time);
            Assert.AreEqual(RouteSegmentType.Along, segment3.Type);
            Assert.AreEqual(connectionsDb.Feed.Stops.Get(1).Name, segment3.Name);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferVehicle, segment3.Vehicle);
            var segment4 = route.Segments[4];
            Assert.AreEqual(55 * 60, segment4.Time);
            Assert.AreEqual(RouteSegmentType.Stop, segment4.Type);
            Assert.AreEqual(connectionsDb.Feed.Stops.Get(2).Name, segment4.Name);
            Assert.AreEqual(RouteType.Tram.ToVehicleUniqueName(), segment4.Vehicle);
        }
    }
}