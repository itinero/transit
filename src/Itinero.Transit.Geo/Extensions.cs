// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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
