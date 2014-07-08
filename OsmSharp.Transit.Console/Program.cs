﻿using GTFS;
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

            // read the sample feed.
            var reader = new GTFSReader<GTFSFeed>(false);
            reader.DateTimeReader = (dateString) =>
            {
                var year = int.Parse(dateString.Substring(0, 4));
                var month = int.Parse(dateString.Substring(4, 2));
                var day = int.Parse(dateString.Substring(6, 2));
                return new System.DateTime(year, month, day);
            };
            var feed = reader.Read(new GTFS.IO.GTFSDirectorySource(@"c:\work\osmsharp_data\nmbs\"));

            // create router.
            //var progress = new OsmSharp.Osm.Streams.Filters.OsmStreamFilterProgress();
            //progress.RegisterSource(new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(new FileInfo(@"d:\OSM\bin\belgium-latest.osm.pbf").OpenRead()));
            //var router = MultiModalRouter.CreateFrom(progress,
            //    new OsmRoutingInterpreter());
            var router = MultiModalRouter.CreateFrom(new FileInfo(@"c:\temp\belgium-latest.simple.flat.routing").OpenRead(),
                new OsmRoutingInterpreter());

            router.AddGTFSFeed(feed);

            var date = new DateTime(2014, 7, 2, 10, 0 ,0);

            var gent = new GeoCoordinate(51.05642, 3.72132);
            var antwerpen = new GeoCoordinate(51.20627, 4.39369);
            var sintNiklaas = new GeoCoordinate(51.16995, 4.14554);
            var beverenWaas = new GeoCoordinate(51.2090, 4.2593);
            var lokeren = new GeoCoordinate(51.1043, 3.9940);

            var antwerpenZuidStation = "32390";
            var gentDampoortStation = "32772";

            var route = router.CalculateTransit(date, antwerpenZuidStation, gentDampoortStation);
            route.SaveAsGpx(new FileInfo(@"c:\temp\transit-antwerpenZuidStation-gentDampoortStation.gpx").OpenWrite());

            var from = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.05642, 3.72132)); // gent
            var to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.26797, 4.80191)); // wechel
            route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            route.SaveAsGpx(new FileInfo(@"c:\temp\gent-wechel.gpx").OpenWrite());

            to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.10546, 3.98864)); // lokeren
            route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            route.SaveAsGpx(new FileInfo(@"c:\temp\gent-lokeren.gpx").OpenWrite());

            from = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.10546, 3.98864)); // lokeren
            to = router.Resolve(Vehicle.Pedestrian, new GeoCoordinate(51.26797, 4.80191)); // wechel
            route = router.CalculateTransit(DateTime.Now, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            route.SaveAsGpx(new FileInfo(@"c:\temp\lokeren-wechel.gpx").OpenWrite());

            from = router.Resolve(Vehicle.Pedestrian, antwerpen); // antwerpen (jo)
            to = router.Resolve(Vehicle.Pedestrian, gent); // gent
            route = router.CalculateTransit(date, Vehicle.Pedestrian, Vehicle.Pedestrian, Vehicle.Pedestrian, from, to);
            route.SaveAsGpx(new FileInfo(@"c:\temp\antwerpen-gent.gpx").OpenWrite());

            // System.Console.ReadLine();

            //var dynamicGraph = new DynamicGraphRouterDataSource<TransitEdge>(router.Graph, new TagsTableCollectionIndex());
            //var serializer = new TransitEdgeFlatfileSerializer();
            //serializer.Serialize(new FileInfo(@"d:\work\osmsharp_data\nmbs.flat").OpenWrite(), dynamicGraph, new TagsCollection());
        }
    }
}