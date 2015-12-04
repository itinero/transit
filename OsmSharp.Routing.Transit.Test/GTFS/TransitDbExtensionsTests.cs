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
using OsmSharp.Routing.Transit.GTFS;
using NUnit.Framework;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Collections.Tags;
using GTFS;

namespace OsmSharp.Routing.Transit.Test.GTFS
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
            var tripMeta = new TagsCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.ContainsKeyValue("id", "0"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("route_id", "0"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("service_id", "0"));
            var agencyMeta = new TagsCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.ContainsKeyValue("id", "0"));
            
            Assert.AreEqual(2, transitDb.StopsCount);
            var stopEnumerator = transitDb.GetStopsEnumerator();
            Assert.IsTrue(stopEnumerator.MoveTo(0));
            Assert.AreEqual(0, stopEnumerator.Id);
            Assert.AreEqual(0, stopEnumerator.Latitude);
            Assert.AreEqual(0, stopEnumerator.Longitude);
            var stopMeta = new TagsCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.ContainsKeyValue("id", "0"));
            Assert.IsTrue(stopEnumerator.MoveTo(1));
            Assert.AreEqual(1, stopEnumerator.Id);
            Assert.AreEqual(1, stopEnumerator.Latitude);
            Assert.AreEqual(1, stopEnumerator.Longitude);
            stopMeta = new TagsCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.ContainsKeyValue("id", "1"));

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
            var tripMeta = new TagsCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.ContainsKeyValue("id", "0"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("route_id", "0"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("service_id", "0"));
            var agencyMeta = new TagsCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.ContainsKeyValue("id", "0"));
            Assert.IsTrue(tripEnumerator.MoveTo(1));
            Assert.AreEqual(1, tripEnumerator.Id);
            Assert.AreEqual(0, tripEnumerator.ScheduleId);
            tripMeta = new TagsCollection(transitDb.TripAttributes.Get(tripEnumerator.MetaId));
            Assert.IsTrue(tripMeta.ContainsKeyValue("id", "1"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("route_id", "1"));
            Assert.IsTrue(tripMeta.ContainsKeyValue("service_id", "0"));
            agencyMeta = new TagsCollection(transitDb.AgencyAttributes.Get(tripEnumerator.AgencyId));
            Assert.IsTrue(agencyMeta.ContainsKeyValue("id", "0"));

            Assert.AreEqual(3, transitDb.StopsCount);
            var stopEnumerator = transitDb.GetStopsEnumerator();
            Assert.IsTrue(stopEnumerator.MoveTo(0));
            Assert.AreEqual(0, stopEnumerator.Id);
            Assert.AreEqual(0, stopEnumerator.Latitude);
            Assert.AreEqual(0, stopEnumerator.Longitude);
            var stopMeta = new TagsCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.ContainsKeyValue("id", "0"));
            Assert.IsTrue(stopEnumerator.MoveTo(1));
            Assert.AreEqual(1, stopEnumerator.Id);
            Assert.AreEqual(1, stopEnumerator.Latitude);
            Assert.AreEqual(1, stopEnumerator.Longitude);
            stopMeta = new TagsCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.ContainsKeyValue("id", "1"));
            Assert.IsTrue(stopEnumerator.MoveTo(2));
            Assert.AreEqual(2, stopEnumerator.Id);
            Assert.AreEqual(2, stopEnumerator.Latitude);
            Assert.AreEqual(2, stopEnumerator.Longitude);
            stopMeta = new TagsCollection(transitDb.StopAttributes.Get(stopEnumerator.MetaId));
            Assert.IsTrue(stopMeta.ContainsKeyValue("id", "2"));

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