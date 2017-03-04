// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
