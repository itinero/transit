// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Profiles;

namespace OsmSharp.Routing.Transit.Multimodal.Data
{
    /// <summary>
    /// Contains extension method for the multimodal db.
    /// </summary>
    public static class MultimodalDbExtensions
    {
        /// <summary>
        /// The default search offset when linking stops.
        /// </summary>
        public const float DefaultSearchOffset = .01f;

        /// <summary>
        /// The default max distance when linking stops.
        /// </summary>
        public const float DefaultMaxDistance = 50;

        /// <summary>
        /// The default number of closest edges to use when linking stops.
        /// </summary>
        public const int DefaultMaxRouterPoints = 3;

        /// <summary>
        /// Adds a new stop links db for the given profile.
        /// </summary>
        public static void AddStopLinksDb(this MultimodalDb db, Profile profile, float searchOffset = DefaultSearchOffset,
            float maxDistance = DefaultMaxDistance, int maxRouterPoints = DefaultMaxRouterPoints)
        {
            var stops = db.ConnectionsDb.GetStops();
            var linksDb = new StopLinksDb(stops.Count);

            for (int stopId = 0; stopId < stops.Count; stopId++)
            {
                var stop = stops[stopId];
                var multiResolver = new ResolveMultipleAlgorithm(db.RouterDb.Network.GeometricGraph,
                    stop.Latitude, stop.Longitude, searchOffset, maxDistance, (edge) =>
                    {
                        // get profile.
                        float distance;
                        ushort edgeProfileId;
                        OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0],
                            out distance, out edgeProfileId);
                        var edgeProfile = db.RouterDb.EdgeProfiles.Get(edgeProfileId);
                        // get factor from profile.
                        if (profile.Factor(edgeProfile).Value <= 0)
                        { // cannot be traversed by this profile.
                            return false;
                        }
                        // verify stoppable.
                        if (!profile.CanStopOn(edgeProfile))
                        { // this profile cannot stop on this edge.
                            return false;
                        }
                        return true;
                    });
                multiResolver.Run();
                if (multiResolver.HasSucceeded)
                {
                    // get the n-closest.
                    var closest = multiResolver.Results.GetLowestN(maxRouterPoints,
                        (p) => (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(stop.Latitude, stop.Longitude,
                            p.Latitude, p.Longitude));

                    // add them as new links.
                    for (var i = 0; i < closest.Count; i++)
                    {
                        linksDb.Add((uint)stopId, closest[i]);
                    }
                }
            }

            db.AddStopLinksDb(profile, linksDb);
        }
    }
}
