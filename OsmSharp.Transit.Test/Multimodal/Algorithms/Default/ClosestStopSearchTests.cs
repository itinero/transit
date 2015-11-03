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
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Profiles;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.Default;
using OsmSharp.Routing.Transit.Multimodal.Data;
using System.Reflection;

namespace OsmSharp.Transit.Test.Multimodal.Algorithms.Default
{
    /// <summary>
    /// Contains tests for the default closest stop search.
    /// </summary>
    [TestFixture]
    public class ClosestStopSearchTests
    {
        /// <summary>
        /// Tests searching for a closest stop when it's on the same edge as the source.
        /// </summary>
        [Test]
        public void TestSourceEdgeHasStop()
        {
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, 
                new Routing.Network.Data.EdgeData()
                {
                    Profile = 0,
                    MetaId = 0,
                    Distance = 1000
                }, null);
            routerDb.EdgeProfiles.Add(new TagsCollection());
            routerDb.EdgeMeta.Add(new TagsCollection());

            var profile = MockProfile.CarMock(x => new Speed()
            {
                Direction = 0,
                Value = .1f
            });

            var stopLinksDb = new StopLinksDb(1);
            stopLinksDb.Add(0, new RouterPoint(0.5f, 0.5f, 0, ushort.MaxValue / 2));

            var stopFound = false;
            var closestStopSearch = new ClosestStopSearch(
                routerDb, profile, stopLinksDb, new RouterPoint(0.5f, 0.5f, 0, ushort.MaxValue / 2), 1800, false);
            closestStopSearch.StopFound = (uint stopId, float seconds) =>
                {
                    Assert.AreEqual(0, stopId);
                    Assert.AreEqual(0, seconds);
                    stopFound = true;
                    return false;
                };
            closestStopSearch.Run();

            Assert.IsTrue(stopFound);

            stopFound = false;
            closestStopSearch = new ClosestStopSearch(
                routerDb, profile, stopLinksDb, new RouterPoint(0f, 0f, 0, 0), 1800, false);
            closestStopSearch.StopFound = (uint stopId, float seconds) =>
            {
                Assert.AreEqual(0, stopId);
                Assert.AreEqual(profile.Factor(null).Value * 500, seconds, .1f);
                stopFound = true;
                return false;
            };
            closestStopSearch.Run();

            Assert.IsTrue(stopFound);
        }

        /// <summary>
        /// Tests searching in network 2.
        /// </summary>
        [Test]
        public void TestNetwork2()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "OsmSharp.Transit.Test.test_data.networks.network2.geojson"));

            var profile = MockProfile.CarMock(x => new Speed()
            {
                Direction = 0,
                Value = 10f
            });

            var stopLocation = new GeoCoordinate(51.229621576122774f, 4.464208334684372f);
            var stopLinksDb = new StopLinksDb(1);
            stopLinksDb.Add(0, new RouterPoint((float)stopLocation.Latitude, (float)stopLocation.Longitude, 1, ushort.MaxValue / 2));

            var stopFound = false;
            var distanceToStop = GeoCoordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1)) + GeoCoordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(1),
                stopLocation);
            var closestStopSearch = new ClosestStopSearch(
                routerDb, profile, stopLinksDb, routerDb.Network.CreateRouterPointForVertex(0, 1), float.MaxValue, false);
            closestStopSearch.StopFound = (uint stopId, float seconds) =>
            {
                Assert.AreEqual(0, stopId);
                Assert.AreEqual(distanceToStop * 0.1, seconds, 0.01);
                stopFound = true;
                return false;
            };
            closestStopSearch.Run();

            Assert.IsTrue(stopFound);

            var path = closestStopSearch.GetPath(0);
            Assert.IsNotNull(path);
            Assert.AreEqual(distanceToStop * 0.1, path.Weight, 0.01);
            Assert.AreEqual(OsmSharp.Routing.Constants.NO_VERTEX, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1)) * 0.1, path.Weight, 0.01);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.01);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);
        }
    }
}