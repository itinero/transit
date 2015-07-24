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
        /// Returns true if the given object represents the same connection.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj is Connection)
            {
                return Connection.Equals(this, (Connection)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ArrivalStop.GetHashCode() ^
                this.ArrivalTime.GetHashCode() ^
                this.DepartureStop.GetHashCode() ^
                this.DepartureTime.GetHashCode() ^
                this.RouteId.GetHashCode() ^
                this.TripId.GetHashCode() ^
                this.TripIdx.GetHashCode();
        }

        /// <summary>
        /// Returns true when the two connections represent the same connection.
        /// </summary>
        /// <returns></returns>
        public static bool Equals(Connection connection1, Connection connection2)
        {
            return connection1.ArrivalStop == connection2.ArrivalStop &&
                connection1.ArrivalTime == connection2.ArrivalTime &&
                connection1.DepartureStop == connection2.DepartureStop &&
                connection1.DepartureTime == connection2.DepartureTime &&
                connection1.RouteId == connection2.RouteId &&
                connection1.TripId == connection2.TripId &&
                connection1.TripIdx == connection2.TripIdx;
        }

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
