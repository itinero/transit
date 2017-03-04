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
using Itinero.Transit.Algorithms.Search;
using Itinero.Transit.Data;
using System.Collections.Generic;
using Itinero.LocalGeo;
using Itinero.Algorithms.Search.Hilbert;

namespace Itinero.Transit.Test.Algorithms.Search
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
            var locations = new List<Coordinate>();
            locations.Add(new Coordinate(-90, -180));
            locations.Add(new Coordinate(-90, -60));
            locations.Add(new Coordinate(-90, 60));
            locations.Add(new Coordinate(-90, 180));
            locations.Add(new Coordinate(-30, -180));
            locations.Add(new Coordinate(-30, -60));
            locations.Add(new Coordinate(-30, 60));
            locations.Add(new Coordinate(-30, 180));
            locations.Add(new Coordinate(30, -180));
            locations.Add(new Coordinate(30, -60));
            locations.Add(new Coordinate(30, 60));
            locations.Add(new Coordinate(30, 180));
            locations.Add(new Coordinate(90, -180));
            locations.Add(new Coordinate(90, -60));
            locations.Add(new Coordinate(90, 60));
            locations.Add(new Coordinate(90, 180));

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
