﻿// OsmSharp - OpenStreetMap (OSM) SDK
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

            Assert.AreEqual(6, db.StopCount);

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

            Assert.AreEqual(0, db.AddConnection(0, 1, 1234, 0, 100));

            var enumerator = db.GetConnectionEnumerator();

            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.AreEqual(1, db.AddConnection(0, 1, 1234, 100, 1000));
            Assert.AreEqual(2, db.AddConnection(0, 2, 1234, 100, 200));
            Assert.AreEqual(3, db.AddConnection(0, 3, 1234, 100, 200));
            Assert.AreEqual(4, db.AddConnection(0, 4, 1234, 100, 200));
            Assert.AreEqual(5, db.AddConnection(0, 5, 1234, 100, 100 + (1 << 15) - 1));

            enumerator = db.GetConnectionEnumerator();
            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(4));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(5));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.Catch<ArgumentException>(() =>
                db.AddConnection(0, 1, 1234, 100, 100));
            Assert.Catch<ArgumentException>(() =>
                db.AddConnection(0, 1, 1234, 100, 99));
        }

        /// <summary>
        /// Test stop enumerator.
        /// </summary>
        [Test]
        public void TestStopEnumerator()
        {
            var db = new ConnectionsDb();

            db.AddStop(1.1f, 1.2f, 124);
            db.AddStop(2.1f, 2.2f, 128);
            db.AddStop(3.1f, 3.2f, 132);
            db.AddStop(4.1f, 4.2f, 136);
            db.AddStop(5.1f, 5.2f, 140);
            db.AddStop(6.1f, 6.2f, 144);

            var enumerator = db.GetStopEnumerator();

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

            db.AddConnection(0, 1, 1234, 0, 100);
            db.AddConnection(0, 1, 1234, 100, 1000);
            db.AddConnection(0, 2, 1234, 100, 200);
            db.AddConnection(0, 3, 1234, 100, 200);
            db.AddConnection(0, 4, 1234, 100, 200);
            db.AddConnection(0, 5, 1234, 100, 100 + (1 << 15) - 1);

            var enumerator = db.GetConnectionEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
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

        /// <summary>
        /// Tests connection sorting.
        /// </summary>
        [Test]
        public void TestConnectionSorting()
        {
            var db = new ConnectionsDb();

            Assert.IsFalse(db.Sorting.HasValue);

            db.AddStop(1.1f, 1.2f, 124);
            db.AddStop(2.1f, 2.2f, 128);
            db.AddStop(3.1f, 3.2f, 132);
            db.AddStop(4.1f, 4.2f, 136);
            db.AddStop(5.1f, 5.2f, 140);
            db.AddStop(6.1f, 6.2f, 144);

            db.AddConnection(0, 1, 1234, 0, 100);
            db.AddConnection(0, 1, 1234, 100, 1000);
            db.AddConnection(0, 2, 1234, 10, 200);
            db.AddConnection(0, 3, 1234, 1000, 2000);
            db.AddConnection(0, 4, 1234, 101, 201);
            db.AddConnection(0, 5, 1234, 102, 101 + (1 << 15) - 1);

            db.SortConnections(DefaultSorting.DepartureTime, null);
            Assert.IsTrue(db.Sorting.HasValue);
            Assert.AreEqual(DefaultSorting.DepartureTime, db.Sorting.Value);

            var enumerator = db.GetConnectionEnumerator(DefaultSorting.DepartureTime);

            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(10, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(101, enumerator.DepartureTime);
            Assert.AreEqual(201, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(4));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(102, enumerator.DepartureTime);
            Assert.AreEqual(101 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(5));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(1000, enumerator.DepartureTime);
            Assert.AreEqual(2000, enumerator.ArrivalTime);

            enumerator = db.GetConnectionEnumerator(DefaultSorting.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(10, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(101, enumerator.DepartureTime);
            Assert.AreEqual(201, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(4));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(1000, enumerator.DepartureTime);
            Assert.AreEqual(2000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MoveTo(5));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(102, enumerator.DepartureTime);
            Assert.AreEqual(101 + (1 << 15) - 1, enumerator.ArrivalTime);
        }

        /// <summary>
        /// Tests binary searching connections.
        /// </summary>
        [Test]
        public void TestConnectionBinarySearch()
        {
            var db = new ConnectionsDb();

            db.AddStop(1.1f, 1.2f, 124);
            db.AddStop(2.1f, 2.2f, 128);
            db.AddStop(3.1f, 3.2f, 132);
            db.AddStop(4.1f, 4.2f, 136);
            db.AddStop(5.1f, 5.2f, 140);
            db.AddStop(6.1f, 6.2f, 144);

            var connection0 = db.AddConnection(0, 1, 1234, 0, 9);
            var connection1 = db.AddConnection(0, 1, 1234, 10, 19);
            var connection2 = db.AddConnection(0, 2, 1234, 20, 29);
            var connection3 = db.AddConnection(0, 3, 1234, 30, 39);
            var connection4 = db.AddConnection(0, 4, 1234, 40, 49);
            var connection5 = db.AddConnection(0, 5, 1234, 50, 59);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            var enumerator = db.GetConnectionEnumerator(DefaultSorting.DepartureTime);

            Assert.IsTrue(enumerator.MoveToDepartureTime(5));
            Assert.AreEqual(connection1, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(15));
            Assert.AreEqual(connection2, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(25));
            Assert.AreEqual(connection3, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(35));
            Assert.AreEqual(connection4, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(45));
            Assert.AreEqual(connection5, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(0));
            Assert.AreEqual(connection0, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(10));
            Assert.AreEqual(connection1, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(20));
            Assert.AreEqual(connection2, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(30));
            Assert.AreEqual(connection3, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(40));
            Assert.AreEqual(connection4, enumerator.Id);

            Assert.IsTrue(enumerator.MoveToDepartureTime(50));
            Assert.AreEqual(connection5, enumerator.Id);

            Assert.IsFalse(enumerator.MoveToDepartureTime(51));
        }
    }
}