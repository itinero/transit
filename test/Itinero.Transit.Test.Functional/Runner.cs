// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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

namespace Itinero.Transit.Test.Functional
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Executes the given test on the given router.
        /// </summary>
        public static Route Test(MultimodalRouter router, FeatureCollection test)
        {
            var source = test.Features.FirstOrException(x => 
                x.Attributes.Contains("type", "source"), "Invalid test data: no source found.");
            var target = test.Features.FirstOrException(x => 
                x.Attributes.Contains("type", "target"), "Invalid test data: no target found");

            var sourceLocation = (source.Geometry as Point).Coordinate;
            var targetLocation = (target.Geometry as Point).Coordinate;

            var sourceProfile = Itinero.Profiles.Profile.GetRegistered(source.Attributes.FirstOrException(x => x == "profile", 
                "Invalid test data: no vehicle profile found on source.").ToInvariantString());
            var targetProfile = Itinero.Profiles.Profile.GetRegistered(target.Attributes.FirstOrException(x => x == "profile",
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

                //object value;
                //if (result.Attributes.TryGetValue("time", out value))
                //{
                //    var timeResult = (long)value;
                //    Assert.AreEqual(timeResult, route.Value.TotalTime, Settings.MinimumTotalTimeDifference);
                //}
                //performanceInfoConsumer.Stop();
                //File.WriteAllText("temp.geojson", route.Value.ToGeoJson());

                return route.Value;
            }
            else
            {
                throw new Exception("Invalid test data: no departure time set.");
            }
        }

        /// <summary>
        /// Executes the given test on the given router.
        /// </summary>
        public static Route Test(MultimodalRouter router, string embeddedResourceId)
        {
            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                var jsonReader = new JsonTextReader(stream);
                var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
                featureCollection = geoJsonSerializer.Deserialize<FeatureCollection>(jsonReader) as FeatureCollection;
            }

            return Test(router, featureCollection);
        }
    }
}
