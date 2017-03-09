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

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Contains extension methods for the trips db.
    /// </summary>
    public static class TripsDbExtensions
    {
        /// <summary>
        /// Searches all trips.
        /// </summary>
        public static HashSet<uint> SearchAllTrips(this TransitDb transitDb, Func<IAttributeCollection, bool> filter)
        {
            return transitDb.TripsDb.SearchAll(transitDb.TripAttributes, filter);
        }

        /// <summary>
        /// Searches all trips.
        /// </summary>
        public static HashSet<uint> SearchAll(this TripsDb tripsDb, AttributesIndex tripAttributes, Func<IAttributeCollection, bool> filter)
        {
            var ids = new HashSet<uint>();

            var enumerator = tripsDb.GetEnumerator();
            while(enumerator.MoveUntil(tripAttributes, filter))
            {
                ids.Add(enumerator.Id);
            }

            return ids;
        }

        /// <summary>
        /// Moves the enumerator until the trip that satisfies the filter.
        /// </summary>
        public static bool MoveUntil(this TripsDb.Enumerator enumerator, AttributesIndex tripAttributes, Func<IAttributeCollection, bool> filter)
        {
            if (enumerator == null) { throw new ArgumentNullException(nameof(enumerator)); }
            if (filter == null) { throw new ArgumentNullException(nameof(filter)); }

            while(enumerator.MoveNext())
            {
                var meta = tripAttributes.Get(enumerator.MetaId);

                if (filter(meta))
                {
                    return true;
                }
            }
            return false;
        }
    }
}