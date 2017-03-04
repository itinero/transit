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