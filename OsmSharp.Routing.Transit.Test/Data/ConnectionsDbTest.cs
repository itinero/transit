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
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the connections db.
    /// </summary>
    [TestFixture]
    public class ConnectionsDbTest
    {
        /// <summary>
        /// Tests setting stops.
        /// </summary>
        [Test]
        public void TestSetStop()
        {
            var db = new ConnectionsDb(3, 2048);

            Assert.AreEqual(0, db.AddStop(1.1f, 1.2f, 124));

            var enumerator = db.GetStopEnumerator();
            enumerator.MoveTo(0);

            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            Assert.AreEqual(1, db.AddStop(2.1f, 2.2f, 128));
            Assert.AreEqual(2, db.AddStop(3.1f, 3.2f, 132));
            Assert.AreEqual(3, db.AddStop(4.1f, 4.2f, 136));
            Assert.AreEqual(4, db.AddStop(5.1f, 5.2f, 140));
            Assert.AreEqual(5, db.AddStop(6.1f, 6.2f, 144));

            enumerator = db.GetStopEnumerator();

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
        /// Test setting connections.
        /// </summary>
        [Test]
        public void TestSetConnection()
        {
            var db = new ConnectionsDb();

            db.AddStop(1.1f, 1.2f, 124);
            db.AddStop(2.1f, 2.2f, 128);
            db.AddStop(3.1f, 3.2f, 132);
            db.AddStop(4.1f, 4.2f, 136);
            db.AddStop(5.1f, 5.2f, 140);
            db.AddStop(6.1f, 6.2f, 144);

            db.SetConnection(0, 0, 1, 1234, 0, 100);

            var enumerator = db.GetConnectionEnumerator();

            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            db.SetConnection(1, 0, 1, 1234, 100, 1000);
            db.SetConnection(2, 0, 2, 1234, 100, 200);
            db.SetConnection(3, 0, 3, 1234, 100, 200);
            db.SetConnection(4, 0, 4, 1234, 100, 200);
            db.SetConnection(5, 0, 5, 1234, 100, 100 + (1 << 15) - 1);

            enumerator = db.GetConnectionEnumerator();
            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(2, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(3, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(4));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(4, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(5));
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(5, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);
        }

        /// <summary>
        /// Test connection enumerator.
        /// </summary>
        [Test]
        public void TestConnectionEnumerator()
        {
            var db = new ConnectionsDb();

            db.AddStop(1.1f, 1.2f, 124);
            db.AddStop(2.1f, 2.2f, 128);
            db.AddStop(3.1f, 3.2f, 132);
            db.AddStop(4.1f, 4.2f, 136);
            db.AddStop(5.1f, 5.2f, 140);
            db.AddStop(6.1f, 6.2f, 144);

            db.SetConnection(0, 0, 1, 1234, 0, 100);
            db.SetConnection(1, 0, 1, 1234, 100, 1000);
            db.SetConnection(2, 0, 2, 1234, 100, 200);
            db.SetConnection(3, 0, 3, 1234, 100, 200);
            db.SetConnection(4, 0, 4, 1234, 100, 200);
            db.SetConnection(5, 0, 5, 1234, 100, 100 + (1 << 15) - 1);

            var enumerator = db.GetConnectionEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(2, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(3, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(4, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(5, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(1, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(2, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(3, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(4, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Stop1);
            Assert.AreEqual(5, enumerator.Stop2);
            Assert.AreEqual(1234, enumerator.ProfileId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests stop sorting.
        /// </summary>
        [Test]
        public void TestStopSorting()
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
            var db = new ConnectionsDb(locations.Count, 1024);
            for (var stop = 0; stop < locations.Count; stop++)
            {
                db.AddStop((float)locations[stop].Latitude,
                    (float)locations[stop].Longitude, (uint)stop * 2);
            }

            // build a sorted version in-place.
            db.SortStops(null);

            // test if sorted.
            var enumerator = db.GetStopEnumerator();
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
            enumerator = db.GetStopEnumerator();
            for (var stop = 0; stop < locations.Count; stop++)
            {
                enumerator.MoveTo((uint)stop);
                Assert.AreEqual(enumerator.Latitude, locations[(int)stop].Latitude);
                Assert.AreEqual(enumerator.Longitude, locations[(int)stop].Longitude);
            }
        }
    }
}