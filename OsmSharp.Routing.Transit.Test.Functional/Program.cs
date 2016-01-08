using GTFS;
using GTFS.IO;
using OsmSharp.Routing.Osm.Vehicles;
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Transit.GTFS;
using OsmSharp.Routing.Transit.Test.Functional.Staging;
using System;
using OsmSharp.Routing.Transit.Osm.Data;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            OsmSharp.Routing.Osm.Vehicles.Vehicle.RegisterVehicles();

            // download and extract test-data.
            Console.WriteLine("Downloading Belgium...");
            Download.DownloadBelgiumAll();
            Console.WriteLine("Downloading NMBS GTFS...");
            Download.DownloadNMBS();

            // create test router.
            Console.WriteLine("Loading routing data for Belgium...");
            var router = RouterDb.Deserialize(File.OpenRead("belgium.a.routing"));

            Console.WriteLine("Loading NMBS data...");
            var reader = new GTFSReader<GTFSFeed>(false);
            var feed = reader.Read(new GTFSDirectorySource(@"NMBS"));
            var db = new TransitDb();
            db.LoadFrom(feed);
            db.SortConnections(DefaultSorting.DepartureTime, null);
            db.AddTransfersDb(OsmSharp.Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 100);
            db.AddStopLinksDb(router, Vehicle.Pedestrian.Fastest(), maxDistance: 100);

            var transitRouter = new MultimodalRouter(router, db, Vehicle.Pedestrian.Fastest());

            // run tests.
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test1.geojson");
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test2.geojson");
            Tests.Runner.Test(transitRouter, "OsmSharp.Routing.Transit.Test.Functional.Tests.Belgium.test3.geojson");
            
            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
