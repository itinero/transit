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
using OsmSharp.Routing.Transit.Multimodal.Data;

namespace OsmSharp.Transit.Test.Multimodal.Data
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
        public void TestAddLink()
        {
            var linksDb = new StopLinksDb(1);

            // add link.
            linksDb.Add(0, new Routing.RouterPoint(10, 11, 1012, 0));

            // check result.
            Assert.IsTrue(linksDb.HasLink(1012));
            var enumerator = linksDb.GetEnumerator();
            enumerator.MoveTo(1012);
            Assert.IsTrue(enumerator.MoveNext());
            var point = enumerator.RouterPoint;
            Assert.AreEqual(10, point.Latitude);
            Assert.AreEqual(11, point.Longitude);
            Assert.AreEqual(0, point.Offset);
            Assert.AreEqual(1012, point.EdgeId);
            Assert.AreEqual(0, enumerator.StopId);
        }

        /// <summary>
        /// Tests adding multiple links.
        /// </summary>
        [Test]
        public void TestAddMultipleLinks()
        {
            var linksDb = new StopLinksDb(1);

            // add link.
            linksDb.Add(0, new Routing.RouterPoint(10, 11, 1012, 457));
            linksDb.Add(0, new Routing.RouterPoint(12, 14, 1025, 14));

            // check result.
            Assert.IsTrue(linksDb.HasLink(1012));
            var enumerator = linksDb.GetEnumerator();
            enumerator.MoveTo(1012);
            Assert.IsTrue(enumerator.MoveNext());
            var point = enumerator.RouterPoint;
            Assert.AreEqual(10, point.Latitude);
            Assert.AreEqual(11, point.Longitude);
            Assert.AreEqual(457, point.Offset);
            Assert.AreEqual(1012, point.EdgeId);
            Assert.AreEqual(0, enumerator.StopId);

            // check result.
            Assert.IsTrue(linksDb.HasLink(1025));
            enumerator = linksDb.GetEnumerator();
            enumerator.MoveTo(1025);
            Assert.IsTrue(enumerator.MoveNext());
            point = enumerator.RouterPoint;
            Assert.AreEqual(12, point.Latitude);
            Assert.AreEqual(14, point.Longitude);
            Assert.AreEqual(14, point.Offset);
            Assert.AreEqual(1025, point.EdgeId);
            Assert.AreEqual(0, enumerator.StopId);
        }

        /// <summary>
        /// Tests adding multiple links.
        /// </summary>
        [Test]
        public void TestAddMultipleLinksOnOneEdge()
        {
            var linksDb = new StopLinksDb(2);

            // add link.
            linksDb.Add(0, new Routing.RouterPoint(10, 11, 1012, 457));
            linksDb.Add(1, new Routing.RouterPoint(12, 14, 1012, 14));

            // check result.
            Assert.IsTrue(linksDb.HasLink(1012));
            var enumerator = linksDb.GetEnumerator();
            enumerator.MoveTo(1012);
            Assert.IsTrue(enumerator.MoveNext());
            var point = enumerator.RouterPoint;
            Assert.AreEqual(12, point.Latitude);
            Assert.AreEqual(14, point.Longitude);
            Assert.AreEqual(14, point.Offset);
            Assert.AreEqual(1012, point.EdgeId);
            Assert.AreEqual(1, enumerator.StopId);
            Assert.IsTrue(enumerator.MoveNext());
            point = enumerator.RouterPoint;
            Assert.AreEqual(10, point.Latitude);
            Assert.AreEqual(11, point.Longitude);
            Assert.AreEqual(457, point.Offset);
            Assert.AreEqual(1012, point.EdgeId);
            Assert.AreEqual(0, enumerator.StopId);
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}