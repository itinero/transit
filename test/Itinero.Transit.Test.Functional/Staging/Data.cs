using NetTopologySuite.Features;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace Itinero.Transit.Test.Functional.Staging
{
    /// <summary>
    /// Contains general functionality to handle test-data.
    /// </summary>
    public static class Data
    {
        /// <summary>
        /// Gets a feature collection from an embedded geojson.
        /// </summary>
        public static FeatureCollection GetFeatureCollection(string embeddedResourceId)
        {
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                var jsonReader = new JsonTextReader(stream);
                var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
                return geoJsonSerializer.Deserialize<FeatureCollection>(jsonReader) as FeatureCollection;
            }
        }
    }
}
