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
using System;

namespace OsmSharp.Routing.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the stop links db.
    /// </summary>
    [TestFixture]
    public class StopLinksDbTests
    {
        /// <summary>
        /// Tests adding a link.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new StopLinksDb();

            db.Add(0, new RouterPoint(0, 0, 0, 0));

            var enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            db.Add(1, new RouterPoint(0, 0, 1, ushort.MaxValue));

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            enumerator.MoveTo(1);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue, enumerator.Offset);

            db.Add(1, new RouterPoint(0, 0, 2, ushort.MaxValue / 2));

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.EdgeId);
            Assert.AreEqual(0, enumerator.Offset);

            enumerator.MoveTo(1);
            Assert.AreEqual(2, enumerator.Count);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue, enumerator.Offset);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue / 2, enumerator.Offset);
        }
    }
}
