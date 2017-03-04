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

using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Contains extension methods for the multimodal db.
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
            var stopsDbEnumerator = db.TransitDb.GetStopsEnumerator();
            var linksDb = new StopLinksDb(stopsDbEnumerator.Count, db.RouterDb, profile);

            while (stopsDbEnumerator.MoveNext())
            {
                var stopId = stopsDbEnumerator.Id;
                var multiResolver = new ResolveMultipleAlgorithm(db.RouterDb.Network.GeometricGraph,
                    stopsDbEnumerator.Latitude, stopsDbEnumerator.Longitude, searchOffset, maxDistance, (edge) =>
                    {
                        // get profile.
                        float distance;
                        ushort edgeProfileId;
                        Itinero.Data.Edges.EdgeDataSerializer.Deserialize(edge.Data[0],
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
                        (p) => Coordinate.DistanceEstimateInMeter(stopsDbEnumerator.Latitude, stopsDbEnumerator.Longitude,
                            p.Latitude, p.Longitude));

                    // add them as new links.
                    for (var i = 0; i < closest.Count; i++)
                    {
                        linksDb.Add((uint)stopId, closest[i]);
                    }
                }
            }

            db.AddStopLinksDb(linksDb);
        }
    }
}
