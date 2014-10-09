using GTFS;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Osm.Streams;
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

            // create router.
            System.Console.Write("Loading routing graph...");
            var router = MultiModalRouter.CreateFrom(new PBFOsmStreamSource(new FileInfo(@"d:\OSM\bin\kortrijk.new.osm.pbf").OpenRead()),
                new OsmRoutingInterpreter());
            System.Console.WriteLine("Done!");

            //// read the nmbs feed.
            //var reader = new GTFSReader<GTFSFeed>(false);
            //System.Console.Write("Parsing feed 'De Lijn'...");
            //var delijn = reader.Read(new GTFS.IO.GTFSDirectorySource(@"D:\work\osmsharp_data\delijn_2014-04"));
            //System.Console.WriteLine("Done!");
            //System.Console.Write("Parsing feed 'NMBS'...");
            //var nmbs = reader.Read(new GTFS.IO.GTFSDirectorySource(@"d:\work\osmsharp_data\nmbs\"));
            //System.Console.WriteLine("Done!");

            //// prefix all ids in the feeds.
            //foreach(var stop in nmbs.GetStops())
            //{
            //    stop.Id = "nmbs_" + stop.Id;
            //}
            //foreach (var item in nmbs.GetRoutes())
            //{
            //    item.Id = "nmbs_" + item.Id;
            //}
            //foreach (var stopTime in nmbs.GetStopTimes())
            //{
            //    stopTime.StopId = "nmbs_" + stopTime.StopId;
            //    stopTime.TripId = "nmbs_" + stopTime.TripId;
            //}
            //foreach (var trip in nmbs.GetTrips())
            //{
            //    trip.Id = "nmbs_" + trip.Id;
            //    trip.RouteId = "nmbs_" + trip.RouteId;
            //}

            // http://localhost:12010/kortrijk_new/multimodal?callback=PT.JSONP.callbacks.route0&vehicle=car|car|car&time=201408071200&loc=50.821808,3.262655&loc=50.821591,3.261169
            long ticksBefore = DateTime.Now.Ticks;
            //System.Console.Write("belgium.train.example1....");
            //var departure = router.Resolve(Vehicle.Car, new GeoCoordinate(50.821808, 3.262655));
            //var arrival = router.Resolve(Vehicle.Car, new GeoCoordinate(50.821591, 3.261169));
            //var date = new DateTime(2014, 09, 26, 07, 20, 00);
            //var route = router.CalculateTransit(date, Vehicle.Car, Vehicle.Car, Vehicle.Car, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\kortrijk.result.geojson");
            //System.Console.WriteLine("Done!");

            //router.AddGTFSFeed(nmbs);
            //router.AddGTFSFeed(delijn);

            //long ticksBefore = DateTime.Now.Ticks;
            //System.Console.Write("belgium.train.example1....");
            //var departure = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.1405998794364, 4.563612341880798));
            //var arrival = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(50.84142172612373, 4.336799383163452));
            //var date = new DateTime(2014, 09, 26, 07, 20, 00);
            //var route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.train.example1.result.geojson");
            //System.Console.WriteLine("Done!");

            //System.Console.Write("belgium.train.example2....");
            //departure = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.99924522240044, 4.831471145153046));
            //arrival = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.8413810765485, 4.33685302734375));
            //date = new DateTime(2014, 09, 26, 07, 30, 00);
            //route = router.CalculateTransit(date, Vehicle.Bicycle, Vehicle.Bicycle, Vehicle.Bicycle, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.train.example2.result.geojson");
            //System.Console.WriteLine("Done!");

            //System.Console.Write("belgium.train.example3....");
            //departure = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.99924522240044, 4.831471145153046));
            //arrival = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(51.01972453598589, 4.482502341270447));
            //date = new DateTime(2014, 09, 26, 07, 15, 00);
            //route = router.CalculateTransit(date, Vehicle.Bicycle, Vehicle.Bicycle, Vehicle.Bicycle, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.train.example3.result.geojson");
            //System.Console.WriteLine("Done!");

            //System.Console.Write("belgium.train.example4....");
            //departure = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(51.01972453598589, 4.482502341270447));
            //arrival = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.99924522240044, 4.831471145153046));
            //date = new DateTime(2014, 09, 26, 17, 55, 00);
            //route = router.CalculateTransit(date, Vehicle.Bicycle, Vehicle.Bicycle, Vehicle.Bicycle, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.train.example4.result.geojson");
            //System.Console.WriteLine("Done!");

            //System.Console.Write("belgium.train.example5....");
            //departure = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.8413810765485, 4.33685302734375));
            //arrival = router.Resolve(Vehicle.Bicycle, new GeoCoordinate(50.99924522240044, 4.831471145153046));
            //date = new DateTime(2014, 09, 26, 17, 55, 00);
            //route = router.CalculateTransit(date, Vehicle.Bicycle, Vehicle.Bicycle, Vehicle.Bicycle, departure, arrival);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.train.example5.result.geojson");
            //System.Console.WriteLine("Done!");

            //System.Console.Write("belgium.trainandbus.example6....");
            //var from = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.05647, 3.72130));
            //var to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.26798, 4.80193));
            //date = new DateTime(2014, 09, 26, 17, 55, 00);
            //route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            //WriteGeoJSON(router, route, @"c:\temp\belgium.trainandbus.example6.result.geojson");
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
        }

        /// <summary>
        /// Writes a route as geojson to the given file.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="route"></param>
        /// <param name="fileName"></param>
        private static void WriteGeoJSON(MultiModalRouter router, Route route, string fileName)
        {
            var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
            var features= router.GetFeatures(route, true);
            File.WriteAllText(fileName, geoJsonWriter.Write(features));
        }
    }
}