// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using GTFS;
using GTFS.IO;
using OsmSharp.Routing.Osm.Vehicles;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.GTFS;
using OsmSharp.Routing.Transit.Test.Functional.Staging;
using System;
using OsmSharp.Routing.Transit.Osm.Data;
using System.IO;

namespace OsmSharp.Routing.Transit.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(new ConsoleTraceListener());

            OsmSharp.Routing.Osm.Vehicles.Vehicle.RegisterVehicles();

            // download and extract test-data.
            Console.WriteLine("Downloading Belgium...");
            Download.DownloadBelgiumAll();
            Console.WriteLine("Downloading NMBS GTFS...");
            Download.DownloadNMBS();

            // create test router.
            Console.WriteLine("Loading routing data for Belgium...");
            var routerDb = RouterDb.Deserialize(File.OpenRead("belgium.a.routing"));

            Console.WriteLine("Loading NMBS data...");
            var reader = new GTFSReader<GTFSFeed>(false);
            var feed = reader.Read(new GTFSDirectorySource(@"NMBS"));
            var transitDb = new TransitDb();
            var db = new MultimodalDb(routerDb, transitDb);
            db.TransitDb.LoadFrom(feed);
            db.TransitDb.SortConnections(DefaultSorting.DepartureTime, null);
            db.TransitDb.AddTransfersDb(OsmSharp.Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 100);
            db.AddStopLinksDb(Vehicle.Pedestrian.Fastest(), maxDistance: 100);

            var transitRouter = new MultimodalRouter(db, Vehicle.Pedestrian.Fastest());

            // run tests.
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test1.geojson");
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test2.geojson");
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test3.geojson");
            
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}