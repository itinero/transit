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

using NUnit.Framework;
using Itinero.Transit.GTFS;
using Itinero.Transit.Data;
using Itinero.Transit.Osm.Data;
using System;
using GTFS;
using System.IO;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.Transit.Test.Data
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

            db.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), new TransfersDb(1024));
        }

        /// <summary>
        /// Tests adding a transfers db.
        /// </summary>
        [Test]
        public void TestAddTransfersDb()
        {
            var db = new TransitDb();

            db.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), new TransfersDb(1024));

            Assert.IsTrue(db.HasTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest()));

            var tranfersDb = db.GetTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            Assert.IsNotNull(tranfersDb);

            Assert.Catch<ArgumentNullException>(() =>
            {
                db.AddTransfersDb(null, new TransfersDb(1024));
            });
            Assert.Catch<ArgumentNullException>(() =>
            {
                db.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), null);
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

            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            var transfers = db.GetTransfersDb(profile);
            var transferEnumerator = transfers.GetTransferEnumerator();

            Assert.IsTrue(transferEnumerator.MoveTo(stop1));
            Assert.IsTrue(transferEnumerator.MoveNext());
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(
                51.10700473650233f, 3.9084237813949585f, 51.10700473650233f, 3.9091318845748897f) *
                    profile.Factor(Osm.Data.TransitDbExtensions.DefaultEdgeProfile).Value, transferEnumerator.Seconds, 1);
            Assert.AreEqual(stop2, transferEnumerator.Stop);
        }

        /// <summary>
        /// Test serializing and deserializing.
        /// </summary>
        [Test]
        public void TestSerializeDeserialize()
        {
            var transitDb = new TransitDb();
            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(GTFS.sample_feed.SampleFeed.BuildSource());
            transitDb.LoadFrom(feed);

            Assert.AreEqual(13, transitDb.TripsCount);
            Assert.AreEqual(9, transitDb.StopsCount);
            Assert.AreEqual(22, transitDb.ConnectionsCount);
            
            using (var stream = new MemoryStream())
            {
                var size = transitDb.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);

                transitDb = TransitDb.Deserialize(stream);

                Assert.AreEqual(13, transitDb.TripsCount);
                Assert.AreEqual(9, transitDb.StopsCount);
                Assert.AreEqual(22, transitDb.ConnectionsCount);
            }
        }
    }
}