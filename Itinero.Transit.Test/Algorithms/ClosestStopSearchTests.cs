// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using NUnit.Framework;
using Itinero.Profiles;
using Itinero.Transit.Algorithms;
using Itinero.Transit.Data;
using Itinero.Transit.Test.Profiles;
using System.Reflection;
using Itinero.Attributes;
using Itinero.LocalGeo;

namespace Itinero.Transit.Test.Algorithms
{
    /// <summary>
    /// Contains tests for the default closest stop search.
    /// </summary>
    [TestFixture]
    class ClosestStopSearchTests
    {
        /// <summary>
        /// Tests searching for a closest stop when it's on the same edge as the source.
        /// </summary>
        [Test]
        public void TestSourceEdgeHasStop()
        {
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 51.27018537520318f, 4.799609184265137f);
            routerDb.Network.AddVertex(1, 51.2682252886248f, 4.793150424957275f);
            routerDb.Network.AddEdge(0, 1,
                new Itinero.Data.Network.Edges.EdgeData()
                {
                    Profile = 0,
                    MetaId = 0,
                    Distance = 500
                }, null);
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.EdgeMeta.Add(new AttributeCollection());

            var profile = MockProfile.CarMock(x => new Speed()
            {
                Direction = 0,
                Value = .1f
            });

            var stopLinksDb = new StopLinksDb(1, routerDb, profile);
            stopLinksDb.Add(0, new RouterPoint(51.269138216062984f, 4.796175956726074f, 0, ushort.MaxValue / 2));
            var transitDb = new TransitDb();
            var multimodalDb = new MultimodalDb(routerDb, transitDb);
            multimodalDb.AddStopLinksDb(stopLinksDb);
            multimodalDb.TransitDb.AddStop(51.269138216062984f, 4.796175956726074f, 0);

            var stopFound = false;
            var closestStopSearch = new ClosestStopsSearch(multimodalDb,
                profile, new RouterPoint(51.269138216062984f, 4.796175956726074f, 0, ushort.MaxValue / 2), 1800, false);
            closestStopSearch.StopFound = (uint stopId, float seconds) =>
            {
                if (!stopFound)
                {
                    Assert.AreEqual(0, stopId);
                    Assert.AreEqual(0, seconds);
                    stopFound = true;
                }
                return false;
            };
            closestStopSearch.Run();

            Assert.IsTrue(closestStopSearch.HasRun);
            Assert.IsTrue(closestStopSearch.HasSucceeded);
            Assert.IsTrue(stopFound);

            var path = closestStopSearch.GetPath(0);
            Assert.IsNotNull(path);
            Assert.AreEqual(Itinero.Constants.NO_VERTEX, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);
            var point = closestStopSearch.GetTargetPoint(0);
            Assert.IsNotNull(point);
            Assert.AreEqual(0, point.EdgeId);
            Assert.AreEqual(ushort.MaxValue / 2, point.Offset);

            stopFound = false;
            closestStopSearch = new ClosestStopsSearch(multimodalDb,
                profile, new RouterPoint(51.27018537520318f, 4.799609184265137f, 0, 0), 1800, false);
            closestStopSearch.StopFound = (uint stopId, float seconds) =>
            {
                Assert.AreEqual(0, stopId);
                Assert.AreEqual(profile.Factor(null).Value * 250, seconds, .1f);
                stopFound = true;
                return false;
            };
            closestStopSearch.Run();

            Assert.IsTrue(stopFound);

            path = closestStopSearch.GetPath(0);
            Assert.IsNotNull(path);
            Assert.AreEqual(Itinero.Constants.NO_VERTEX, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(0, path.From.Vertex);
            Assert.AreEqual(profile.Factor(null).Value * 250, path.Weight, .1f);
            point = closestStopSearch.GetTargetPoint(0);
            Assert.IsNotNull(point);
            Assert.AreEqual(0, point.EdgeId);
            Assert.AreEqual(ushort.MaxValue / 2, point.Offset);
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
                    "Itinero.Transit.Test.test_data.networks.network2.geojson"));

            var profile = MockProfile.CarMock(x => new Speed()
            {
                Direction = 0,
                Value = 10f
            });

            var stopLocation = new Coordinate(51.229621576122774f, 4.464208334684372f);
            var stopLinksDb = new StopLinksDb(1, routerDb, profile);
            stopLinksDb.Add(0, new RouterPoint((float)stopLocation.Latitude, (float)stopLocation.Longitude, 1, 
                ushort.MaxValue / 2));
            var transitDb = new TransitDb();
            var multimodalDb = new MultimodalDb(routerDb, transitDb);
            multimodalDb.AddStopLinksDb(stopLinksDb);
            multimodalDb.TransitDb.AddStop(51.229621576122774f, 4.464208334684372f, 0);

            var stopFound = false;
            var distanceToStop = Coordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1)) + Coordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(1),
                stopLocation);
            var closestStopSearch = new ClosestStopsSearch(multimodalDb,
                profile, routerDb.Network.CreateRouterPointForVertex(0, 1), 3600, false);
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
            Assert.AreEqual(Itinero.Constants.NO_VERTEX, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(routerDb.Network.GetVertex(0),
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