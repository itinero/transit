using NUnit.Framework;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Test.Functional.Tests
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Executes the given test on the given router.
        /// </summary>
        public static void Test(MultimodalRouter router, FeatureCollection test)
        {
            var source = test.FirstOrException(x => x.Attributes.ContainsKeyValue("type", "source"), "Invalid test data: no source found.");
            var target = test.FirstOrException(x => x.Attributes.ContainsKeyValue("type", "target"), "Invalid test data: no target found");

            var sourceLocation = (source.Geometry as Point).Coordinate;
            var targetLocation = (target.Geometry as Point).Coordinate;

            var sourceProfile = OsmSharp.Routing.Profiles.Profile.Get(source.Attributes.FirstOrException(x => x.Key == "profile", 
                "Invalid test data: no vehicle profile found on source.").Value.ToInvariantString());
            var targetProfile = OsmSharp.Routing.Profiles.Profile.Get(target.Attributes.FirstOrException(x => x.Key == "profile",
                "Invalid test data: no vehicle profile found on target.").Value.ToInvariantString());

            var resolvedSource = router.Resolve(sourceProfile, sourceLocation);
            var resolvedTarget = router.Resolve(targetProfile, targetLocation);

            var result = test.First(x => x.Attributes.ContainsKeyValue("type", "result"));
            DateTime time;
            if (result.Attributes.ContainsKey("departuretime") &&
                DateTime.TryParseExact(result.Attributes.First(x => x.Key == "departuretime").Value.ToInvariantString(),
                    "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out time))
            {
                var route = router.TryEarliestArrival(time, resolvedSource, sourceProfile, resolvedTarget, targetProfile, EarliestArrivalSettings.Default);

                Assert.IsFalse(route.IsError, "Route was not found.");

                object value;
                if (result.Attributes.TryGetValue("time", out value))
                {
                    var timeResult = (long)value;
                    Assert.AreEqual(timeResult, route.Value.TotalTime, Settings.MinimumTotalTimeDifference);
                }
                File.WriteAllText("temp.geojson", route.Value.ToGeoJson());
            }
            else
            {
                throw new Exception("Invalid test data: no departure time set.");
            }
        }

        /// <summary>
        /// Executes the given test on the given router.
        /// </summary>
        public static void Test(MultimodalRouter router, string embeddedResourceId)
        {
            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                featureCollection = OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToFeatureCollection(stream.ReadToEnd());
            }

            Test(router, featureCollection);
        }
    }
}
