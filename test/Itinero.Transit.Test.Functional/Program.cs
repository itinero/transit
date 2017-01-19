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

using GTFS;
using GTFS.IO;
using Itinero.Osm.Vehicles;
using Itinero.Transit.Data;
using Itinero.Transit.GTFS;
using Itinero.Transit.Test.Functional.Staging;
using System;
using Itinero.Transit.Osm.Data;
using System.IO;
using Itinero.Transit.Test.Functional.Tests;

namespace Itinero.Transit.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", origin, level, message));
            };
            
            // download and extract test-data.
            Console.WriteLine("Downloading Belgium...");
            Download.DownloadBelgiumAll();
            Console.WriteLine("Downloading NMBS GTFS...");
            var nmbsGtfs = Download.DownloadNMBS();
            Console.WriteLine("Downloading Delijn GTFS...");
            var delijnGfts = Download.DownloadDeLijn();
            Console.WriteLine("Downloading TEC GTFS...");
            var tecGtfs = Download.DownloadTEC();
            Console.WriteLine("Downloading MIVB GTFS...");
            var mivbGtfs = Download.DownloadMIVB();

            // build routerdb and save the result.
            var routerDb = Staging.RouterDbBuilder.BuildBelgium();

            // build transitdb's.
            var nmbs = TransitDbBuilder.RunOrLoad(nmbsGtfs);
            var delijn = TransitDbBuilder.RunOrLoad(delijnGfts);
            var tec = TransitDbBuilder.RunOrLoad(tecGtfs);
            var mivb = TransitDbBuilder.RunOrLoad(mivbGtfs);

            // merge transit db's.
            var transitDb = TransitDbBuilder.Merge(nmbs); // TODO: figure out why system is broken when loading multiple operators.

            // build multimodal db.
            var multimodalDb = MultimodalDbBuilder.Run(routerDb, transitDb);

            // create transit router.
            var transitRouter = new MultimodalRouter(multimodalDb, Vehicle.Pedestrian.Fastest());

            // run tests.
            Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test1.geojson");
            Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test2.geojson");
            Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test3.geojson");

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}