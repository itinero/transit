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
using OsmSharp.Routing.Transit.Data;

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

            db.SetStop(0, 1.1f, 1.2f, 124);

            var enumerator = db.GetStopEnumerator();
            enumerator.MoveTo(0);

            Assert.AreEqual(1.1f, enumerator.Latitude);
            Assert.AreEqual(1.2f, enumerator.Longitude);
            Assert.AreEqual(124, enumerator.MetaId);

            db.SetStop(1, 2.1f, 2.2f, 128);
            db.SetStop(2, 3.1f, 3.2f, 132);
            db.SetStop(3, 4.1f, 4.2f, 136);
            db.SetStop(4, 5.1f, 5.2f, 140);
            db.SetStop(5, 6.1f, 6.2f, 144);

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

            db.SetStop(0, 1.1f, 1.2f, 124);
            db.SetStop(1, 2.1f, 2.2f, 128);
            db.SetStop(2, 3.1f, 3.2f, 132);
            db.SetStop(3, 4.1f, 4.2f, 136);
            db.SetStop(4, 5.1f, 5.2f, 140);
            db.SetStop(5, 6.1f, 6.2f, 144);

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

            db.SetStop(0, 1.1f, 1.2f, 124);
            db.SetStop(1, 2.1f, 2.2f, 128);
            db.SetStop(2, 3.1f, 3.2f, 132);
            db.SetStop(3, 4.1f, 4.2f, 136);
            db.SetStop(4, 5.1f, 5.2f, 140);
            db.SetStop(5, 6.1f, 6.2f, 144);

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
    }
}