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
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Test.Data
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
                    HilbertCurve.HilbertDistance(latitude1, longitude1, Hilbert.DefaultHilbertSteps) <=
                    HilbertCurve.HilbertDistance(latitude2, longitude2, Hilbert.DefaultHilbertSteps));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, Hilbert.DefaultHilbertSteps).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, Hilbert.DefaultHilbertSteps));
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
    }
}