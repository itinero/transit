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

using GTFS;
using GTFS.Entities;

namespace Itinero.Transit.Test.GTFS
{ 
    /// <summary>
    /// Builds dummy GTFS feeds.
    /// </summary>
    public static class DummyGTFSFeedBuilder
    {
        /// <summary>
        /// Builds an empty GTFS feed.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed Empty()
        {
            return new GTFSFeed();
        }

        /// <summary>
        /// Builds a GTFS feed with just one connection.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed OneConnection(TimeOfDay departureTime, TimeOfDay arrivalTime)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime,
                StopSequence = 0
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime,
                StopSequence = 1
            });
            feed.Calendars.Add(new Calendar()
                {
                    StartDate = new System.DateTime(2015, 01, 01),
                    EndDate = new System.DateTime(2015, 12, 31),
                    Mask = 127,
                    ServiceId = "0"
                });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with two connections on the same trip.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed TwoConnectionsOneTrip(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = departureTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "0",
                DepartureTime = arrivalTime2
            });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with two connections on two different trips but sharing one transfer stop.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed TwoConnectionsTwoTrips(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0",
                ServiceId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "1",
                RouteId = "1",
                ServiceId = "0"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1,
                StopSequence = 0
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime1,
                StopSequence = 1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime2,
                StopId = "1",
                TripId = "1",
                DepartureTime = departureTime2,
                StopSequence = 0
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "1",
                DepartureTime = arrivalTime2,
                StopSequence = 1
            });
            feed.Calendars.Add(new Calendar()
            {
                StartDate = new System.DateTime(2015, 01, 01),
                EndDate = new System.DateTime(2015, 12, 31),
                Mask = 127,
                ServiceId = "0"
            });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with two connections on two different trips, sharing no transfer stop but stops are close together geographically.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed TwoConnectionsTwoTripsCloseStops(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "1",
                RouteId = "1"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "10",
                Longitude = 1.0001,
                Latitude = 1.0001 // approx 15m.
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime2,
                StopId = "10",
                TripId = "1",
                DepartureTime = departureTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "1",
                DepartureTime = arrivalTime2
            });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with two connections on two different trips, sharing no transfer stop but stops are close together geographically. 
        /// The 'middle' stop has two outgoing connections with one 5 minutes later.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed ThreeConnectionsThreeTripsCloseStops(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "2",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "1",
                RouteId = "1"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "2",
                RouteId = "2"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "10",
                Longitude = 1.0001,
                Latitude = 1.0001 // approx 15m.
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime2,
                StopId = "10",
                TripId = "1",
                DepartureTime = departureTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "1",
                DepartureTime = arrivalTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = TimeOfDay.FromTotalSeconds(departureTime2.TotalSeconds + 60 * 5),
                StopId = "10",
                TripId = "2",
                DepartureTime = TimeOfDay.FromTotalSeconds(departureTime2.TotalSeconds + 60 * 5)
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = TimeOfDay.FromTotalSeconds(arrivalTime2.TotalSeconds + 60 * 5),
                StopId = "2",
                TripId = "2",
                DepartureTime = TimeOfDay.FromTotalSeconds(arrivalTime2.TotalSeconds + 60 * 5)
            });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with three connections on three different trips. Two of them share a transfers stop and one is a direct connection but with the same arrival time.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed ThreeConnectionsThreeTripsTransferVSNoTranfer(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2, TimeOfDay departureTime3, TimeOfDay arrivalTime3)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "2",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "1",
                RouteId = "1"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "2",
                RouteId = "2"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime2,
                StopId = "1",
                TripId = "1",
                DepartureTime = departureTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "1",
                DepartureTime = arrivalTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime3,
                StopId = "0",
                TripId = "2",
                DepartureTime = departureTime3
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime3,
                StopId = "2",
                TripId = "2",
                DepartureTime = arrivalTime3
            });
            return feed;
        }

        /// <summary>
        /// Builds a GTFS feed with three connections on three different trips. Two of them share a transfers stop and one is a direct trip, also stopping at the middel stop,
        /// but with the same arrival time.
        /// </summary>
        /// <returns></returns>
        public static IGTFSFeed ThreeConnectionsThreeTripsTransferVSNoTranferWithStop(TimeOfDay departureTime1, TimeOfDay arrivalTime1,
            TimeOfDay departureTime2, TimeOfDay arrivalTime2, TimeOfDay departureTime3, TimeOfDay arrivalTime3, TimeOfDay departureTime4, TimeOfDay arrivalTime4)
        {
            var feed = new GTFSFeed();
            feed.Agencies.Add(new Agency()
            {
                Id = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new global::GTFS.Entities.Route()
            {
                Id = "2",
                AgencyId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "0",
                RouteId = "0"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "1",
                RouteId = "1"
            });
            feed.Trips.Add(new Trip()
            {
                Id = "2",
                RouteId = "2"
            });
            feed.Stops.Add(new Stop()
            {
                Id = "0",
                Longitude = 0,
                Latitude = 0
            });
            feed.Stops.Add(new Stop()
            {
                Id = "1",
                Longitude = 1,
                Latitude = 1
            });
            feed.Stops.Add(new Stop()
            {
                Id = "2",
                Longitude = 2,
                Latitude = 2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime1,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime1,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime1
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime2,
                StopId = "1",
                TripId = "1",
                DepartureTime = departureTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime2,
                StopId = "2",
                TripId = "1",
                DepartureTime = arrivalTime2
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime3,
                StopId = "0",
                TripId = "2",
                DepartureTime = departureTime3
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime3,
                StopId = "1",
                TripId = "2",
                DepartureTime = arrivalTime3
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime4,
                StopId = "2",
                TripId = "2",
                DepartureTime = arrivalTime4
            });
            return feed;
        }
    }
}