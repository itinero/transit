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
using OsmSharp.Routing.Transit.Data.GTFS;
using System.Collections.Generic;

namespace OsmSharp.Transit.Test.Data.GTFS
{
    /// <summary>
    /// A test fixture for the GTFS-based connections database.
    /// </summary>
    [TestFixture]
    public class GTFSConnectionsDbTests
    {
        /// <summary>
        /// Tests the db with only one stop.
        /// </summary>
        [Test]
        public void TestOneStop()
        {
            var feed = new GTFSFeed();
            feed.AddStop(new global::GTFS.Entities.Stop()
                {
                    Id = "0",
                    Code = "STOP_0",
                    Description = "Stop 0",
                    Latitude  = 0,
                    Longitude = 1,
                    Name = "The one and only stop in this feed.",
                    ParentStation = string.Empty,
                    Tag = null,
                    Timezone = null,
                    Url = null,
                    WheelchairBoarding = null,
                    Zone = null,
                    LocationType = LocationType.Stop
                });

            var db = new GTFSConnectionsDb(feed);
            var stops = db.GetStops();
            Assert.IsNotNull(stops);
            Assert.AreEqual(1, stops.Count);
            Assert.AreEqual(0, stops[0].Latitude);
            Assert.AreEqual(1, stops[0].Longitude);
        }

