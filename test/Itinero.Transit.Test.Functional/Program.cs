// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
using System.Collections.Generic;
using Itinero.LocalGeo;
using NetTopologySuite.Geometries;
using Itinero.Geo;
using NetTopologySuite.Features;
using System.Linq;
using Itinero.Transit.Geo;

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
            //Console.WriteLine("Downloading TEC GTFS...");
            //var tecGtfs = Download.DownloadTEC();
            //Console.WriteLine("Downloading MIVB GTFS...");
            //var mivbGtfs = Download.DownloadMIVB();

            // build routerdb and save the result.
            var routerDb = Staging.RouterDbBuilder.BuildBelgium();

            // build transitdb's.
            var nmbs = TransitDbBuilder.RunOrLoad(nmbsGtfs);
            var delijn = TransitDbBuilder.RunOrLoad(delijnGfts);
            //var tec = TransitDbBuilder.RunOrLoad(tecGtfs);
            //var mivb = TransitDbBuilder.RunOrLoad(mivbGtfs);

            // merge transit db's.
            var transitDb = TransitDbBuilder.Merge(nmbs, delijn); // TODO: figure out why system is broken when loading multiple operators.

            //var tripId = kempen.SearchAllTrips((meta) =>
            //{
            //    string name;
            //    if (meta.TryGetValue("headsign", out name))
            //    {
            //        if (name.ToLowerInvariant().Contains("wechelderzande"))
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //}).First();
            //var features = kempen.GetTripFeatures(tripId);
            //var json = ToJson(features);

            // build multimodal db.
            var multimodalDb = MultimodalDbBuilder.Run(routerDb, transitDb);

            // create transit router.
            var transitRouter = new MultimodalRouter(multimodalDb, Vehicle.Pedestrian.Fastest());

            var route = transitRouter.TryEarliestArrival(new DateTime(2017, 11, 08, 18, 00, 00),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("antwerp-central")),
                Vehicle.Pedestrian.Fastest(),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("turnhout")),
                Vehicle.Pedestrian.Fastest(),
                new EarliestArrivalSettings()
                {
                    MaxSecondsSource = 1500,
                    MaxSecondsTarget = 1500
                });
            route = transitRouter.TryEarliestArrival(new DateTime(2017, 11, 08, 18, 00, 00),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("antwerp-central")),
                Vehicle.Pedestrian.Fastest(),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("wechelderzande")),
                Vehicle.Pedestrian.Fastest(),
                new EarliestArrivalSettings()
                {
                    MaxSecondsSource = 1500,
                    MaxSecondsTarget = 1500
                });
            route = transitRouter.TryEarliestArrival(new DateTime(2017, 11, 18, 18, 00, 00),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("antwerp-central")),
                Vehicle.Pedestrian.Fastest(),
                transitRouter.Router.Resolve(Vehicle.Pedestrian.Fastest(), Locations.GetLocation("brugge")),
                Vehicle.Pedestrian.Fastest(),
                new EarliestArrivalSettings()
                {
                    MaxSecondsSource = 1500,
                    MaxSecondsTarget = 1500
                });

            //// run tests.
            //var route = Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test1.geojson");
            ////route = Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test2.geojson");
            //route = Runner.Test(transitRouter, "Itinero.Transit.Test.Functional.test_data.belgium.test3.geojson");

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static string ToGeoJson(IEnumerable<Coordinate> shape)
        {
            var lineString = new LineString(shape.ToArray().ToCoordinates().ToArray());
            var feature = new Feature(lineString, new AttributesTable());
            var features = new FeatureCollection();
            features.Add(feature);

            return ToJson(features);
        }

        private static string ToJson(FeatureCollection featureCollection)
        {
            var jsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
            var jsonStream = new StringWriter();
            jsonSerializer.Serialize(jsonStream, featureCollection);
            var json = jsonStream.ToInvariantString();
            return json;
        }
    }
}