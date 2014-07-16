using GTFS;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Graphs.Serialization;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.MultiModal;
using System;
using System.IO;

namespace OsmSharp.Transit.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging and use the console as output.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(
                new OsmSharp.WinForms.UI.Logging.ConsoleTraceListener());

            // read the nmbs feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            System.Console.Write("Parsing feed 'De Lijn'...");
            var delijn = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\delijn\"));
            System.Console.WriteLine("Done!");
            System.Console.Write("Parsing feed 'NMBS'...");
            var nmbs = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\nmbs\"));
            System.Console.WriteLine("Done!");

            // create router.
            //var progress = new OsmSharp.Osm.Streams.Filters.OsmStreamFilterProgress();
            //progress.RegisterSource(new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(new FileInfo(@"d:\OSM\bin\belgium-latest.osm.pbf").OpenRead()));
            //var router = MultiModalRouter.CreateFrom(progress,
            //    new OsmRoutingInterpreter());
            System.Console.Write("Loading routing graph...");
            var router = MultiModalRouter.CreateFrom(new FileInfo(@"d:\temp\belgium-latest.simple.flat.routing").OpenRead(),
                new OsmRoutingInterpreter());
            System.Console.WriteLine("Done!");

            // prefix all ids in the feeds.
            foreach(var stop in nmbs.Stops)
            {
                stop.Id = "nmbs_" + stop.Id;
            }
            foreach (var item in nmbs.Routes)
            {
                item.Id = "nmbs_" + item.Id;
            }
            foreach (var stopTime in nmbs.StopTimes)
            {
                stopTime.StopId = "nmbs_" + stopTime.StopId;
                stopTime.TripId = "nmbs_" + stopTime.TripId;
            }
            foreach (var trip in nmbs.Trips)
            {
                trip.Id = "nmbs_" + trip.Id;
                trip.RouteId = "nmbs_" + trip.RouteId;
            }

            // router.AddGTFSFeed(nmbs);
            long ticksBefore = DateTime.Now.Ticks;
            router.AddGTFSFeed(delijn);


            //var date = new DateTime(2014, 7, 2, 10, 0 ,0);

            //var gent = new GeoCoordinate(51.05642, 3.72132);
            //var antwerpen = new GeoCoordinate(51.20627, 4.39369);
            //var sintNiklaas = new GeoCoordinate(51.16995, 4.14554);
            //var beverenWaas = new GeoCoordinate(51.2090, 4.2593);
            //var lokeren = new GeoCoordinate(51.1043, 3.9940);
            //var wechel = new GeoCoordinate(51.26797, 4.80191);

            //var antwerpenZuidStation = "nmbs_32390";
            //var gentDampoortStation = "nmbs_32772";

            //System.Console.Write("Antwerpen-Zuid naar Gent-Dampoort....");
            //var route = router.CalculateTransit(date, antwerpenZuidStation, gentDampoortStation);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\transit-antwerpenZuidStation-gentDampoortStation.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Gent naar Wechel....");
            //var from = router.Resolve(Vehicle.Pedestrian, gent); // gent
            //var to = router.Resolve(Vehicle.Pedestrian, wechel); // wechel
            //route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\gent-wechel.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Gent naar Lokeren....");
            //to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.10546, 3.98864)); // lokeren
            //route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\gent-lokeren.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Lokeren naar Wechel....");
            //from = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.10546, 3.98864)); // lokeren
            //to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.26797, 4.80191)); // wechel
            //route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\lokeren-wechel.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Antwerpen naar Gent....");
            //from = router.Resolve(Vehicle.Pedestrian, antwerpen); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, gent); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Antwerpen naar Wechel....");
            //from = router.Resolve(Vehicle.Pedestrian, antwerpen); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, wechel); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Antwerpen naar Lokeren....");
            //from = router.Resolve(Vehicle.Pedestrian, antwerpen); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, lokeren); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Gent naar Sint-niklaas....");
            //from = router.Resolve(Vehicle.Pedestrian, gent); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, sintNiklaas); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Beveren-Waas naar Wechel....");
            //from = router.Resolve(Vehicle.Pedestrian, beverenWaas); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, wechel); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            //System.Console.Write("Beveren-Waas naar Gent....");
            //from = router.Resolve(Vehicle.Pedestrian, beverenWaas); // antwerpen (jo)
            //to = router.Resolve(Vehicle.Pedestrian, gent); // gent
            //route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            ////route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());
            //System.Console.WriteLine("Done!");

            long ticksAfter = DateTime.Now.Ticks;
            System.Console.WriteLine(new TimeSpan(ticksAfter - ticksBefore));
            System.Console.ReadLine();

            //var dynamicGraph = new DynamicGraphRouterDataSource<TransitEdge>(router.Graph, new TagsTableCollectionIndex());
            //var serializer = new TransitEdgeFlatfileSerializer();
            //serializer.Serialize(new FileInfo(@"d:\work\osmsharp_data\nmbs.flat").OpenWrite(), dynamicGraph, new TagsCollection());
        }
    }
}