        /// <summary>
        /// Tests the db with only one connection.
        /// </summary>
        [Test]
        public void TestOneConnection()
        {
            var feed = new GTFSFeed();
            feed.AddStop(new Stop()
            {
                Id = "0",
                Code = "STOP_0",
                Description = "Stop 0",
                Latitude = 0,
                Longitude = 1,
                Name = "The one of two stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddStop(new Stop()
            {
                Id = "1",
                Code = "STOP_1",
                Description = "Stop 1",
                Latitude = 1,
                Longitude = 0,
                Name = "The one of two stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddRoute(new Route()
            {
                AgencyId = "0",
                Color = null,
                Description = "The one and only route in this feed.",
                Id = "0",
                LongName = "The one and only route in this feed.",
                ShortName = "Route 0",
                Tag = null,
                TextColor = null,
                Type = RouteType.Bus,
                Url = null
            });
            feed.AddTrip(new Trip()
            {
                AccessibilityType = null,
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 0",
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShapeId = null,
                ShortName = "Trip 0",
                Tag = null
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 1,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 0",
                StopId = "0",
                StopSequence = 0,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 11,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 1",
                StopId = "1",
                StopSequence = 1,
                Tag = null,
                TripId = "0"
            });

            var stopTimesInFeed = new List<StopTime>(feed.GetStopTimes());

            var db = new GTFSConnectionsDb(feed);
            var stops = db.GetStops();
            Assert.IsNotNull(stops);
            Assert.AreEqual(2, stops.Count);
            Assert.AreEqual(0, stops[0].Latitude);
            Assert.AreEqual(1, stops[0].Longitude);
            Assert.AreEqual(1, stops[1].Latitude);
            Assert.AreEqual(0, stops[1].Longitude);

            var departureTimeView = db.GetDepartureTimeView();
            Assert.IsNotNull(departureTimeView);
            Assert.AreEqual(1, departureTimeView.Count);
            Assert.AreEqual(0, departureTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, departureTimeView[0].DepartureTime);
            Assert.AreEqual(1, departureTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, departureTimeView[0].ArrivalTime);
            Assert.AreEqual(0, departureTimeView[0].TripId);

            var arrivalTimeView = db.GetArrivalTimeView();
            Assert.IsNotNull(arrivalTimeView);
            Assert.AreEqual(1, arrivalTimeView.Count);
            Assert.AreEqual(0, arrivalTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, arrivalTimeView[0].DepartureTime);
            Assert.AreEqual(1, arrivalTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, arrivalTimeView[0].ArrivalTime);
            Assert.AreEqual(0, arrivalTimeView[0].TripId);
        }

        /// <summary>
        /// Tests the db with two connections.
        /// </summary>
        [Test]
        public void TestTwoConnections()
        {
            var feed = new GTFSFeed();
            feed.AddStop(new Stop()
            {
                Id = "0",
                Code = "STOP_0",
                Description = "Stop 0",
                Latitude = 0,
                Longitude = 1,
                Name = "The one of three stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddStop(new Stop()
            {
                Id = "1",
                Code = "STOP_1",
                Description = "Stop 1",
                Latitude = 1,
                Longitude = 0,
                Name = "The one of three stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddStop(new Stop()
            {
                Id = "2",
                Code = "STOP_2",
                Description = "Stop 2",
                Latitude = 2,
                Longitude = 0,
                Name = "The one of two three in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddRoute(new Route()
            {
                AgencyId = "0",
                Color = null,
                Description = "The one and only route in this feed.",
                Id = "0",
                LongName = "The one and only route in this feed.",
                ShortName = "Route 0",
                Tag = null,
                TextColor = null,
                Type = RouteType.Bus,
                Url = null
            });
            feed.AddTrip(new Trip()
            {
                AccessibilityType = null,
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 0",
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShapeId = null,
                ShortName = "Trip 0",
                Tag = null
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 1,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 0",
                StopId = "0",
                StopSequence = 0,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 11,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 1",
                StopId = "1",
                StopSequence = 1,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 21,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 2",
                StopId = "2",
                StopSequence = 2,
                Tag = null,
                TripId = "0"
            });

            var stopTimesInFeed = new List<StopTime>(feed.GetStopTimes());

            var db = new GTFSConnectionsDb(feed);
            var stops = db.GetStops();
            Assert.IsNotNull(stops);
            Assert.AreEqual(3, stops.Count);
            Assert.AreEqual(0, stops[0].Latitude);
            Assert.AreEqual(1, stops[0].Longitude);
            Assert.AreEqual(1, stops[1].Latitude);
            Assert.AreEqual(0, stops[1].Longitude);
            Assert.AreEqual(2, stops[2].Latitude);
            Assert.AreEqual(0, stops[2].Longitude);

            var departureTimeView = db.GetDepartureTimeView();
            Assert.IsNotNull(departureTimeView);
            Assert.AreEqual(2, departureTimeView.Count);
            Assert.AreEqual(0, departureTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, departureTimeView[0].DepartureTime);
            Assert.AreEqual(1, departureTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, departureTimeView[0].ArrivalTime);
            Assert.AreEqual(0, departureTimeView[0].TripId);
            Assert.AreEqual(1, departureTimeView[1].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[1].DepartureTime.TotalSeconds, departureTimeView[1].DepartureTime);
            Assert.AreEqual(2, departureTimeView[1].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[2].ArrivalTime.TotalSeconds, departureTimeView[1].ArrivalTime);
            Assert.AreEqual(0, departureTimeView[1].TripId);

            var arrivalTimeView = db.GetArrivalTimeView();
            Assert.IsNotNull(arrivalTimeView);
            Assert.AreEqual(2, arrivalTimeView.Count);
            Assert.AreEqual(0, arrivalTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, arrivalTimeView[0].DepartureTime);
            Assert.AreEqual(1, arrivalTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, arrivalTimeView[0].ArrivalTime);
            Assert.AreEqual(0, arrivalTimeView[0].TripId);
            Assert.AreEqual(1, arrivalTimeView[1].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[1].DepartureTime.TotalSeconds, arrivalTimeView[1].DepartureTime);
            Assert.AreEqual(2, arrivalTimeView[1].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[2].ArrivalTime.TotalSeconds, arrivalTimeView[1].ArrivalTime);
            Assert.AreEqual(0, arrivalTimeView[1].TripId);
        }

        /// <summary>
        /// Tests the db with two trips and including arrival- and departuretimes that should be sorted differently.
        /// </summary>
        [Test]
        public void TestTwoTrips()
        {
            var feed = new GTFSFeed();
            feed.AddStop(new Stop()
            {
                Id = "0",
                Code = "STOP_0",
                Description = "Stop 0",
                Latitude = 0,
                Longitude = 1,
                Name = "The one of three stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddStop(new Stop()
            {
                Id = "1",
                Code = "STOP_1",
                Description = "Stop 1",
                Latitude = 1,
                Longitude = 0,
                Name = "The one of three stops in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddStop(new Stop()
            {
                Id = "2",
                Code = "STOP_2",
                Description = "Stop 2",
                Latitude = 2,
                Longitude = 0,
                Name = "The one of two three in this feed.",
                ParentStation = string.Empty,
                Tag = null,
                Timezone = null,
                Url = null,
                WheelchairBoarding = null,
                Zone = null,
                LocationType = LocationType.Stop
            });
            feed.AddRoute(new Route()
            {
                AgencyId = "0",
                Color = null,
                Description = string.Empty,
                Id = "0",
                LongName = string.Empty,
                ShortName = "Route 0",
                Tag = null,
                TextColor = null,
                Type = RouteType.Bus,
                Url = null
            });
            feed.AddRoute(new Route()
            {
                AgencyId = "0",
                Color = null,
                Description = string.Empty,
                Id = "1",
                LongName = string.Empty,
                ShortName = "Route 1",
                Tag = null,
                TextColor = null,
                Type = RouteType.Bus,
                Url = null
            });
            feed.AddTrip(new Trip()
            {
                AccessibilityType = null,
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 0",
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShapeId = null,
                ShortName = "Trip 0",
                Tag = null
            });
            feed.AddTrip(new Trip()
            {
                AccessibilityType = null,
                BlockId = "0",
                Direction = 0,
                Headsign = "Trip 1",
                Id = "1",
                RouteId = "1",
                ServiceId = "0",
                ShapeId = null,
                ShortName = "Trip 1",
                Tag = null
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 1,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 0",
                StopId = "0",
                StopSequence = 0,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 11,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 1",
                StopId = "1",
                StopSequence = 1,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
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
                    Minutes = 21,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = "Stop 2",
                StopId = "2",
                StopSequence = 2,
                Tag = null,
                TripId = "0"
            });
            feed.AddStopTime(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 9,
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
                ShapeDistTravelled = string.Empty,
                StopHeadsign = string.Empty,
                StopId = "1",
                StopSequence = 0,
                Tag = null,
                TripId = "1"
            });
            feed.AddStopTime(new StopTime()
            {
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 11,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 8,
                    Minutes = 12,
                    Seconds = 0
                },
                DropOffType = DropOffType.Regular,
                PickupType = PickupType.Regular,
                ShapeDistTravelled = string.Empty,
                StopHeadsign = string.Empty,
                StopId = "2",
                StopSequence = 1,
                Tag = null,
                TripId = "1"
            });

            var stopTimesInFeed = new List<StopTime>(feed.GetStopTimes());

            var db = new GTFSConnectionsDb(feed);
            var stops = db.GetStops();
            Assert.IsNotNull(stops);
            Assert.AreEqual(3, stops.Count);
            Assert.AreEqual(0, stops[0].Latitude);
            Assert.AreEqual(1, stops[0].Longitude);
            Assert.AreEqual(1, stops[1].Latitude);
            Assert.AreEqual(0, stops[1].Longitude);
            Assert.AreEqual(2, stops[2].Latitude);
            Assert.AreEqual(0, stops[2].Longitude);

            var departureTimeView = db.GetDepartureTimeView();
            Assert.IsNotNull(departureTimeView);
            Assert.AreEqual(3, departureTimeView.Count);
            Assert.AreEqual(0, departureTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, departureTimeView[0].DepartureTime);
            Assert.AreEqual(1, departureTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, departureTimeView[0].ArrivalTime);
            Assert.AreEqual(0, departureTimeView[0].TripId);
            Assert.AreEqual(1, departureTimeView[1].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[3].DepartureTime.TotalSeconds, departureTimeView[1].DepartureTime);
            Assert.AreEqual(2, departureTimeView[1].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[4].ArrivalTime.TotalSeconds, departureTimeView[1].ArrivalTime);
            Assert.AreEqual(1, departureTimeView[1].TripId);
            Assert.AreEqual(1, departureTimeView[2].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[1].DepartureTime.TotalSeconds, departureTimeView[2].DepartureTime);
            Assert.AreEqual(2, departureTimeView[2].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[2].ArrivalTime.TotalSeconds, departureTimeView[2].ArrivalTime);
            Assert.AreEqual(0, departureTimeView[2].TripId);

            var arrivalTimeView = db.GetArrivalTimeView();
            Assert.IsNotNull(arrivalTimeView);
            Assert.AreEqual(3, arrivalTimeView.Count);
            Assert.AreEqual(1, arrivalTimeView[0].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[1].DepartureTime.TotalSeconds, arrivalTimeView[0].DepartureTime);
            Assert.AreEqual(2, arrivalTimeView[0].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[2].ArrivalTime.TotalSeconds, arrivalTimeView[0].ArrivalTime);
            Assert.AreEqual(0, arrivalTimeView[0].TripId);
            Assert.AreEqual(1, arrivalTimeView[1].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[3].DepartureTime.TotalSeconds, arrivalTimeView[1].DepartureTime);
            Assert.AreEqual(2, arrivalTimeView[1].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[4].ArrivalTime.TotalSeconds, arrivalTimeView[1].ArrivalTime);
            Assert.AreEqual(1, arrivalTimeView[1].TripId);
            Assert.AreEqual(0, arrivalTimeView[2].DepartureStop);
            Assert.AreEqual(stopTimesInFeed[0].DepartureTime.TotalSeconds, arrivalTimeView[2].DepartureTime);
            Assert.AreEqual(1, arrivalTimeView[2].ArrivalStop);
            Assert.AreEqual(stopTimesInFeed[1].ArrivalTime.TotalSeconds, arrivalTimeView[2].ArrivalTime);
            Assert.AreEqual(0, arrivalTimeView[2].TripId);
        }
    }
}