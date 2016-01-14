﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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
using System.IO;

namespace OsmSharp.Routing.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the connections db.
    /// </summary>
    [TestFixture]
    public class ConnectionsDbTest
    {
        /// <summary>
        /// Test setting connections.
        /// </summary>
        [Test]
        public void TestSetConnection()
        {
            var db = new ConnectionsDb();

            Assert.AreEqual(0, db.Add(0, 1, 1234, 0, 100));

            var enumerator = db.GetEnumerator();

            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.AreEqual(1, db.Add(0, 1, 1234, 100, 1000));
            Assert.AreEqual(2, db.Add(0, 2, 1234, 100, 200));
            Assert.AreEqual(3, db.Add(0, 3, 1234, 100, 200));
            Assert.AreEqual(4, db.Add(0, 4, 1234, 100, 200));
            Assert.AreEqual(5, db.Add(0, 5, 1234, 100, 100 + (1 << 15) - 1));

            enumerator = db.GetEnumerator();
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
                db.Add(0, 1, 1234, 100, 99));
        }

        /// <summary>
        /// Test connection enumerator.
        /// </summary>
        [Test]
        public void TestConnectionEnumerator()
        {
            var db = new ConnectionsDb();

            db.Add(0, 1, 1234, 0, 100);
            db.Add(0, 1, 1234, 100, 1000);
            db.Add(0, 2, 1234, 100, 200);
            db.Add(0, 3, 1234, 100, 200);
            db.Add(0, 4, 1234, 100, 200);
            db.Add(0, 5, 1234, 100, 100 + (1 << 15) - 1);

            var enumerator = db.GetEnumerator();

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

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            enumerator.Reset();
            
            while (enumerator.MoveNext()) { } // move to the end.

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(5, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(100 + (1 << 15) - 1, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(4, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(3, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(2, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(200, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(100, enumerator.DepartureTime);
            Assert.AreEqual(1000, enumerator.ArrivalTime);

            Assert.IsTrue(enumerator.MovePrevious());
            Assert.AreEqual(0, enumerator.DepartureStop);
            Assert.AreEqual(1, enumerator.ArrivalStop);
            Assert.AreEqual(1234, enumerator.TripId);
            Assert.AreEqual(0, enumerator.DepartureTime);
            Assert.AreEqual(100, enumerator.ArrivalTime);

            Assert.IsFalse(enumerator.MovePrevious());
        }

        /// <summary>
        /// Tests connection sorting.
        /// </summary>
        [Test]
        public void TestConnectionSorting()
        {
            var db = new ConnectionsDb();

            Assert.IsFalse(db.Sorting.HasValue);

            db.Add(0, 1, 1234, 0, 100);
            db.Add(0, 1, 1234, 100, 1000);
            db.Add(0, 2, 1234, 10, 200);
            db.Add(0, 3, 1234, 1000, 2000);
            db.Add(0, 4, 1234, 101, 201);
            db.Add(0, 5, 1234, 102, 101 + (1 << 15) - 1);

            db.Sort(DefaultSorting.DepartureTime, null);
            Assert.IsTrue(db.Sorting.HasValue);
            Assert.AreEqual(DefaultSorting.DepartureTime, db.Sorting.Value);

            var enumerator = db.GetEnumerator(DefaultSorting.DepartureTime);

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

            enumerator = db.GetEnumerator(DefaultSorting.ArrivalTime);

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

            var connection0 = db.Add(0, 1, 1234, 0, 9);
            var connection1 = db.Add(0, 1, 1234, 10, 19);
            var connection2 = db.Add(0, 2, 1234, 20, 29);
            var connection3 = db.Add(0, 3, 1234, 30, 39);
            var connection4 = db.Add(0, 4, 1234, 40, 49);
            var connection5 = db.Add(0, 5, 1234, 50, 59);

            db.Sort(DefaultSorting.DepartureTime, null);

            var enumerator = db.GetEnumerator(DefaultSorting.DepartureTime);

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

        /// <summary>
        /// Tests moving to a previous connection on the same trip.
        /// </summary>
        [Test]
        public void TestConnectionMoveToPreviousConnection()
        {
            var db = new ConnectionsDb();

            var connection0 = db.Add(0, 1, 1, 0, 9);
            var connection1 = db.Add(0, 1, 2, 1, 19);
            var connection2 = db.Add(0, 2, 3, 2, 29);
            var connection3 = db.Add(0, 3, 3, 3, 39);
            var connection4 = db.Add(0, 4, 2, 4, 49);
            var connection5 = db.Add(0, 5, 1, 5, 59);

            db.Sort(DefaultSorting.DepartureTime, null);

            var enumerator = db.GetEnumerator();

            enumerator.MoveTo(connection5);
            Assert.IsTrue(enumerator.MoveToPreviousConnection());
            Assert.AreEqual(connection0, enumerator.Id);

            enumerator.MoveTo(connection4);
            Assert.IsTrue(enumerator.MoveToPreviousConnection());
            Assert.AreEqual(connection1, enumerator.Id);

            enumerator.MoveTo(connection3);
            Assert.IsTrue(enumerator.MoveToPreviousConnection());
            Assert.AreEqual(connection2, enumerator.Id);

            enumerator.MoveTo(connection2);
            Assert.IsFalse(enumerator.MoveToPreviousConnection());

            enumerator.MoveTo(connection1);
            Assert.IsFalse(enumerator.MoveToPreviousConnection());

            enumerator.MoveTo(connection0);
            Assert.IsFalse(enumerator.MoveToPreviousConnection());
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var db = new ConnectionsDb();

            db.Add(0, 1, 1, 0, 9);
            db.Add(0, 1, 2, 1, 19);
            db.Add(0, 2, 3, 2, 29);
            db.Add(0, 3, 3, 3, 39);
            db.Add(0, 4, 2, 4, 49);
            db.Add(0, 5, 1, 5, 59);

            var size = 2 + 8 + (6 * 4 * 4) + (6 * 4);
            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(size, db.SizeInBytes);
                Assert.AreEqual(size, db.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests deserializing.
        /// </summary>
        [Test]
        public void TestDeserialization()
        {
            var db = new ConnectionsDb();

            db.Add(0, 1, 1, 0, 9);
            db.Add(0, 1, 2, 1, 19);
            db.Add(0, 2, 3, 2, 29);
            db.Add(0, 3, 3, 3, 39);
            db.Add(0, 4, 2, 4, 49);
            db.Add(0, 5, 1, 5, 59);

            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);
                var db1 = ConnectionsDb.Deserialize(stream);

                Assert.AreEqual(db.Count, db1.Count);
                Assert.AreEqual(db.SizeInBytes, db1.SizeInBytes);

                var enumerator = db.GetEnumerator();
                var enumerator1 = db1.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Assert.IsTrue(enumerator1.MoveNext());

                    Assert.AreEqual(enumerator.Id, enumerator1.Id);
                    Assert.AreEqual(enumerator.ArrivalStop, enumerator1.ArrivalStop);
                    Assert.AreEqual(enumerator.ArrivalTime, enumerator1.ArrivalTime);
                    Assert.AreEqual(enumerator.DepartureStop, enumerator1.DepartureStop);
                    Assert.AreEqual(enumerator.DepartureTime, enumerator1.DepartureTime);
                }
            }
        }
    }
}