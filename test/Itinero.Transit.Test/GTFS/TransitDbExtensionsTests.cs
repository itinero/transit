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

using GTFS;
using GTFS.Entities;
using NUnit.Framework;
using Itinero.Transit.Data;
using Itinero.Transit.GTFS;
using Itinero.Attributes;

namespace Itinero.Transit.Test.GTFS
{
    /// <summary>
    /// Contains tests for the GTFS-related transit db extensions.
    /// </summary>
    [TestFixture]
    public class TransitDbExtensionsTests
    {
        /// <summary>
        /// Test loading from a GTFS feed.
        /// </summary>
        [Test]
        public void TestLoadFromOneConnection()
        {
            var transitDb = new TransitDb();
            var feed = DummyGTFSFeedBuilder.OneConnection(
                TimeOfDay.FromTotalSeconds(0), TimeOfDay.FromTotalSeconds(3600));
            transitDb.LoadFrom(feed);

            Assert.AreEqual(1, transitDb.TripsCount);
            var tripEnumerator = transitDb.GetTripsEnumerator();
            Assert.IsTrue(tripEnumerator.MoveTo(0));
            Assert.AreEqual(0, tripEnumerator.Id);
            Assert.AreEqual(0, tripEnumerator.ScheduleId);
            var tripMeta = new AttributeCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.Contains("id", "0"));
            Assert.IsTrue(tripMeta.Contains("route_id", "0"));
            Assert.IsTrue(tripMeta.Contains("service_id", "0"));
            var agencyMeta = new AttributeCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.Contains("id", "0"));
            
