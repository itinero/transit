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

using Itinero.Geo;
using Itinero.Profiles;
using Itinero.Transit.Data;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace Itinero.Transit.Geo
{
    public static class Extensions
    {
        /// <summary>
        /// Gets a feature collection with features representing the stop links.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetStopLinks(this MultimodalDb db,
            Profile profile, uint stopId)
        {
            var features = new FeatureCollection();

            var stopEnumerator = db.TransitDb.GetStopsEnumerator();
            if (stopEnumerator.MoveTo(stopId))
            {
                var stopLocation = new GeoAPI.Geometries.Coordinate(stopEnumerator.Longitude, stopEnumerator.Latitude);
                var attributes = new AttributesTable();
                attributes.AddAttribute("stop_id", stopId);
                features.Add(new Feature( new Point(stopLocation), attributes));

                var stopLinksDb = db.GetStopLinksDb(profile).GetEnumerator();
                stopLinksDb.MoveTo(stopId);
                while (stopLinksDb.MoveNext())
                {
                    var routerPoint = new RouterPoint(0, 0, stopLinksDb.EdgeId, stopLinksDb.Offset);
                    var linkLocation = routerPoint.LocationOnNetwork(db.RouterDb).ToCoordinate();

                    attributes = new AttributesTable();
                    attributes.AddAttribute("edge_id", stopLinksDb.EdgeId.ToInvariantString());
                    attributes.AddAttribute("offset", stopLinksDb.Offset.ToInvariantString());
                    features.Add(new Feature(new Point(linkLocation), attributes));
                    features.Add(new Feature(new LineString(new GeoAPI.Geometries.Coordinate[] { stopLocation, linkLocation }), new AttributesTable()));
                }
            }
            return features;
        }
    }
}
