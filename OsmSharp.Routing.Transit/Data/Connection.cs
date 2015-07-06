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

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a transit-connection between two adjacent stops. A 1-hop connection.
    /// </summary>
    public struct Connection
    {
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

        /// <summary>
        /// The trip index, to sort connections with an identical arrival/departure time. 
        /// </summary>
        public int TripIdx { get; set; }

        /// <summary>
        /// The route.
        /// </summary>
        public int RouteId { get; set; }

        /// <summary>
        /// Returns a string representing this connection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}@{1} -> {2}@{3} on {4}",
                this.DepartureStop, this.DepartureTime,
                this.ArrivalStop, this.ArrivalTime, this.TripId);
        }
    }
}
