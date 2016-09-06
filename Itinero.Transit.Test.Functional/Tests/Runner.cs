using NUnit.Framework;
using Itinero;
using Itinero.Geo;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Itinero.Transit.Test.Functional.Tests
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
            var source = test.Features.FirstOrException(x => 
                x.Attributes.Contains("type", "source"), "Invalid test data: no source found.");
            var target = test.Features.FirstOrException(x => 
                x.Attributes.Contains("type", "target"), "Invalid test data: no target found");

            var sourceLocation = (source.Geometry as Point).Coordinate;
            var targetLocation = (target.Geometry as Point).Coordinate;

            var sourceProfile = Itinero.Profiles.Profile.Get(source.Attributes.FirstOrException(x => x == "profile", 
                "Invalid test data: no vehicle profile found on source.").ToInvariantString());
            var targetProfile = Itinero.Profiles.Profile.Get(target.Attributes.FirstOrException(x => x == "profile",
                "Invalid test data: no vehicle profile found on target.").ToInvariantString());

            var resolvedSource = router.Router.Resolve(sourceProfile, sourceLocation);
            var resolvedTarget = router.Router.Resolve(targetProfile, targetLocation);

            var result = test.Features.First(x => x.Attributes.Contains("type", "result"));
            var name = result.Attributes.FirstOrException(x => x == "name", "Name of test case not found, expected on result geometry.").ToInvariantString();
            Contract.Requires(name != null, "Name of test case not set.");

            var performanceInfoConsumer = new PerformanceInfoConsumer(name, 5000);

            var time = DateTime.Now;
            if (result.Attributes.GetNames().Contains("departuretime") &&
                DateTime.TryParseExact(result.Attributes.FirstOrException(x => x == "departuretime", string.Empty).ToInvariantString(),
                    "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out time))
            {
                performanceInfoConsumer.Start();
                var route = router.TryEarliestArrival(time, resolvedSource, sourceProfile, resolvedTarget, targetProfile, EarliestArrivalSettings.Default);

                Assert.IsFalse(route.IsError, "Route was not found.");

                object value;
                if (result.Attributes.TryGetValue("time", out value))
                {
                    var timeResult = (long)value;
                    Assert.AreEqual(timeResult, route.Value.TotalTime, Settings.MinimumTotalTimeDifference);
                }
                performanceInfoConsumer.Stop();
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
                var jsonReader = new JsonTextReader(stream);
                var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
                featureCollection = geoJsonSerializer.Deserialize(jsonReader) as FeatureCollection;
            }

            Test(router, featureCollection);
        }
    }
}
