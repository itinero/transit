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
using OsmSharp.Routing.Transit;
using System;

namespace OsmSharp.Transit.Test
{
    /// <summary>
    /// Contains tests for the transit edge schedule entry.
    /// </summary>
    [TestFixture]
    public class TransitEdgeScheduleEntryTests
    {
        /// <summary>
        /// Tests creating new objects.
        /// </summary>
        [Test]
        public void TestConstructors()
        {
            var entry = new TransitEdgeScheduleEntry();
            Assert.AreEqual(0, entry.ArrivalTime);
            Assert.AreEqual(0, entry.DepartureTime);
            Assert.AreEqual(0, entry.Trip);

            entry = new TransitEdgeScheduleEntry(1, 10, 11);
            Assert.AreEqual(11, entry.ArrivalTime);
            Assert.AreEqual(10, entry.DepartureTime);
            Assert.AreEqual(1, entry.Trip);

            entry = new TransitEdgeScheduleEntry(1, new DateTime(2015, 10, 01, 0, 0, 10), new DateTime(2015, 10, 01, 0, 0, 11));
            Assert.AreEqual(11, entry.ArrivalTime);
            Assert.AreEqual(10, entry.DepartureTime);
            Assert.AreEqual(1, entry.Trip);
        }

        /// <summary>
        /// Tests the duration property.
        /// </summary>
        [Test]
        public void TestDuration()
        {
            var entry = new TransitEdgeScheduleEntry();
            Assert.AreEqual(0, entry.Duration);

            entry = new TransitEdgeScheduleEntry(1, 10, 11);
            Assert.AreEqual(1, entry.Duration);

            entry = new TransitEdgeScheduleEntry(1, 11, 10);
            Assert.AreEqual(24 * 60 * 60 - 1, entry.Duration); // cycle back.

            entry = new TransitEdgeScheduleEntry(1, 10, 10);
            Assert.AreEqual(0, entry.Duration);
        }

        /// <summary>
        /// Tests the departs in property.
        /// </summary>
        [Test]
        public void TestDepartsIn()
        {
            var entry = new TransitEdgeScheduleEntry();
            Assert.AreEqual(0, entry.DepartsIn(new DateTime(2015, 10, 01, 0, 0, 0)));
            Assert.AreEqual(24 * 60 * 60 - 1, entry.DepartsIn(new DateTime(2015, 10, 01, 0, 0, 1)));

            entry = new TransitEdgeScheduleEntry(1, 10, 11);
            Assert.AreEqual(10, entry.DepartsIn(new DateTime(2015, 10, 01, 0, 0, 0)));
            Assert.AreEqual(24 * 60 * 60 - 1, entry.DepartsIn(new DateTime(2015, 10, 01, 0, 0, 11)));
        }
    }
}
