using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Transit.Test.Functional.Staging
{
    public static class Locations
    {
        private static Dictionary<string, Coordinate> _locations;

        /// <summary>
        /// Gets a test location by it's id from the embedded geojson.
        /// </summary>
        public static Coordinate GetLocation(string id)
        {
            if (_locations == null)
            {
                var locations = new Dictionary<string, Coordinate>();
                var features = Data.GetFeatureCollection("Itinero.Transit.Test.Functional.test_data.locations.geojson");
                foreach (var feature in features.Features)
                {
                    var point = feature.Geometry as NetTopologySuite.Geometries.Point;
                    if (point == null)
                    {
                        continue;
                    }

                    object val;
                    if (feature.Attributes.TryGetValue("id", out val))
                    {
                        locations[val.ToInvariantString()] = new Coordinate(
                            (float)point.Coordinate.Y, (float)point.Coordinate.X);
                    }
                }
                _locations = locations;
            }

            return _locations[id];
        }
    }
}