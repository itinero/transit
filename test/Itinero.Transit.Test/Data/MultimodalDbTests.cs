// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using GTFS.Entities;
using NUnit.Framework;
using Itinero.Attributes;
using Itinero.Transit.Data;
using Itinero.Transit.GTFS;
using Itinero.Transit.Test.GTFS;
using Itinero.Transit.Test.Profiles;
using System.Linq;

namespace Itinero.Transit.Test.Data
{
    /// <summary>
    /// Contains tests for the multimodal db.
    /// </summary>
    [TestFixture]
    public class MultimodalDbTests
    { 
        /// <summary>
        /// Tests adding stop links db.
        /// </summary>
        [Test]
        public void TestAddStopLinksDb()
        {
            // build a simple network and connections db.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Transit.Test.test_data.networks.network1.geojson"));

            var transitDb = new TransitDb();
            var feed = DummyGTFSFeedBuilder.OneConnection(
                TimeOfDay.FromTotalSeconds(0), TimeOfDay.FromTotalSeconds(3600));
            feed.Stops.Get(0).Latitude = 51.22965768754021f;
            feed.Stops.Get(0).Longitude = 4.460974931716918f;
            feed.Stops.Get(1).Latitude = 51.229617377118906f;
            feed.Stops.Get(1).Longitude = 4.463152885437011f;
            transitDb.LoadFrom(feed);

            var db = new MultimodalDb(routerDb, transitDb);

            // add stop links.
            var profile = VehicleMock.Car().Fastest();
            db.AddStopLinksDb(profile);

            // check result.
            var stopLinksDb = db.GetStopLinksDb(profile);
            Assert.IsNotNull(stopLinksDb);
            var stop0 = db.TransitDb.SearchFirstStopsWithTags((t) =>
            {
                return t.Contains("id", "0");
            });
            var stop1 = db.TransitDb.SearchFirstStopsWithTags((t) =>
            {
                return t.Contains("id", "1");
            });

            var stopLinksDbEnumerator = stopLinksDb.GetEnumerator();
            stopLinksDbEnumerator.MoveTo(stop0);
            Assert.AreEqual(1, stopLinksDbEnumerator.Count);
            Assert.IsTrue(stopLinksDbEnumerator.MoveNext());
            Assert.AreEqual(0, stopLinksDbEnumerator.EdgeId);
            Assert.AreEqual(0, stopLinksDbEnumerator.Offset);
            stopLinksDbEnumerator.MoveTo(stop1);
            Assert.AreEqual(1, stopLinksDbEnumerator.Count);
            Assert.IsTrue(stopLinksDbEnumerator.MoveNext());
            Assert.AreEqual(0, stopLinksDbEnumerator.EdgeId);
            Assert.AreEqual(ushort.MaxValue, stopLinksDbEnumerator.Offset);
        }
    }
}
