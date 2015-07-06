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

using GTFS;
using GTFS.Entities;

namespace OsmSharp.Transit.Test.Data.GTFS
{
    /// <summary>
    /// Builds dummy GTFS feeds.
    /// </summary>
    public static class GTFSConnectionsDbBuilder
    {
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
            feed.Routes.Add(new Route()
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
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = departureTime,
                StopId = "0",
                TripId = "0",
                DepartureTime = departureTime
            });
            feed.StopTimes.Add(new StopTime()
            {
                ArrivalTime = arrivalTime,
                StopId = "1",
                TripId = "0",
                DepartureTime = arrivalTime
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
            feed.Routes.Add(new Route()
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
            feed.Routes.Add(new Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
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
            feed.Routes.Add(new Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
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
            feed.Routes.Add(new Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
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
            feed.Routes.Add(new Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
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
            feed.Routes.Add(new Route()
            {
                Id = "0",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
            {
                Id = "1",
                AgencyId = "0"
            });
            feed.Routes.Add(new Route()
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