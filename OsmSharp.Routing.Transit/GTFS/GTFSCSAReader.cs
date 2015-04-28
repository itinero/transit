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
using OsmSharp.Routing.Transit.RouteCalculators.CSA;
using System;
using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.GTFS
{
    /// <summary>
    /// 
    /// </summary>
    public static class GTFSCSAReader
    {
        /// <summary>
        /// Reads and converts a GTFS feed into a CSA database.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public static CSADb CreateCSADb(IGTFSFeed feed)
        {
            var db = new CSADb();
            GTFSCSAReader.AddGTFSToCSADb(db, feed);
            return db;
        }

        /// <summary>
        /// Reads and converts GTFS feeds int a CSA database.
        /// </summary>
        /// <param name="feeds"></param>
        /// <returns></returns>
        public static CSADb CreateCSADb(IEnumerable<IGTFSFeed> feeds)
        {
            var db = new CSADb();
            foreach(var feed in feeds)
            {
                GTFSCSAReader.AddGTFSToCSADb(db, feed);
            }
            return db;
        }

        /// <summary>
        /// Adds all information in the given feed to the given CSA db.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="feed"></param>
        private static void AddGTFSToCSADb(CSADb db, IGTFSFeed feed)
        {
            var feedId = db.FeedDb.AddFeed(feed);

            var stops = new Dictionary<string, int>();
            foreach(var stop in feed.GetStops())
            {
                stops.Add(stop.Id, stops.Count);
            }

            var trips = new Dictionary<string, int>();
            foreach(var trip in feed.GetTrips())
            {
                trips.Add(trip.Id, trips.Count);
            }

            // build all connections by looping over all stoptimes.
            var stopTimesEnumerator = feed.GetStopTimes().GetEnumerator();
            stopTimesEnumerator.MoveNext();
            var previousStopTime = stopTimesEnumerator.Current;
            while (stopTimesEnumerator.MoveNext())
            {
                var stopTime = stopTimesEnumerator.Current;

                // check if two stop times belong to the same trip.
                if (previousStopTime.TripId == stopTime.TripId)
                { // we have two stops in the same trip
                    int tripId = trips[previousStopTime.TripId];

                    // parse arrival/departure.
                    var arrival = stopTime.ArrivalTime.TotalSeconds;
                    var departure = previousStopTime.DepartureTime.TotalSeconds;

                    // arrival/departure stops.
                    var arrivalStop = stops[stopTime.StopId];
                    var departureStop = stops[previousStopTime.StopId];

                    // build connections.
                    var connection = new CSAConnection();
                    connection.ArrivalStop = arrivalStop;
                    connection.ArrivalTime = arrival;
                    connection.DepartureStop = departureStop;
                    connection.DepartureTime = departure;
                    connection.FeedId = feedId;
                    connection.TripId = tripId;

                    db.Connections.Add(connection);
                }
                previousStopTime = stopTime;
            }
        }
    }
}
