// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
