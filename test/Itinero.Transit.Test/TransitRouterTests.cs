// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Transit.Data;
using Itinero.Attributes;
using System;

namespace Itinero.Transit.Test
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
            db.AddStop(0, 0, db.StopAttributes.Add(new Attributes.Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attributes.Attribute("name", "stop2")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attributes.Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 3600, 3600 + 40 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            var router = new TransitRouter(db, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            var result = router.TryEarliestArrival(new DateTime(2017, 05, 10, 00, 50, 00), 0, 1, (i) => true);
            var route = result.Value;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "3000"));
            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "3600"));
            Assert.IsTrue(meta.Attributes.Contains("time", "600"));
            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "6000"));
            Assert.IsTrue(meta.Attributes.Contains("time", "3000"));

            Assert.AreEqual(3000, route.TotalTime);
        }
    }
}
