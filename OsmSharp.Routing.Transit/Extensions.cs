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

using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Contains extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the n elements with the lowest cost.
        /// </summary>
        public static List<T> GetLowestN<T>(this IEnumerable<T> elements, int n, Func<T, float> cost)
        {
            var lowest = new List<T>(n);
            var lowestMax = float.MinValue;
            var lowestMaxIdx = -1;
            foreach (var element in elements)
            {
                if (lowest.Count < n)
                { // add a new lowest.
                    lowest.Add(element);

                    // check for a new low.
                    var newLow = cost(element);
                    if (newLow > lowestMax)
                    { // ok a new low.
                        lowestMax = newLow;
                        lowestMaxIdx = lowest.Count - 1;
                    }
                }
                else
                { // check for a new low.
                    var newLow = cost(element);
                    if (newLow < lowestMax)
                    { // ok, this one is lower than the maximum in the current lowest collection.
                        lowest.RemoveAt(lowestMaxIdx);
                        lowest.Add(element);
                        lowestMax = float.MinValue;
                        lowestMaxIdx = -1;

                        // update lowestMax.
                        for (var i = 0; i < lowest.Count; i++)
                        {
                            newLow = cost(lowest[i]);
                            if (newLow > lowestMax)
                            { // ok a new low.
                                lowestMax = newLow;
                                lowestMaxIdx = i;
                            }
                        }
                    }
                }
            }
            return lowest;
        }
    }
}