// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using GTFS.Entities;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Represents a part of a transit route containing a trip and a number of stops on this trip.
    /// </summary>
    public class TransitRouteEntry
    {
        /// <summary>
        /// Gets or sets the trip.
        /// </summary>
        public Trip Trip { get; set; }

        /// <summary>
        /// Gets or sets the stop.
        /// </summary>
        public List<TransitRouteStop> Stops { get; set; }

        /// <summary>
        /// Returns a description of this entry.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} stops along {1}", this.Stops.Count, this.Trip);
        }
    }
}