            Assert.AreEqual(2, transitDb.StopsCount);
            var stopEnumerator = transitDb.GetStopsEnumerator();
            Assert.IsTrue(stopEnumerator.MoveTo(0));
            Assert.AreEqual(0, stopEnumerator.Id);
            Assert.AreEqual(0, stopEnumerator.Latitude);
            Assert.AreEqual(0, stopEnumerator.Longitude);
            var stopMeta = new AttributeCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.Contains("id", "0"));
            Assert.IsTrue(stopEnumerator.MoveTo(1));
            Assert.AreEqual(1, stopEnumerator.Id);
            Assert.AreEqual(1, stopEnumerator.Latitude);
            Assert.AreEqual(1, stopEnumerator.Longitude);
            stopMeta = new AttributeCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.Contains("id", "1"));

            Assert.AreEqual(1, transitDb.ConnectionsCount);
            transitDb.SortConnections(DefaultSorting.DepartureTime, null);
            var connectionEnumerator = transitDb.GetConnectionsEnumerator(DefaultSorting.DepartureTime);
            Assert.IsTrue(connectionEnumerator.MoveTo(0));
            Assert.AreEqual(0, connectionEnumerator.Id);
            Assert.AreEqual(0, connectionEnumerator.DepartureStop);
            Assert.AreEqual(0, connectionEnumerator.DepartureTime);
            Assert.AreEqual(1, connectionEnumerator.ArrivalStop);
            Assert.AreEqual(3600, connectionEnumerator.ArrivalTime);
            Assert.AreEqual(0, connectionEnumerator.TripId);
        }

        /// <summary>
        /// Test loading from a GTFS feed.
        /// </summary>
        [Test]
        public void TestLoadFromTwoConnectionsTwoTrips()
        {
            var transitDb = new TransitDb();
            var feed = DummyGTFSFeedBuilder.TwoConnectionsTwoTrips(
                TimeOfDay.FromTotalSeconds(0), TimeOfDay.FromTotalSeconds(3600),
                TimeOfDay.FromTotalSeconds(3900), TimeOfDay.FromTotalSeconds(7200));
            transitDb.LoadFrom(feed);

            Assert.AreEqual(2, transitDb.TripsCount);
            var tripEnumerator = transitDb.GetTripsEnumerator();
            Assert.IsTrue(tripEnumerator.MoveTo(0));
            Assert.AreEqual(0, tripEnumerator.Id);
            Assert.AreEqual(0, tripEnumerator.ScheduleId);
            var tripMeta = new AttributeCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.Contains("id", "0"));
            Assert.IsTrue(tripMeta.Contains("route_id", "0"));
            Assert.IsTrue(tripMeta.Contains("service_id", "0"));
            var agencyMeta = new AttributeCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.Contains("id", "0"));
            Assert.IsTrue(tripEnumerator.MoveTo(1));
            Assert.AreEqual(1, tripEnumerator.Id);
            Assert.AreEqual(0, tripEnumerator.ScheduleId);
            tripMeta = new AttributeCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.Contains("id", "1"));
            Assert.IsTrue(tripMeta.Contains("route_id", "1"));
            Assert.IsTrue(tripMeta.Contains("service_id", "0"));
            agencyMeta = new AttributeCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.Contains("id", "0"));

            Assert.AreEqual(3, transitDb.StopsCount);
            var stopEnumerator = transitDb.GetStopsEnumerator();
            Assert.IsTrue(stopEnumerator.MoveTo(0));
            Assert.AreEqual(0, stopEnumerator.Id);
            Assert.AreEqual(0, stopEnumerator.Latitude);
            Assert.AreEqual(0, stopEnumerator.Longitude);
            var stopMeta = new AttributeCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.Contains("id", "0"));
            Assert.IsTrue(stopEnumerator.MoveTo(1));
            Assert.AreEqual(1, stopEnumerator.Id);
            Assert.AreEqual(1, stopEnumerator.Latitude);
            Assert.AreEqual(1, stopEnumerator.Longitude);
            stopMeta = new AttributeCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.Contains("id", "1"));
            Assert.IsTrue(stopEnumerator.MoveTo(2));
            Assert.AreEqual(2, stopEnumerator.Id);
            Assert.AreEqual(2, stopEnumerator.Latitude);
            Assert.AreEqual(2, stopEnumerator.Longitude);
            stopMeta = new AttributeCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.Contains("id", "2"));

            Assert.AreEqual(2, transitDb.ConnectionsCount);
            transitDb.SortConnections(DefaultSorting.DepartureTime, null);
            var connectionEnumerator = transitDb.GetConnectionsEnumerator(DefaultSorting.DepartureTime);
            Assert.IsTrue(connectionEnumerator.MoveTo(0));
            Assert.AreEqual(0, connectionEnumerator.Id);
            Assert.AreEqual(0, connectionEnumerator.DepartureStop);
            Assert.AreEqual(0, connectionEnumerator.DepartureTime);
            Assert.AreEqual(1, connectionEnumerator.ArrivalStop);
            Assert.AreEqual(3600, connectionEnumerator.ArrivalTime);
            Assert.AreEqual(0, connectionEnumerator.TripId);
            Assert.IsTrue(connectionEnumerator.MoveTo(1));
            Assert.AreEqual(1, connectionEnumerator.Id);
            Assert.AreEqual(1, connectionEnumerator.DepartureStop);
            Assert.AreEqual(3900, connectionEnumerator.DepartureTime);
            Assert.AreEqual(2, connectionEnumerator.ArrivalStop);
            Assert.AreEqual(7200, connectionEnumerator.ArrivalTime);
            Assert.AreEqual(1, connectionEnumerator.TripId);
        }

        /// <summary>
        /// Test loading a test GTFS feed.
        /// </summary>
        [Test]
        public void TestLoadFromSampleFeed()
        {
            var transitDb = new TransitDb();
            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(sample_feed.SampleFeed.BuildSource());
            transitDb.LoadFrom(feed);

            Assert.AreEqual(13, transitDb.TripsCount);
            Assert.AreEqual(9, transitDb.StopsCount);
            Assert.AreEqual(22, transitDb.ConnectionsCount);
        }
    }
}