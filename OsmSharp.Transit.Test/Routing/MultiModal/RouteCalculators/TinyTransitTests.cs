// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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
        /// Tests a route in the most basic network: one edge, two vertices.
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
        /// Tests a route in the a basic network with one transit edge.
        /// </summary>
        /// <remarks>The network has 4 vertices: 0 --- 1km --- 1 --------- 10km --------- 2 --- 1km --- 3.
        ///          Between vertex 1 and 2 there is transit link with one trip that takes 10 mins:
        ///                                      @1 01/01/2015 10:00 -> @2 01/01/2015 10:10
        ///          What is tested: - a trip 0 -> 3 on foot leaving 01/01/2015 09:45: should take the transit link.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 01:45: should not take the transit link.
        ///                          - a trip 0 -> 3 on foot leaving 01/01/2015 10:00: should not take the transit link.
        ///                          - a trip 3 -> 0 on foot leaving 01/01/2015 09:45: should not take the transit link.
        /// </remarks>
        public void Test1Transit1()
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
    }
}
