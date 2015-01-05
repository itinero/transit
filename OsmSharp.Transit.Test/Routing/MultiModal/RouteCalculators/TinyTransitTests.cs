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
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Transit.MultiModal;
using System;
using OsmSharp.Routing.Transit.MultiModal.GTFS;

namespace OsmSharp.Transit.Test.Routing.MultiModal.RouteCalculators
{
    /// <summary>
    /// Contains transit routing tests on tiny networks as a reference.
    /// </summary>
    [TestFixture]
    public class TinyTransitTests
    {
        /// <summary>
        /// The tolerance to compare floats/doubles.
        /// </summary>
        private const float E = 0.0001f;

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        [Test]
        public void Test1NoTransit()
        {
            var departureTime = new DateTime(2015, 01, 01);
            var vehicle = Vehicle.Pedestrian;
            var coordinate1 = new GeoCoordinate(51.26390206241818, 4.778001308441162);
            var coordinate2 = new GeoCoordinate(51.26402290345785, 4.792361855506897);

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var router = MultiModalRouter.CreateFrom(graph);
            var resolved1 = router.Resolve(vehicle, coordinate1);
            var resolved2 = router.Resolve(vehicle, coordinate2);

            var route = router.CalculateTransit(departureTime, vehicle, vehicle, vehicle, resolved1, resolved2);

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(2, route.Segments.Length);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>The network has 4 vertices: 0 --- 1km --- 1 --------- 10km --------- 2 --- 1km --- 3.
        ///          Between vertex 1 and 2 there is transit link with one trip that takes 10 mins:
        ///                                      @1 01/01/2015 10:00 -> @2 01/01/2015 10:10
        ///          What is tested: - a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
        ///                          - a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
        /// </remarks>
        [Test]
        public void Test1Transit1OneTrip()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.26390206241818, 4.778001308441162);
            var coordinate1 = new GeoCoordinate(51.26402290345785, 4.792361855506897);
            var coordinate2 = new GeoCoordinate(51.265137311403734, 4.936380386352539);
            var coordinate3 = new GeoCoordinate(51.265405839398, 4.950714111328125);

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });

            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved3 = router.Resolve(vehicle, coordinate3);

            // a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(6, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[4].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[5].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 01, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved3, resolved0);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[3].Longitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>The network has 5 vertices: 0 --- 1km --- 1 ---- 10km ---- 2 --- 1km --- 3.
        ///                                           (STOP1)        (STOP2)        (STOP3)
        ///          Between vertex 1 and 2 there is transit connection with one intermediate stop with one trips that take 20 mins:
        ///                                      @STOP1 01/01/2015 10:00 -> @STOP2 01/01/2015 10:09
        ///                                      @STOP2 01/01/2015 10:10 -> @STOP3 01/01/2015 10:20
        ///          What is tested: - a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit links.
        ///                          - a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit links.
        /// </remarks>
        [Test]
        public void Test1Transit2OneTrip()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.26390206241818, 4.778001308441162);
            var coordinate1 = new GeoCoordinate(51.26402290345785, 4.792361855506897);
            var coordinate2 = new GeoCoordinate(51.265137311403734, 4.936380386352539);
            var coordinate3 = new GeoCoordinate(51.265405839398, 4.950714111328125);
            var coordinate4 = new GeoCoordinate(51.264694236781665, 4.864389896392822); // #4: in the middle between 1 and 2

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate4.Latitude,
                Longitude = coordinate4.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "Code 2",
                Name = "Name 2",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 09,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                StopSequence = 3
            });

            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved3 = router.Resolve(vehicle, coordinate3);

            // a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(7, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate4.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[4].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[5].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[6].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[6].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 01, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved3, resolved0);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[3].Longitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>The network has 5 vertices: 0 --- 1km --- 1 ---- 10km ---- 2 --- 1km --- 3.
        ///                                                 (STOP1)          (STOP2)       (STOP3)
        ///          Between vertex 1 and 2 there is transit connection with one intermediate stop with two trips that take 30 mins in total:
        ///                                      TRIP1 @STOP1 01/01/2015 10:00 -> @STOP2 01/01/2015 10:10
        ///                                      TRIP2 @STOP2 01/01/2015 10:20 -> @STOP3 01/01/2015 10:30
        ///                                      (this means a tranfer is needed at STOP2)
        ///          What is tested: - a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit links.
        ///                          - a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit links.
        /// </remarks>
        [Test]
        public void Test1Transit3TwoTrips()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.26390206241818, 4.778001308441162);
            var coordinate1 = new GeoCoordinate(51.26402290345785, 4.792361855506897);
            var coordinate2 = new GeoCoordinate(51.265137311403734, 4.936380386352539);
            var coordinate3 = new GeoCoordinate(51.265405839398, 4.950714111328125);
            var coordinate4 = new GeoCoordinate(51.264694236781665, 4.864389896392822); // #4: in the middle between 1 and 2

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate4.Latitude,
                Longitude = coordinate4.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "Code 2",
                Name = "Name 2",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "1",
                Id = "1",
                ShortName = "Route 1"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "1",
                RouteId = "1",
                ServiceId = "0",
                ShortName = "Trip 1"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                StopSequence = 2
            });

            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved3 = router.Resolve(vehicle, coordinate3);

            // a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(7, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate4.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[4].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[5].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[6].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[6].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 01, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved3, resolved0);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[3].Longitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>The network has 5 vertices: 0 --- 1km --- 1 ---- 10km ---- 2 --- 1km --- 3.
        ///                                                 (STOP1)          (STOP2)       (STOP3)
        ///          Between vertex 1 and 2 there is transit connection with one intermediate stop with three trips that take 10 mins each:
        ///                                      TRIP1 @STOP1 01/01/2015 10:00 -> @STOP2 01/01/2015 10:10
        ///                                      TRIP2 @STOP2 01/01/2015 10:20 -> @STOP3 01/01/2015 10:30
        ///                                      TRIP3 @STOP2 01/01/2015 10:11 -> @STOP3 01/01/2015 10:21
        ///          This means a tranfer is needed at STOP2 and TRIP1 -> TRIP3 is not possible because there is only 1 minute transfer time.
        ///          What is tested: - a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit links.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit links.
        ///                          - a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit links.
        /// </remarks>
        [Test]
        public void Test1Transit4ThreeTrips()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.26390206241818, 4.778001308441162);
            var coordinate1 = new GeoCoordinate(51.26402290345785, 4.792361855506897);
            var coordinate2 = new GeoCoordinate(51.265137311403734, 4.936380386352539);
            var coordinate3 = new GeoCoordinate(51.265405839398, 4.950714111328125);
            var coordinate4 = new GeoCoordinate(51.264694236781665, 4.864389896392822); // #4: in the middle between 1 and 2

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate4.Latitude,
                Longitude = coordinate4.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "Code 2",
                Name = "Name 2",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "1",
                Id = "1",
                ShortName = "Route 1"
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "2",
                Id = "2",
                ShortName = "Route 2"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "1",
                RouteId = "1",
                ServiceId = "0",
                ShortName = "Trip 1"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "2",
                RouteId = "2",
                ServiceId = "0",
                ShortName = "Trip 2"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "2",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 11,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 11,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "2",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 21,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 21,
                    Seconds = 0
                },
                StopSequence = 2
            });


            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved3 = router.Resolve(vehicle, coordinate3);

            // a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(7, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate4.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[4].Longitude, E);
            Assert.IsTrue(route.Segments[4].Tags.ConvertToTagsCollection().ContainsKeyValue("time_seconds", "2100")); // confirm that TRIP2 was taken not TRIP3.
            Assert.AreEqual(coordinate2.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[5].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[6].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[6].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 01, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[3].Longitude, E);

            // a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 10, 00, 00), vehicle, vehicle, vehicle, resolved3, resolved0);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(4, route.Segments.Length);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[3].Longitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>                           (1)         (2)           (3)      (4)           (5)         (6) 
        ///          The network has 6 vertices: 0 ---1km--- 1 ----5km---- 2 -100m- 3 ----5km---- 4 ---1km--- 5.
        ///          The network has stops:               STOP1(7)      STOP2(8) STOP3(9)      STOP4(10)
        ///          And TRIP1 01/01/2015:                 10:00         10:10    10:20         10:30
        ///          
        ///          This tests is developed to make sure there is no part on foot between vertex 2 and 3. A naive routing algorithm
        ///          would reach vertex 3 via 0->1-(TRIP1)->2->3 instead of 0->1-(TRIP1)->2-(TRIP1)->3 because the first options is faster
        ///          but the resulting route is supposed to be 0->1-(TRIP1)->2-(TRIP1)->3-(TRIP1)->4->5 instead of 0->1-(TRIP1)->2->3-(TRIP1)->4->5
        ///          What is tested: - a trip 0 -> 5 on foot leaving 01/01/2015 09:45: should take the transit links without walking 2->3.
        ///                          - a trip 2 -> 5 on foot leaving 01/01/2015 10:00: should take the transit links without walking 2->3.
        /// </remarks>
        [Test]
        public void Test1Transit5OneTripFasterOnFoot()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.263955769586154, 4.777936935424804);
            var coordinate1 = new GeoCoordinate(51.2640631837338, 4.792613983154297);
            var coordinate2 = new GeoCoordinate(51.264694236781665, 4.864389896392822);
            var coordinate3 = new GeoCoordinate(51.26479157929962, 4.867238402366637);
            var coordinate4 = new GeoCoordinate(51.26509703206914, 4.938933849334717);
            var coordinate5 = new GeoCoordinate(51.265170877488934, 4.953128099441527);

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            var vertex4 = graph.AddVertex((float)coordinate4.Latitude, (float)coordinate4.Longitude);
            var vertex5 = graph.AddVertex((float)coordinate5.Latitude, (float)coordinate5.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 5000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 100,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex3, vertex4, new LiveEdge()
            {
                Distance = 5000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex4, vertex5, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "Code 2",
                Name = "Name 2",
                Latitude = coordinate3.Latitude,
                Longitude = coordinate3.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "3",
                Code = "Code 3",
                Name = "Name 3",
                Latitude = coordinate4.Latitude,
                Longitude = coordinate4.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 09,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 19,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                StopSequence = 3
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "3",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 30,
                    Seconds = 0
                },
                StopSequence = 4
            });

            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved5 = router.Resolve(vehicle, coordinate5);

            // a trip 0 -> 5 on foot leaving 01/01/2015 09:45: should take the transit links without walking 2->3.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved5);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(8, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[4].Longitude, E);
            Assert.IsTrue(route.Segments[4].Tags.ConvertToTagsCollection().ContainsKeyValue("trip_id", "0")); // confirm that TRIP1 was taken.
            Assert.AreEqual(coordinate4.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[5].Longitude, E);
            Assert.AreEqual(coordinate4.Latitude, route.Segments[6].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[6].Longitude, E);
            Assert.AreEqual(coordinate5.Latitude, route.Segments[7].Latitude, E);
            Assert.AreEqual(coordinate5.Longitude, route.Segments[7].Longitude, E);

            // a trip 2 -> 5 on foot leaving 01/01/2015 10:00: should take the transit links without walking 2->3.
            var resolved2 = router.Resolve(vehicle, coordinate2);
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved2, resolved5);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(6, route.Segments.Length);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[2].Longitude, E);
            Assert.IsTrue(route.Segments[2].Tags.ConvertToTagsCollection().ContainsKeyValue("trip_id", "0")); // confirm that TRIP1 was taken.
            Assert.AreEqual(coordinate4.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate4.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate4.Longitude, route.Segments[4].Longitude, E);
            Assert.AreEqual(coordinate5.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate5.Longitude, route.Segments[5].Longitude, E);
        }

        /// <summary>
        /// Tests a route in the a basic network.
        /// </summary>
        /// <remarks>
        ///                                     (1)           (2)              (3)            (4)
        ///          The network has 5 vertices: 0 --- 1km --- 1 ---- 10km ---- 2 --- 10km --- 3.
        ///          The network has stops:                STOP1(5)          STOP2(6)       STOP3(7)
        ///          And TRIP1 01/01/2015:                  10:00             10:10    
        ///          And TRIP2 01/01/2015:                  10:10          10:20->10:21      10:31
        ///          
        ///          This means when going from 0 -> 3 TRIP2 should be taken all the way including to STOP2 even though TRIP1 leaves earlier.
        ///          What is tested: - a trip 0 -> 2 on foot leaving 01/01/2015 09:45: should take TRIP1.
        ///                          - a trip 0 -> 2 on foot leaving 01/01/2015 09:45: should take TRIP2.
        /// </remarks>
        [Test]
        public void Test1Transit6TwoOverlappingTrips()
        {
            var vehicle = Vehicle.Pedestrian;
            var coordinate0 = new GeoCoordinate(51.263902062418180, 4.778001308441162);
            var coordinate1 = new GeoCoordinate(51.264022903457850, 4.792361855506897);
            var coordinate2 = new GeoCoordinate(51.265137311403734, 4.936380386352539);
            var coordinate3 = new GeoCoordinate(51.266318822846884, 5.079803466796875);

            var tags = new TagsTableCollectionIndex();
            var graph = new DynamicGraphRouterDataSource<LiveEdge>(tags);
            var vertex0 = graph.AddVertex((float)coordinate0.Latitude, (float)coordinate0.Longitude);
            var vertex1 = graph.AddVertex((float)coordinate1.Latitude, (float)coordinate1.Longitude);
            var vertex2 = graph.AddVertex((float)coordinate2.Latitude, (float)coordinate2.Longitude);
            var vertex3 = graph.AddVertex((float)coordinate3.Latitude, (float)coordinate3.Longitude);
            graph.AddEdge(vertex0, vertex1, new LiveEdge()
            {
                Distance = 1000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex1, vertex2, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });
            graph.AddEdge(vertex2, vertex3, new LiveEdge()
            {
                Distance = 10000,
                Forward = true,
                Tags = tags.Add(new TagsCollection(new Tag("highway", "residential")))
            });

            var feed = new GTFSFeed();
            feed.AddAgency(new GTFS.Entities.Agency()
            {
                Id = "0",
                Name = "Agency"
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "0",
                Code = "Code 0",
                Name = "Name 0",
                Latitude = coordinate1.Latitude,
                Longitude = coordinate1.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "1",
                Code = "Code 1",
                Name = "Name 1",
                Latitude = coordinate2.Latitude,
                Longitude = coordinate2.Longitude
            });
            feed.AddStop(new GTFS.Entities.Stop()
            {
                Id = "2",
                Code = "Code 2",
                Name = "Name 2",
                Latitude = coordinate3.Latitude,
                Longitude = coordinate3.Longitude
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "0",
                Id = "0",
                ShortName = "Route 0"
            });
            feed.AddRoute(new GTFS.Entities.Route()
            {
                AgencyId = "1",
                Id = "1",
                ShortName = "Route 1"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0",
                ShortName = "Trip 0"
            });
            feed.AddTrip(new GTFS.Entities.Trip()
            {
                Id = "1",
                RouteId = "1",
                ServiceId = "0",
                ShortName = "Trip 1"
            });
            feed.AddCalendarDate(new GTFS.Entities.CalendarDate()
            {
                Date = new DateTime(2015, 01, 01),
                ExceptionType = GTFS.Entities.Enumerations.ExceptionType.Added,
                ServiceId = "0"
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 0,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "0",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "0",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 10,
                    Seconds = 0
                },
                StopSequence = 1
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "1",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 20,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 21,
                    Seconds = 0
                },
                StopSequence = 2
            });
            feed.AddStopTime(new GTFS.Entities.StopTime()
            {
                TripId = "1",
                StopId = "2",
                ArrivalTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 31,
                    Seconds = 0
                },
                DepartureTime = new TimeOfDay()
                {
                    Hours = 10,
                    Minutes = 31,
                    Seconds = 0
                },
                StopSequence = 3
            });


            var router = MultiModalRouter.CreateFrom(graph);
            router.AddGTFSFeed(feed);
            var resolved0 = router.Resolve(vehicle, coordinate0);
            var resolved2 = router.Resolve(vehicle, coordinate2);
            var resolved3 = router.Resolve(vehicle, coordinate3);

            // a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
            var route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved2);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(5, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[3].Longitude, E);
            Assert.IsTrue(route.Segments[3].Tags.ConvertToTagsCollection().ContainsKeyValue("time_seconds", "900")); // confirm that TRIP2 was taken not TRIP3.
            Assert.AreEqual(coordinate2.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[4].Longitude, E);

            // a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
            route = router.CalculateTransit(new DateTime(2015, 01, 01, 09, 45, 00), vehicle, vehicle, vehicle, resolved0, resolved3);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(6, route.Segments.Length);
            Assert.AreEqual(coordinate0.Latitude, route.Segments[0].Latitude, E);
            Assert.AreEqual(coordinate0.Longitude, route.Segments[0].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[1].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[1].Longitude, E);
            Assert.AreEqual(coordinate1.Latitude, route.Segments[2].Latitude, E);
            Assert.AreEqual(coordinate1.Longitude, route.Segments[2].Longitude, E);
            Assert.AreEqual(coordinate2.Latitude, route.Segments[3].Latitude, E);
            Assert.AreEqual(coordinate2.Longitude, route.Segments[3].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[4].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[4].Longitude, E);
            Assert.AreEqual(coordinate3.Latitude, route.Segments[5].Latitude, E);
            Assert.AreEqual(coordinate3.Longitude, route.Segments[5].Longitude, E);
        }
    }
}