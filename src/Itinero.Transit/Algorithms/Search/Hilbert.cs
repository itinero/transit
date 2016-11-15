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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Transit.Data;
using System;
using System.Collections.Generic;

namespace Itinero.Transit.Algorithms.Search
{
    /// <summary>
    /// Hilbert sorting/search helper functions.
    /// </summary>
    public static class Hilbert
    {
        /// <summary>
        /// Searches for the closest stop.
        /// </summary>
        /// <returns></returns>
        public static uint SearchClosest(this StopsDb.Enumerator stopsDbEnumerator, float latitude, float longitude,
            float offset)
        {
            // search for all nearby stops.
            var stops = Hilbert.Search(stopsDbEnumerator, latitude, longitude, offset);

            var bestDistance = double.MaxValue;
            var bestVertex = Constants.NoStopId;
            foreach (var stop in stops)
            {
                if (stopsDbEnumerator.MoveTo(stop))
                {
                    var lat = stopsDbEnumerator.Latitude;
                    var lon = stopsDbEnumerator.Longitude;
                    var distance = Coordinate.DistanceEstimateInMeter(latitude, longitude, lat, lon);
                    if (distance < bestDistance)
                    { // a new closest vertex found.
                        bestDistance = distance;
                        bestVertex = stop;
                    }
                }
            }
            return bestVertex;
        }

        /// <summary>
        /// Searches the stops db for nearby stops assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this StopsDb.Enumerator stopsDbEnumerator, float latitude, float longitude,
            float offset)
        {
            return Hilbert.Search(stopsDbEnumerator, Itinero.Algorithms.Search.Hilbert.HilbertExtensions.DefaultHilbertSteps, latitude - offset, longitude - offset,
                latitude + offset, longitude + offset);
        }

        /// <summary>
        /// Searches the stops db assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this StopsDb.Enumerator stopsDbEnumerator, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            return stopsDbEnumerator.Search(Itinero.Algorithms.Search.Hilbert.HilbertExtensions.DefaultHilbertSteps, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
        }

        /// <summary>
        /// Searches the stops db assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this StopsDb.Enumerator stopsDbEnumerator, int n, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var targets = Itinero.Algorithms.Search.Hilbert.HilbertCurve.HilbertDistances(
             System.Math.Max(minLatitude, -90),
             System.Math.Max(minLongitude, -180),
             System.Math.Min(maxLatitude, 90),
             System.Math.Min(maxLongitude, 180), n);
            targets.Sort();

            var stops = new HashSet<uint>();
            var targetIdx = 0;
            var stop1 = (uint)0;
            var stop2 = (uint)stopsDbEnumerator.Count - 1;
            while (targetIdx < targets.Count)
            {
                uint stop;
                int count;
                if (Hilbert.Search(stopsDbEnumerator, targets[targetIdx], n, stop1, stop2, out stop, out count))
                { // the search was successful.
                    while (count > 0)
                    { // there have been stops found.
                        if (stopsDbEnumerator.MoveTo((uint)stop + (uint)(count - 1)))
                        { // the stop was found.
                            var vertexLat = stopsDbEnumerator.Latitude;
                            var vertexLon = stopsDbEnumerator.Longitude;
                            if (minLatitude < vertexLat &&
                                minLongitude < vertexLon &&
                                maxLatitude > vertexLat &&
                                maxLongitude > vertexLon)
                            { // within offset.
                                stops.Add((uint)stop + (uint)(count - 1));
                            }
                        }
                        count--;
                    }

                    // update stop1.
                    stop1 = stop;
                }

                // move to next target.
                targetIdx++;
            }
            return stops;
        }

        /// <summary>
        /// Searches the stops db assuming it has been sorted.
        /// </summary>
        public static bool Search(this StopsDb.Enumerator stopsDbEnumerator, long hilbert, int n, uint stop1, uint stop2, 
            out uint vertex, out int count)
        {
            var hilbert1 = Hilbert.Distance(stopsDbEnumerator, n, stop1);
            var hilbert2 = Hilbert.Distance(stopsDbEnumerator, n, stop2);
            while (stop1 <= stop2)
            {
                // check the current hilbert distances.
                if (hilbert1 > hilbert2)
                { // situation is impossible and probably the stops are not sorted.
                    throw new Exception("Stops not sorted: Binary search using hilbert distance not possible.");
                }
                if (hilbert1 == hilbert)
                { // found at hilbert1.
                    var lower = stop1;
                    while (hilbert1 == hilbert)
                    {
                        if (lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        lower--;
                        hilbert1 = Hilbert.Distance(stopsDbEnumerator, n, lower);
                    }
                    var upper = stop1;
                    hilbert1 = Hilbert.Distance(stopsDbEnumerator, n, upper);
                    while (hilbert1 == hilbert)
                    {
                        if (upper >= stopsDbEnumerator.Count - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert1 = Hilbert.Distance(stopsDbEnumerator, n, upper);
                    }
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if (hilbert2 == hilbert)
                { // found at hilbert2.
                    var lower = stop2;
                    while (hilbert2 == hilbert)
                    {
                        if (lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        lower--;
                        hilbert2 = Hilbert.Distance(stopsDbEnumerator, n, lower);
                    }
                    var upper = stop2;
                    hilbert2 = Hilbert.Distance(stopsDbEnumerator, n, upper);
                    while (hilbert2 == hilbert)
                    {
                        if (upper >= stopsDbEnumerator.Count - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert2 = Hilbert.Distance(stopsDbEnumerator, n, upper);
                    }
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if (hilbert1 == hilbert2 ||
                    stop1 == stop2 ||
                    stop1 == stop2 - 1)
                { // search is finished.
                    vertex = stop1;
                    count = 0;
                    return true;
                }

                // Binary search: calculate hilbert distance of the middle.
                var vertexMiddle = stop1 + (uint)((stop2 - stop1) / 2);
                var hilbertMiddle = Hilbert.Distance(stopsDbEnumerator, n, vertexMiddle);
                if (hilbert <= hilbertMiddle)
                { // target is in first part.
                    stop2 = vertexMiddle;
                    hilbert2 = hilbertMiddle;
                }
                else
                { // target is in the second part.
                    stop1 = vertexMiddle;
                    hilbert1 = hilbertMiddle;
                }
            }
            vertex = stop1;
            count = 0;
            return false;
        }

        /// <summary>
        /// Returns the hibert distance for n and the given stop.
        /// </summary>
        /// <returns></returns>
        public static long Distance(this StopsDb.Enumerator stopsDbEnumerator, int n, uint stop)
        {
            if (!stopsDbEnumerator.MoveTo(stop))
            {
                throw new Exception(string.Format("Cannot calculate hilbert distance, stop {0} does not exist.",
                    stop));
            }
            return HilbertCurve.HilbertDistance(stopsDbEnumerator.Latitude, stopsDbEnumerator.Longitude, n);
        }
    }
}