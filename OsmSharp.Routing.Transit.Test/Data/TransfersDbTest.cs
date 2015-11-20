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
    /// Contains tests for the transfers db.
    /// </summary>
    [TestFixture]
    public class TransfersDbTest
    {
        /// <summary>
        /// Tests adding transfers.
        /// </summary>
        [Test]
        public void TestAddTransfer()
        {
            var db = new TransfersDb(256);

            db.AddTransfer(0, 1, 60);

            var enumeration = db.GetTransferEnumerator();
            Assert.IsTrue(enumeration.MoveTo(0));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);
            Assert.IsTrue(enumeration.MoveTo(1));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            db.AddTransfer(0, 2, 61);
            db.AddTransfer(1, 2, 62);
            db.AddTransfer(2, 4, 63);
            db.AddTransfer(2, 3, 64);
            db.AddTransfer(4, 5, 65);

            enumeration = db.GetTransferEnumerator();

            Assert.IsTrue(enumeration.MoveTo(0));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(61, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(1));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(60, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(62, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(2));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(61, enumeration.Seconds);
            Assert.AreEqual(0, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(62, enumeration.Seconds);
            Assert.AreEqual(1, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(63, enumeration.Seconds);
            Assert.AreEqual(4, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(64, enumeration.Seconds);
            Assert.AreEqual(3, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveTo(4));
            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(63, enumeration.Seconds);
            Assert.AreEqual(2, enumeration.Stop);

            Assert.IsTrue(enumeration.MoveNext());
            Assert.AreEqual(65, enumeration.Seconds);
            Assert.AreEqual(5, enumeration.Stop);
        }
    }
}
