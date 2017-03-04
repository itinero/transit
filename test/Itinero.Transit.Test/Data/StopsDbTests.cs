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
using System.Collections.Generic;
using System.IO;
using Itinero.LocalGeo;
using Itinero.Algorithms.Search.Hilbert;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the stops db.
    /// </summary>
    [TestFixture]
    public class StopsDbTest
    {
        /// <summary>
        /// Tests adding.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new StopsDb(3);

            Assert.AreEqual(0, db.Add(1.1f, 1.2f, 124));

            var enumerator = db.GetEnumerator();
            enumerator.MoveTo(0);

            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            Assert.AreEqual(1, db.Add(2.1f, 2.2f, 128));
            Assert.AreEqual(2, db.Add(3.1f, 3.2f, 132));
            Assert.AreEqual(3, db.Add(4.1f, 4.2f, 136));
            Assert.AreEqual(4, db.Add(5.1f, 5.2f, 140));
            Assert.AreEqual(5, db.Add(6.1f, 6.2f, 144));

            Assert.AreEqual(6, db.Count);

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveTo(1);
            Assert.AreEqual(2.1f, enumerator.Latitude);
            Assert.AreEqual(2.2f, enumerator.Longitude);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveTo(2);
            Assert.AreEqual(3.1f, enumerator.Latitude);
            Assert.AreEqual(3.2f, enumerator.Longitude);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveTo(3);
            Assert.AreEqual(4.1f, enumerator.Latitude);
            Assert.AreEqual(4.2f, enumerator.Longitude);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveTo(4);
            Assert.AreEqual(5.1f, enumerator.Latitude);
            Assert.AreEqual(5.2f, enumerator.Longitude);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveTo(5);
            Assert.AreEqual(6.1f, enumerator.Latitude);
            Assert.AreEqual(6.2f, enumerator.Longitude);
            Assert.AreEqual(144, enumerator.MetaId);
        }

        /// <summary>
        /// Tests the enumerator.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            var db = new StopsDb();

            db.Add(1.1f, 1.2f, 124);
            db.Add(2.1f, 2.2f, 128);
            db.Add(3.1f, 3.2f, 132);
            db.Add(4.1f, 4.2f, 136);
            db.Add(5.1f, 5.2f, 140);
            db.Add(6.1f, 6.2f, 144);

            var enumerator = db.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(2.1f, enumerator.Latitude);
            Assert.AreEqual(2.2f, enumerator.Longitude);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(3.1f, enumerator.Latitude);
            Assert.AreEqual(3.2f, enumerator.Longitude);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(4.1f, enumerator.Latitude);
            Assert.AreEqual(4.2f, enumerator.Longitude);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(5.1f, enumerator.Latitude);
            Assert.AreEqual(5.2f, enumerator.Longitude);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(6.1f, enumerator.Latitude);
            Assert.AreEqual(6.2f, enumerator.Longitude);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            enumerator.MoveNext();
            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(2.1f, enumerator.Latitude);
            Assert.AreEqual(2.2f, enumerator.Longitude);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(3.1f, enumerator.Latitude);
            Assert.AreEqual(3.2f, enumerator.Longitude);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(4.1f, enumerator.Latitude);
            Assert.AreEqual(4.2f, enumerator.Longitude);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(5.1f, enumerator.Latitude);
            Assert.AreEqual(5.2f, enumerator.Longitude);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(6.1f, enumerator.Latitude);
            Assert.AreEqual(6.2f, enumerator.Longitude);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests sorting.
        /// </summary>
        [Test]
        public void TestSorting()
        {
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
            var db = new StopsDb(locations.Count);
            for (var stop = 0; stop < locations.Count; stop++)
            {
                db.Add((float)locations[stop].Latitude,
                    (float)locations[stop].Longitude, (uint)(stop * 2));
            }

            // build a sorted version in-place.
            db.Sort(null);

            // test if sorted.
            var enumerator = db.GetEnumerator();
            for (var stop = 1; stop < locations.Count; stop++)
            {
                enumerator.MoveTo((uint)stop - 1);
                var latitude1 = enumerator.Latitude;
                var longitude1 = enumerator.Longitude;
                enumerator.MoveTo((uint)stop);
                var latitude2 = enumerator.Latitude;
                var longitude2 = enumerator.Longitude;

                Assert.IsTrue(
                    HilbertCurve.HilbertDistance(latitude1, longitude1, HilbertExtensions.DefaultHilbertSteps) <=
                    HilbertCurve.HilbertDistance(latitude2, longitude2, HilbertExtensions.DefaultHilbertSteps));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance(x.Latitude, x.Longitude, HilbertExtensions.DefaultHilbertSteps).CompareTo(
                     HilbertCurve.HilbertDistance(y.Latitude, y.Longitude, HilbertExtensions.DefaultHilbertSteps));
            });

            // confirm sort.
            enumerator = db.GetEnumerator();
            for (var stop = 0; stop < locations.Count; stop++)
            {
                enumerator.MoveTo((uint)stop);
                Assert.AreEqual(enumerator.Latitude, locations[(int)stop].Latitude);
                Assert.AreEqual(enumerator.Longitude, locations[(int)stop].Longitude);
            }
        }

        /// <summary>
        /// Tests serializing a stops db.
        /// </summary>
        [Test]
        public void TestSerialization()
        {
            var db = new StopsDb();

            db.Add(1.1f, 1.2f, 124);
            db.Add(2.1f, 2.2f, 128);
            db.Add(3.1f, 3.2f, 132);
            db.Add(4.1f, 4.2f, 136);
            db.Add(5.1f, 5.2f, 140);
            db.Add(6.1f, 6.2f, 144);

            var size = 1 + 8 + (6 * 3 * 4);
            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(size, db.SizeInBytes);
                Assert.AreEqual(size, db.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests deserializing a stops db.
        /// </summary>
        [Test]
        public void TestDeserialization()
        {
            var db = new StopsDb();

            db.Add(1.1f, 1.2f, 124);
            db.Add(2.1f, 2.2f, 128);
            db.Add(3.1f, 3.2f, 132);
            db.Add(4.1f, 4.2f, 136);
            db.Add(5.1f, 5.2f, 140);
            db.Add(6.1f, 6.2f, 144);
            
            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);
                var db1 = StopsDb.Deserialize(stream);

                Assert.AreEqual(db.Count, db1.Count);
                Assert.AreEqual(db.SizeInBytes, db1.SizeInBytes);

                var enumerator = db.GetEnumerator();
                var enumerator1 = db1.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    Assert.IsTrue(enumerator1.MoveNext());

                    Assert.AreEqual(enumerator.Id, enumerator1.Id);
                    Assert.AreEqual(enumerator.Latitude, enumerator1.Latitude);
                    Assert.AreEqual(enumerator.Longitude, enumerator1.Longitude);
                    Assert.AreEqual(enumerator.MetaId, enumerator1.MetaId);
                }
            }
        }
    }
}