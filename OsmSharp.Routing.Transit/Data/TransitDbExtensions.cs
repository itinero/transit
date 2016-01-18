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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Contains extension methods for the transit db.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Adds a transfers db.
        /// </summary>
        public static void AddTransfersDb(this TransitDb db, Profiles.Profile profile, TagsCollectionBase defaultProfile,
            float maxTimeInSeconds)
        {
            var transfersDb = new TransfersDb(db.StopsCount);
            var factor = profile.Factor(defaultProfile);

            // add all transfers.
            var enumerator1 = db.GetStopsEnumerator();
            while (enumerator1.MoveNext())
            {
                var enumerator2 = db.GetStopsEnumerator();
                while (enumerator2.MoveNext())
                {
                    if (enumerator1.Id < enumerator2.Id)
                    {
                        var distance = GeoCoordinate.DistanceEstimateInMeter(enumerator1.Latitude, enumerator1.Longitude,
                            enumerator2.Latitude, enumerator2.Longitude);
                        var time = (int)System.Math.Round(distance * factor.Value, 0);
                        if (time < maxTimeInSeconds)
                        {
                            transfersDb.AddTransfer(enumerator1.Id, enumerator2.Id, time);
                        }
                    }
                }
            }

            db.AddTransfersDb(profile, transfersDb);
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddScheduleEntry(this TransitDb db, uint id, DateTime day)
        {
            db.AddScheduleEntry(id, day, day, day.Weekmask());
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public static void AddScheduleEntry(this TransitDb db, uint id, DateTime start, DateTime end,
            params DayOfWeek[] days)
        {
            if (days == null || days.Length == 0) { throw new ArgumentOutOfRangeException("days", "Cannot add empty week patterns."); }

            db.AddScheduleEntry(id, start, end, SchedulesDbExtensions.Weekmask(days));
        }

        /// <summary>
        /// Searches for the first stop with some tags or based on some condition.
        /// </summary>
        public static uint SearchFirstStopsWithTags(this TransitDb db,
            Func<TagsCollectionBase, bool> condition)
        {
            var stops = new HashSet<uint>();
            var enumerator = db.GetStopsEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var stopTags = db.StopAttributes.Get(enumerator.MetaId);
                if (condition(stopTags))
                {
                    return enumerator.Id;
                }
            }
            return Constants.NoStopId;
        }

        /// <summary>
        /// Searches the stops with some tags or based on some condition.
        /// </summary>
        public static HashSet<uint> SearchStopsWithTags(this TransitDb db,
            Func<TagsCollectionBase, bool> condition)
        {
            var stops = new HashSet<uint>();
            var enumerator = db.GetStopsEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var stopTags = db.StopAttributes.Get(enumerator.MetaId);
                if(condition(stopTags))
                {
                    stops.Add(enumerator.Id);
                }
            }
            return stops;
        }

        /// <summary>
        /// Gets the default is trip possible function.
        /// </summary>
        public static Func<uint, DateTime, bool> GetIsTripPossibleFunc(this TransitDb db)
        {
            var tripEnumerator = db.GetTripsEnumerator();
            var schedulesSnumerator = db.GetSchedulesEnumerator();

            return (tripId, day) =>
                {
                    if(tripEnumerator.MoveTo(tripId))
                    {
                        if(schedulesSnumerator.MoveTo(tripEnumerator.ScheduleId))
                        {
                            return schedulesSnumerator.DateIsSet(day);
                        }
                    }
                    return false;
                };
        }

        /// <summary>
        /// Gets the meta-data for the given stop.
        /// </summary>
        public static TagsCollectionBase GetStopMeta(this TransitDb db, uint stopId)
        {
            var enumerator = db.GetStopsEnumerator();
            if(!enumerator.MoveTo(stopId))
            {
                return null;
            }
            return db.StopAttributes.Get(enumerator.MetaId);
        }

        /// <summary>
        /// Copies all core data stops, schedules, trips, and connections from the given transit db.
        /// </summary>
        public static void CopyFrom(this TransitDb db, TransitDb other)
        {
            if (db == null) { throw new ArgumentNullException("db"); }
            if (other == null) { throw new ArgumentNullException("other"); }
            if (other.ConnectionSorting == null) { throw new ArgumentException("A database can only be copied if connections are sorted."); }

            var agencyIds = new Dictionary<uint, uint>();
            var stopIds = new Dictionary<uint, uint>();
            var tripIds = new Dictionary<uint, uint>();
            var scheduleIds = new Dictionary<uint, uint>();

            // copy stops and keep id transformations.
            var stopsEnumerator = other.GetStopsEnumerator();
            while(stopsEnumerator.MoveNext())
            {
                var stopsMeta = other.StopAttributes.Get(stopsEnumerator.MetaId);
                var newMetaId = db.StopAttributes.Add(stopsMeta);
                var newStopId = db.AddStop(stopsEnumerator.Latitude, stopsEnumerator.Longitude, newMetaId);
                stopIds[stopsEnumerator.Id] = newStopId;
            }

            // copy trips, copy schedules that have not been copied yet, and keep trip id transformations.
            var tripsEnumerator = other.GetTripsEnumerator();
            var scheduleEnumerator = other.GetSchedulesEnumerator();
            while (tripsEnumerator.MoveNext())
            {
                var tripsMeta = other.TripAttributes.Get(tripsEnumerator.MetaId);
                var newMetaId = db.TripAttributes.Add(tripsMeta);

                uint newAgencyMetaId = uint.MaxValue;
                if (!agencyIds.TryGetValue(tripsEnumerator.AgencyId, out newAgencyMetaId))
                {
                    var agencyMeta = other.AgencyAttributes.Get(tripsEnumerator.AgencyId);
                    newAgencyMetaId = db.AgencyAttributes.Add(agencyMeta);
                    agencyIds.Add(tripsEnumerator.AgencyId, newAgencyMetaId);
                }

                uint newScheduleId = uint.MaxValue;
                if(!scheduleIds.TryGetValue(tripsEnumerator.ScheduleId, out newScheduleId))
                {
                    if(scheduleEnumerator.MoveTo(tripsEnumerator.ScheduleId))
                    {
                        newScheduleId = scheduleEnumerator.CopyTo(db.SchedulesDb);
                        scheduleIds[tripsEnumerator.ScheduleId] = newScheduleId;
                    }
                }

                var newTripId = db.AddTrip(newScheduleId, newAgencyMetaId, newMetaId);
                tripIds[tripsEnumerator.Id] = newTripId;
            }

            // copy connections.
            var connectionEnumerator = other.GetConnectionsEnumerator(other.ConnectionSorting.Value);
            while(connectionEnumerator.MoveNext())
            {
                var newArrivalStop = stopIds[connectionEnumerator.ArrivalStop];
                var newDepartureStop = stopIds[connectionEnumerator.DepartureStop];
                var newTripId = tripIds[connectionEnumerator.TripId];

                db.AddConnection(newDepartureStop, newArrivalStop, newTripId, connectionEnumerator.DepartureTime, 
                    connectionEnumerator.ArrivalTime);
            }
        }
    }
}