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

using Itinero.Attributes;
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