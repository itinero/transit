// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2017 Abelshausen Ben
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

using Itinero.Transit.Data;
using Itinero.Transit.Osm.Data;
using System;

namespace Itinero.Transit.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests to build a multimodal db.
    /// </summary>
    public static class MultimodalDbBuilder
    {
        /// <summary>
        /// Tests building a multimodal db.
        /// </summary>
        public static MultimodalDb Run(RouterDb routerDb, TransitDb transitDb)
        {
            return MultimodalDbBuilder.GetTestBuildMultimopdalDb(routerDb, transitDb).TestPerf(
                "Testing build a multimodal db.");
        }

        /// <summary>
        /// Tests building a multimodaldb.
        /// </summary>
        public static Func<MultimodalDb> GetTestBuildMultimopdalDb(RouterDb routerDb, TransitDb transitDb)
        {
            return () =>
            {
                var multimodalDb = new MultimodalDb(routerDb, transitDb);
                multimodalDb.TransitDb.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 100);
                multimodalDb.AddStopLinksDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), maxDistance: 100);
                return multimodalDb;
            };
        }
    }
}
