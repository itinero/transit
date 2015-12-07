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
using OsmSharp.Routing;
using OsmSharp.Routing.Transit.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Attributes;
using System;
using OsmSharp.Collections.Tags;

namespace OsmSharp.Transit.Test.Algorithms.OneToOne
{
    /// <summary>
    /// Contains for the profile search route builder.
    /// </summary>
    [TestFixture]
    public class ProfileSearchRouteBuilderTests
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
            db.AddStop(0, 0, db.StopAttributes.Add(new Tag("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Tag("name", "stop2")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip1")));
            db.AddConnection(0, 1, 0, 3600, 3600 + 40 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 00, 50, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 50 * 60);
            algorithm.SetTargetStop(1, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(3, route.Segments.Count);
            Assert.AreEqual(3000, route.TotalTime);

            var segment = route.Segments[0];
            Assert.AreEqual(segment.Time, 0);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(null, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            var tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey, "3000"));

            segment = route.Segments[1];
            Assert.AreEqual(segment.Time, 600);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey, "3600"));

            segment = route.Segments[2];
            Assert.AreEqual(segment.Time, 3000);
            Assert.AreEqual(segment.Latitude, 1);
            Assert.AreEqual(segment.Longitude, 1);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop2"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey, "6000"));
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
            db.AddStop(0, 0, db.StopAttributes.Add(new Tag("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Tag("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Tag("name", "stop3")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 11 * 60, 8 * 3600 + 20 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(2, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(3, route.Segments.Count);
            Assert.AreEqual(3000, route.TotalTime);

            var segment = route.Segments[0];
            Assert.AreEqual(segment.Time, 0);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(null, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            var tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));

            segment = route.Segments[1];
            Assert.AreEqual(segment.Time, 30 * 60);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));

            segment = route.Segments[2];
            Assert.AreEqual(segment.Time, 50 * 60);
            Assert.AreEqual(segment.Latitude, 2);
            Assert.AreEqual(segment.Longitude, 2);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop3"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (20 * 60)).ToInvariantString()));
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
            db.AddStop(0, 0, db.StopAttributes.Add(new Tag("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Tag("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Tag("name", "stop3")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip1")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip2")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(2, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(5, route.Segments.Count);
            Assert.AreEqual(3300, route.TotalTime);

            var segment = route.Segments[0];
            Assert.AreEqual(segment.Time, 0);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(null, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            var tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));

            segment = route.Segments[1];
            Assert.AreEqual(segment.Time, 30 * 60);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));

            segment = route.Segments[2];
            Assert.AreEqual(segment.Time, 40 * 60);
            Assert.AreEqual(segment.Latitude, 1);
            Assert.AreEqual(segment.Longitude, 1);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop2"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));

            segment = route.Segments[3];
            Assert.AreEqual(segment.Time, 45 * 60);
            Assert.AreEqual(segment.Latitude, 1);
            Assert.AreEqual(segment.Longitude, 1);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop2"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (15 * 60)).ToInvariantString()));

            segment = route.Segments[4];
            Assert.AreEqual(segment.Time, 55 * 60);
            Assert.AreEqual(segment.Latitude, 2);
            Assert.AreEqual(segment.Longitude, 2);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop3"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip2"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (25 * 60)).ToInvariantString()));
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
            db.AddStop(0, 0, db.StopAttributes.Add(new Tag("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Tag("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Tag("name", "stop3")));
            db.AddStop(3, 3, db.StopAttributes.Add(new Tag("name", "stop4")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip1")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Tag("name", "trip2")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(2, 3, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            // build dummy transfers db.
            var transfersDb = new TransfersDb(1024);
            transfersDb.AddTransfer(1, 2, 100);

            // run algorithm.
            var departureTime = new DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime, transfersDb,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(3, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(6, route.Segments.Count);
            Assert.AreEqual(3300, route.TotalTime);

            var segment = route.Segments[0];
            Assert.AreEqual(segment.Time, 0);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(null, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            var tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));

            segment = route.Segments[1];
            Assert.AreEqual(segment.Time, 30 * 60);
            Assert.AreEqual(segment.Latitude, 0);
            Assert.AreEqual(segment.Longitude, 0);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));

            segment = route.Segments[2];
            Assert.AreEqual(segment.Time, 40 * 60);
            Assert.AreEqual(segment.Latitude, 1);
            Assert.AreEqual(segment.Longitude, 1);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop2"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip1"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));

            segment = route.Segments[3];
            Assert.AreEqual(segment.Time, 40 * 60 + 100);
            Assert.AreEqual(segment.Latitude, 2);
            Assert.AreEqual(segment.Longitude, 2);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.TransferProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop3"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60) + 100).ToInvariantString()));

            segment = route.Segments[4];
            Assert.AreEqual(segment.Time, 45 * 60);
            Assert.AreEqual(segment.Latitude, 2);
            Assert.AreEqual(segment.Longitude, 2);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.WaitProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(2, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop3"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (15 * 60)).ToInvariantString()));

            segment = route.Segments[5];
            Assert.AreEqual(segment.Time, 55 * 60);
            Assert.AreEqual(segment.Latitude, 3);
            Assert.AreEqual(segment.Longitude, 3);
            Assert.AreEqual(OsmSharp.Routing.Transit.Constants.VehicleProfile, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            tags = segment.Tags.ConvertToTagsCollection();
            Assert.AreEqual(3, tags.Count);
            Assert.IsTrue(tags.ContainsKeyValue("stop_name", "stop4"));
            Assert.IsTrue(tags.ContainsKeyValue("trip_name", "trip2"));
            Assert.IsTrue(tags.ContainsKeyValue(OsmSharp.Routing.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (25 * 60)).ToInvariantString()));
        }
    }
}