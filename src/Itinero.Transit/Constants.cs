// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

namespace Itinero.Transit
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
        /// Holds the value to use as a vehicle profile when a segment in a route represents travelling on a transit vehicle.
        /// </summary>
        public static string VehicleProfile = "Transit.Vehicle";

        /// <summary>
        /// Holds the key to use when setting a time of day on a route.
        /// </summary>
        public static string TimeOfDayKey = "transit.timeofday";

        /// <summary>
        /// Holds the key to use when setting a duration on a route segment.
        /// </summary>
        public static string DurationKey = "duration";

        /// <summary>
        /// Holds the value to use for a stop id when no information is available.
        /// </summary>
        public const uint NoStopId = uint.MaxValue;

        /// <summary>
        /// Holds the value to use for transfers when no information is available.
        /// </summary>
        public const int NoTransfers = -1;

        /// <summary>
        /// A default search offset.
        /// </summary>
        public static float SearchOffsetInMeter = 7500;

        /// <summary>
        /// A maximum search distance.
        /// </summary>
        public const float SearchDistanceInMeter = 50;
    }
}