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
    /// Contains tests for the trips db.
    /// </summary>
    [TestFixture]
    public class TripDbTests
    {
        /// <summary>
        /// Tests adding.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new TripsDb(3);

            Assert.AreEqual(0, db.Add(1, 2, 124));

            var enumerator = db.GetEnumerator();
            enumerator.MoveTo(0);

            Assert.AreEqual(1, enumerator.ScheduleId);
            Assert.AreEqual(2, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            Assert.AreEqual(1, db.Add(2, 22, 128));
            Assert.AreEqual(2, db.Add(3, 32, 132));
            Assert.AreEqual(3, db.Add(4, 42, 136));
            Assert.AreEqual(4, db.Add(5, 52, 140));
            Assert.AreEqual(5, db.Add(6, 62, 144));

            Assert.AreEqual(6, db.Count);

            enumerator = db.GetEnumerator();

            enumerator.MoveTo(0);
            Assert.AreEqual(1, enumerator.ScheduleId);
            Assert.AreEqual(2, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveTo(1);
            Assert.AreEqual(2, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveTo(2);
            Assert.AreEqual(3, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveTo(3);
            Assert.AreEqual(4, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveTo(4);
            Assert.AreEqual(5, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveTo(5);
            Assert.AreEqual(6, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);
        }

        /// <summary>
        /// Tests the enumerator.
        /// </summary>
        [Test]
        public void TestEnumerator()
        {
            var db = new TripsDb();

            db.Add(11, 12, 124);
            db.Add(21, 22, 128);
            db.Add(31, 32, 132);
            db.Add(41, 42, 136);
            db.Add(51, 52, 140);
            db.Add(61, 62, 144);

            var enumerator = db.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual(11, enumerator.ScheduleId);
            Assert.AreEqual(12, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(21, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(31, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(41, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(51, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(61, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());

            enumerator.Reset();

            enumerator.MoveNext();
            Assert.AreEqual(11, enumerator.ScheduleId);
            Assert.AreEqual(12, enumerator.AgencyId);
            Assert.AreEqual(124, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(21, enumerator.ScheduleId);
            Assert.AreEqual(22, enumerator.AgencyId);
            Assert.AreEqual(128, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(31, enumerator.ScheduleId);
            Assert.AreEqual(32, enumerator.AgencyId);
            Assert.AreEqual(132, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(41, enumerator.ScheduleId);
            Assert.AreEqual(42, enumerator.AgencyId);
            Assert.AreEqual(136, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(51, enumerator.ScheduleId);
            Assert.AreEqual(52, enumerator.AgencyId);
            Assert.AreEqual(140, enumerator.MetaId);

            enumerator.MoveNext();
            Assert.AreEqual(61, enumerator.ScheduleId);
            Assert.AreEqual(62, enumerator.AgencyId);
            Assert.AreEqual(144, enumerator.MetaId);

            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}
