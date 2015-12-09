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

using GTFS.Entities;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Transit.GTFS;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Osm.Data;
using OsmSharp.Routing.Transit.Test.GTFS;
using OsmSharp.Routing.Transit.Test.Profiles;
using System;
using System.Linq;

namespace OsmSharp.Routing.Transit.Test.Data
{
    /// <summary>
    /// Contains tests related to the transit db.
    /// </summary>
    [TestFixture]
    public class TransitDbTests
    {
        /// <summary>
        /// Tests a brand new transit db.
        /// </summary>
        [Test]
        public void TestNew()
        {
            var db = new TransitDb();

            db.AddTransfersDb(Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), new TransfersDb(1024));
        }

        /// <summary>
        /// Tests adding a transfers db.
        /// </summary>
        [Test]
        public void TestAddTransfersDb()
        {
            var db = new TransitDb();

            db.AddTransfersDb(Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), new TransfersDb(1024));

            Assert.IsTrue(db.HasTransfersDb(Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest()));

            var tranfersDb = db.GetTransfersDb(Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            Assert.IsNotNull(tranfersDb);

            Assert.Catch<ArgumentNullException>(() =>
            {
                db.AddTransfersDb(null, new TransfersDb(1024));
            });
            Assert.Catch<ArgumentNullException>(() =>
            {
                db.AddTransfersDb(Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), null);
            });
            Assert.Catch<ArgumentNullException>(() =>
            {
                db.HasTransfersDb(null);
            });
            Assert.Catch<ArgumentNullException>(() =>
            {
                db.GetTransfersDb(null);
            });
        }

        /// <summary>
        /// Tests the add tranfers db extension method.
        /// </summary>
        [Test]
        public void TestAddTransfersDbExtension()
        {
            var db = new TransitDb();
            var stop1 = db.AddStop(51.10700473650233f, 3.9084237813949585f, 1);
            var stop2 = db.AddStop(51.10700473650233f, 3.9091318845748897f, 1);

            db.AddTransfersDbForPedestrians(5 * 60);

            var profile = OsmSharp.Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            var transfers = db.GetTransfersDb(profile);
            var transferEnumerator = transfers.GetTransferEnumerator();

            Assert.IsTrue(transferEnumerator.MoveTo(stop1));
            Assert.IsTrue(transferEnumerator.MoveNext());
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(
                51.10700473650233f, 3.9084237813949585f, 51.10700473650233f, 3.9091318845748897f) *
                    profile.Factor(Osm.Data.TransitDbExtensions.DefaultEdgeProfile).Value, transferEnumerator.Seconds, 1);
            Assert.AreEqual(stop2, transferEnumerator.Stop);
        }

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
                    "OsmSharp.Routing.Transit.Test.test_data.networks.network1.geojson"));

            var db = new TransitDb();
            var feed = DummyGTFSFeedBuilder.OneConnection(
                TimeOfDay.FromTotalSeconds(0), TimeOfDay.FromTotalSeconds(3600));
            feed.Stops.Get(0).Latitude = 51.22965768754021f;
            feed.Stops.Get(0).Longitude = 4.460974931716918f;
            feed.Stops.Get(1).Latitude = 51.229617377118906f;
            feed.Stops.Get(1).Longitude = 4.463152885437011f;
            db.LoadFrom(feed);

            // add stop links.
            var profile = MockProfile.CarMock();
            db.AddStopLinksDb(routerDb, profile);

            // check result.
            var stopLinksDb = db.GetStopLinksDb(profile);
            Assert.IsNotNull(stopLinksDb);
            var stop0 = db.SearchFirstStopsWithTags((t) =>
            {
                return t.ContainsKeyValue("id", "0");
            });
            var stop1 = db.SearchFirstStopsWithTags((t) =>
            {
                return t.ContainsKeyValue("id", "1");
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