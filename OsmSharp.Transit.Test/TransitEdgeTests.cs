//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2014 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using NUnit.Framework;
//using OsmSharp.Routing.Transit;
//using System;

//namespace OsmSharp.Transit.Test
//{
//    /// <summary>
//    /// Contains tests for the transit edge and related functionalities.
//    /// </summary>
//    [TestFixture]
//    public class TransitEdgeTests
//    {
//        /// <summary>
//        /// Tests the schedule entries.
//        /// </summary>
//        public void TestTransitEdgeScheduleEntry()
//        {
//            // create simple entries and test their properties.
//            var entry = new TransitEdgeScheduleEntry(1, new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 9, 0, 0));
//            Assert.AreEqual(8 * 60 * 60, entry.DepartureTime);
//            Assert.AreEqual(9 * 60 * 60, entry.ArrivalTime);
//            Assert.AreEqual(1 * 60 * 60, entry.Duration);

//            // create simple entries and test their properties.
//            entry = new TransitEdgeScheduleEntry(1, new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 19, 0, 0));
//            Assert.AreEqual(8 * 60 * 60, entry.DepartureTime);
//            Assert.AreEqual(19 * 60 * 60, entry.ArrivalTime);
//            Assert.AreEqual(11 * 60 * 60, entry.Duration);

//            // create simple entries and test their properties.
//            entry = new TransitEdgeScheduleEntry(1, new DateTime(2014, 01, 01, 8, 0, 0), new DateTime(2014, 01, 01, 7, 0, 0));
//            Assert.AreEqual(8 * 60 * 60, entry.DepartureTime);
//            Assert.AreEqual(7 * 60 * 60, entry.ArrivalTime);
//            Assert.AreEqual(23 * 60 * 60, entry.Duration);
//        }
//    }
//}