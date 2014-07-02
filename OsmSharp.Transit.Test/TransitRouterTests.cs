// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using GTFS;
using NUnit.Framework;
using OsmSharp.Routing.Transit.GTFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Transit.Test
{
    /// <summary>
    /// Contains tests for the transit router and related functionalities.
    /// </summary>
    [TestFixture]
    public class TransitRouterTests
    {
        /// <summary>
        /// Tests routes along the CITY1 sample route.
        /// </summary>
        [Test]
        public void TestCITY1()
        {
            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(SampleFeed.BuildSource());

            // read the graph.
            var router = GTFSGraphReader.CreateRouter(feed);

            // calculate some routes.

            // 4->6 @ 05:30
            var route = router.Calculate("STAGECOACH", "NANAA", new System.DateTime(2008, 01, 01, 05, 30, 0));
            Assert.AreEqual(1, route.Entries.Count);
            Assert.AreEqual(2, route.Entries[0].Stops.Count);
            Assert.AreEqual("STAGECOACH", route.Entries[0].Stops[0].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 0, 0), route.Entries[0].Stops[0].Time);
            Assert.AreEqual("NANAA", route.Entries[0].Stops[1].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 5, 0), route.Entries[0].Stops[1].Time);

            // 4-8 @ 05:30
            route = router.Calculate("STAGECOACH", "EMSI", new System.DateTime(2008, 01, 01, 05, 30, 0));
            Assert.AreEqual(1, route.Entries.Count);
            Assert.AreEqual(5, route.Entries[0].Stops.Count);
            Assert.AreEqual("STAGECOACH", route.Entries[0].Stops[0].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 0, 0), route.Entries[0].Stops[0].Time);
            Assert.AreEqual("NANAA", route.Entries[0].Stops[1].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 5, 0), route.Entries[0].Stops[1].Time);
            Assert.AreEqual("NADAV", route.Entries[0].Stops[2].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 12, 0), route.Entries[0].Stops[2].Time);
            Assert.AreEqual("DADAN", route.Entries[0].Stops[3].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 19, 0), route.Entries[0].Stops[3].Time);
            Assert.AreEqual("EMSI", route.Entries[0].Stops[4].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 01, 6, 26, 0), route.Entries[0].Stops[4].Time);

            // 4->6 @ 05:30 @ 2007-06-04
            route = router.Calculate("STAGECOACH", "NANAA", new System.DateTime(2007, 06, 04, 05, 30, 0));
            Assert.IsNull(route);
        }

        /// <summary>
        /// Tests routes along that tranfer from STBA to AAMV1.
        /// </summary>
        [Test]
        public void TestTransferSTBAToAAMV1()
        {
            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(SampleFeed.BuildSource());

            // read the graph.
            var router = GTFSGraphReader.CreateRouter(feed);

            // calculate some routes.

            // 4-9 @ 05:30 @ 2008-01-05
            var route = router.Calculate("STAGECOACH", "AMV", new System.DateTime(2008, 01, 05, 05, 30, 0));
            Assert.AreEqual(2, route.Entries.Count);
            Assert.AreEqual(2, route.Entries[0].Stops.Count);
            Assert.AreEqual("STAGECOACH", route.Entries[0].Stops[0].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 05, 6, 0, 0), route.Entries[0].Stops[0].Time);
            Assert.AreEqual("BEATTY_AIRPORT", route.Entries[0].Stops[1].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 05, 6, 20, 0), route.Entries[0].Stops[1].Time);

            Assert.AreEqual(2, route.Entries[1].Stops.Count);
            Assert.AreEqual("BEATTY_AIRPORT", route.Entries[1].Stops[0].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 05, 8, 0, 0), route.Entries[1].Stops[0].Time);
            Assert.AreEqual("AMV", route.Entries[1].Stops[1].Stop.Id);
            Assert.AreEqual(new System.DateTime(2008, 01, 05, 9, 0, 0), route.Entries[1].Stops[1].Time);

            // 4-9 @ 05:30 @ 2008-01-01
            route = router.Calculate("STAGECOACH", "AMV", new System.DateTime(2008, 01, 01, 05, 30, 0));
            Assert.IsNull(route);
        }
    }
}