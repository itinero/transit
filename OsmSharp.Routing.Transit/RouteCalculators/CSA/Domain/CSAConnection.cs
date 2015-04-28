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

namespace OsmSharp.Routing.Transit.RouteCalculators.CSA
{
    /// <summary>
    /// Represents a transit-connection.
    /// </summary>
    public struct CSAConnection
    {
        /// <summary>
        /// The GTFS feed id.
        /// </summary>
        public int FeedId { get; set; }

        /// <summary>
        /// The departure stop.
        /// </summary>
        public int DepartureStop { get; set; }

        /// <summary>
        /// The departure time.
        /// </summary>
        public int DepartureTime { get; set; }
        
        /// <summary>
        /// The arrival stop.
        /// </summary>
        public int ArrivalStop { get; set; }

        /// <summary>
        /// The arrival time.
        /// </summary>
        public int ArrivalTime { get; set; }

        /// <summary>
        /// The trip.
        /// </summary>
        public int TripId { get; set; }
    }
}