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
using OsmSharp.Math.Algorithms;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Transit.Algorithms.Search;
using OsmSharp.Routing.Transit.Data;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Test.Algorithms.Search
{
   /// <summary>
    /// Contains tests for the hilbert sort/search algorithms.
    /// </summary>
    [TestFixture]
    class HibertTests
    {
        /// <summary>
        /// Tests the sort hilbert function with order #4.
        /// </summary>
        [Test]
        public void SortHilbertTestSteps4()
        {
            var n = 4;

            // build locations.
            var locations = new List<GeoCoordinate>();
            locations.Add(new GeoCoordinate(-90, -180));
            locations.Add(new GeoCoordinate(-90, -60));
            locations.Add(new GeoCoordinate(-90, 60));
            locations.Add(new GeoCoordinate(-90, 180));
            locations.Add(new GeoCoordinate(-30, -180));
            locations.Add(new GeoCoordinate(-30, -60));
            locations.Add(new GeoCoordinate(-30, 60));
            locations.Add(new GeoCoordinate(-30, 180));
            locations.Add(new GeoCoordinate(30, -180));
            locations.Add(new GeoCoordinate(30, -60));
            locations.Add(new GeoCoordinate(30, 60));
            locations.Add(new GeoCoordinate(30, 180));
            locations.Add(new GeoCoordinate(90, -180));
            locations.Add(new GeoCoordinate(90, -60));
            locations.Add(new GeoCoordinate(90, 60));
            locations.Add(new GeoCoordinate(90, 180));

            // build db.
            var stops = new StopsDb();
            for (var stop = 0; stop < locations.Count; stop++)
            {
                stops.Add((float)locations[stop].Latitude, (float)locations[stop].Longitude, (uint)stop);
            }

            // build a sorted version in-place.
            stops.Sort(null);

            // test if sorted.
            var stopsDbEnumerator = stops.GetEnumerator();
            for (uint stop = 1; stop < stops.Count - 1; stop++)
            {
                Assert.IsTrue(
                    stopsDbEnumerator.Distance(n, stop) <=
                    stopsDbEnumerator.Distance(n, stop + 1));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, n));
            });

            // confirm sort.
            for (uint stop = 0; stop < stops.Count; stop++)
            {
                stopsDbEnumerator.MoveTo(stop);
                Assert.AreEqual(stopsDbEnumerator.Latitude, locations[(int)stop].Latitude);
                Assert.AreEqual(stopsDbEnumerator.Longitude, locations[(int)stop].Longitude);
            }
        }

        /// <summary>
        /// Tests searching the closest stop.
        /// </summary>
        [Test]
        public void SearchClosestStopTest()
        {
            var stops = new StopsDb();
            stops.Add(1, 1, 10);
            stops.Add(2, 2, 20);

            stops.Sort(null);

            var stopsDbEnumerator = stops.GetEnumerator();

            Assert.AreEqual(0, stopsDbEnumerator.SearchClosest(1, 1, 1));
            Assert.AreEqual(1, stopsDbEnumerator.SearchClosest(2, 2, 1));
            Assert.AreEqual(Constants.NoStopId, stopsDbEnumerator.SearchClosest(3, 3, .5f));
        }
    }
}
