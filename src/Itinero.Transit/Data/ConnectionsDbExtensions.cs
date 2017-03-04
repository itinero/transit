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

using System;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Contains extension methods related to the connections db.
    /// </summary>
    public static class ConnectionsDbExtensions
    {
        /// <summary>
        /// Moves the enumerator that is assumed to be sorted by departure time to the first connection with a departure time larger than or equal to the given departure time.
        /// </summary>
        public static bool MoveToDepartureTime(this ConnectionsDb.Enumerator enumerator,
            uint departureTime)
        {
            if (enumerator == null) { throw new ArgumentNullException("enumerator"); }

            if (enumerator.Count == 0)
            { // not possible to search and there's no need.
                return false;
            }

            // do a binary search.
            uint lower = 0;
            uint upper = enumerator.Count - 1;

            // check bound.
            enumerator.MoveTo(upper);
            if (enumerator.DepartureTime < departureTime)
            { // no connection with a departure time after the given departure time.
                return false;
            }

            // check lowest.
            enumerator.MoveTo(lower);
            if (enumerator.DepartureTime >= departureTime)
            { // the first connection is already the one with a departure time after the given departure time.
                return true;
            }

            while (upper - lower > 1)
            {
                var position = (uint)((lower + upper) / 2);
                enumerator.MoveTo(position);
                if (enumerator.DepartureTime >= departureTime)
                { // position >= than target position.
                    upper = position;
                }
                else
                { // position > than target position.
                    lower = position;
                }
            }

            enumerator.MoveTo(upper);
            return true;
        }

        /// <summary>
        /// Moves the given enumerator to the connection right before the current connection on the same trip.
        /// </summary>
        /// <remarks>This requires the connections to be sorted by departure time.</remarks>
        /// <returns></returns>
        public static bool MoveToPreviousConnection(this ConnectionsDb.Enumerator enumerator)
        {
            if (enumerator == null) { throw new ArgumentNullException("enumerator"); }

            if (enumerator.Count == 0)
            { // not possible to search and there's no need.
                return false;
            }

            var trip = enumerator.TripId;
            while (enumerator.MovePrevious())
            {
                if (enumerator.TripId == trip)
                {
                    return true;
                }
            }
            return false;
        }
    }
}