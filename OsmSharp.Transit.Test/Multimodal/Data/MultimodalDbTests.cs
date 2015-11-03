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
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Network.Data;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.Multimodal.Data;
using OsmSharp.Transit.Test.Data.GTFS;
using System.Linq;

namespace OsmSharp.Transit.Test.Multimodal.Data
{
    /// <summary>
    /// Contains tests for the multimodal db.
    /// </summary>
    [TestFixture]
    public class MultimodalDbTests
    {
        /// <summary>
        /// Tests the properties.
        /// </summary>
        [Test]
        public void TestProperties()
        {
            var routerDb = new RouterDb();
            var connectionsDb = new GTFSConnectionsDb(GTFSConnectionsDbBuilder.Empty());
            var db = new MultimodalDb(routerDb, connectionsDb);

            Assert.AreEqual(routerDb, db.RouterDb);
            Assert.AreEqual(connectionsDb, db.ConnectionsDb);
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
                    "OsmSharp.Transit.Test.test_data.networks.network1.geojson"));
            var connectionsDb = new GTFSConnectionsDb(
                GTFSConnectionsDbBuilder.OneStop(new GeoCoordinate(51.2295837850742f,
                    4.462069272994995f)));
            var db = new MultimodalDb(routerDb, connectionsDb);

            // add stop links.
            db.AddStopLinksDb(MockProfile.CarMock());
            
            // check result.
            StopLinksDb linksDb;
            Assert.IsTrue(db.TryGetStopLinksDb(MockProfile.CarMock(), out linksDb));
            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.IsTrue(linksDb.HasLink(edge.Id));
            var enumerator = linksDb.GetEnumerator();
            enumerator.MoveTo(edge.Id);
            Assert.IsTrue(enumerator.MoveNext());
            var point = enumerator.RouterPoint;
            Assert.AreEqual(51.2295837850742, point.Latitude, 0.0001);
            Assert.AreEqual(4.462069272994995, point.Longitude, 0.0001);
            Assert.AreEqual(edge.Id, point.EdgeId);
            Assert.AreEqual(0, enumerator.StopId);
        }
    }
}