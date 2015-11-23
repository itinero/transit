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

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Contains extension methods related to the connections db.
    /// </summary>
    public static class ConnectionsDbExtensions
    {
        /// <summary>
        /// Moves the enumerator that is assumed to be sorted by departure time to the first connection with a departure time larger than or equal to the given departure time.
        /// </summary>
        public static bool MoveToDepartureTime(this ConnectionsDb.ConnectionEnumerator enumerator,
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
    }
}