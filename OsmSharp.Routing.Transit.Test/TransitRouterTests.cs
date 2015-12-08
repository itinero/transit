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
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Attributes;
using System;

namespace OsmSharp.Routing.Transit.Test
{
    /// <summary>
    /// Contains test for the transit router.
    /// </summary>
    [TestFixture]
    class TransitRouterTests
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

            var router = new TransitRouter(db, OsmSharp.Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            var result = router.TryEarliestArrival(new DateTime(2017, 05, 10, 00, 50, 00), 0, 1, (i) => true);
            var route = result.Value;

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
    }
}
