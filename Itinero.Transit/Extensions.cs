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

using Itinero.Attributes;
using Itinero.Profiles;
using Itinero.Transit.Data;
using System;
using System.Collections.Generic;

namespace Itinero.Transit
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

        /// <summary>
        /// Represents a node in a linked-list.
        /// </summary>
        public class LinkedListNode<T>
        {
            /// <summary>
            /// Creates a new linked-list node.
            /// </summary>
            public LinkedListNode()
            {

            }

            /// <summary>
            /// Creates a new linked-list node.
            /// </summary>
            public LinkedListNode(T value)
            {
                this.Value = value;
            }

            /// <summary>
            /// Gets or sets the next node.
            /// </summary>
            public LinkedListNode<T> Next { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public T Value { get; set; }
        }

        /// <summary>
        /// Adds attributes to this attribute collection and adds a prefix to all keys.
        /// </summary>
        public static void AddOrReplaceWithPrefix(this IAttributeCollection attributes, string prefix, IEnumerable<Attributes.Attribute> toPrefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) { throw new ArgumentException("Not a valid prefix."); }

            if (toPrefix == null)
            {
                return;
            }

            foreach (var attribute in toPrefix)
            {
                attributes.AddOrReplace(prefix + attribute.Key, attribute.Value);
            }
        }
    }
}