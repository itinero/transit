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

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Holds common constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Holds the amount of seconds in a day.
        /// </summary>
        public const int OneDayInSeconds = 24 * 60 * 60;

        /// <summary>
        /// Holds the trip id a connection gets when it's just a pseudo-connection.
        /// </summary>
        public const uint TransferConnectionId = uint.MaxValue - 1;

        /// <summary>
        /// Holds the trip id in case there is no information available.
        /// </summary>
        public const int NoTripId = -2;

        /// <summary>
        /// Holds the connection id to set if there is no information available.
        /// </summary>
        public const uint NoConnectionId = uint.MaxValue;

        /// <summary>
        /// Holds the seconds to set if no seconds information available.
        /// </summary>
        public const uint NoSeconds = uint.MaxValue;

        /// <summary>
        /// Holds the route id in case there is no information available.
        /// </summary>
        public const int NoRouteId = -1;

        /// <summary>
        /// Holds the value to use as a vehicle profile when a segment in a route represents waiting time.
        /// </summary>
        public static string WaitProfile = "Transit.Wait";

        /// <summary>
        /// Holds the value to use as a vehicle profile when a segment in a route represents a transfer from one trip to another.
        /// </summary>
        public static string TransferProfile = "Transit.Transfer";

        /// <summary>
        /// Holds the value to use for a stop id when no information is available.
        /// </summary>
        public const uint NoStopId = uint.MaxValue;

        /// <summary>
        /// Holds the value to use for transfers when no information is available.
        /// </summary>
        public const int NoTransfers = -1;
    }
}