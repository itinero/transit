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
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.GTFS
{
    /// <summary>
    /// Contains transit db extensions related to GTFS.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Loads data from a gtfs feed.
        /// </summary>
        public static void LoadFrom(this TransitDb db, IGTFSFeed feed)
        {
            if(db.StopsCount > 0 ||
               db.TripsCount > 0)
            { // the database is not empty, cannot load this new GTFS-feed.
                throw new InvalidOperationException("Cannot load a GTFS-feed into a non-empty transit db.");
            }

            // load agencies.
            var agenciesIndex = new Dictionary<string, uint>();
            for(var i = 0; i < feed.Agencies.Count; i++)
            {
                var agency = feed.Agencies.Get(i);
                agenciesIndex[agency.Id] = db.AddAgency(agency);
            }

            // load schedules.
            var calendarTag = 1;
            var schedulesIndex = new Dictionary<string, uint>();
            var calendarDates = new List<CalendarDate>(feed.CalendarDates);
            foreach(var calendar in feed.Calendars)
            {
                for(var day = calendar.StartDate; day <= calendar.EndDate; day = day.AddDays(1))
                {
                    if(calendar.CoversDate(day))
                    {
                        calendarDates.Add(new CalendarDate()
                            {
                                Date = day,
                                ExceptionType = global::GTFS.Entities.Enumerations.ExceptionType.Added,
                                ServiceId = calendar.ServiceId,
                                Tag = calendarTag
                            });
                    }
                }
            }
            calendarDates.Sort((x, y) => {
                var c = x.ServiceId.CompareTo(y.ServiceId);
                if(c == 0)
                {
                    return x.Date.CompareTo(y.Date);
                }
                return c;
            });

            // merge/remove dates.
            for(var i = 0; i < calendarDates.Count - 1; i++)
            {
                if(calendarDates[i].ServiceId == calendarDates[i + 1].ServiceId &&
                   calendarDates[i].Date == calendarDates[i + 1].Date)
                {
                    if (calendarDates[i].ExceptionType == 
                        calendarDates[i + 1].ExceptionType)
                    {
                        calendarDates.RemoveAt(i + 1);
                    }
                    else if (calendarDates[i].Tag == null &&
                        calendarDates[i + 1].Tag != null)
                    {
                        calendarDates.RemoveAt(i + 1);
                    } 
                    else if (calendarDates[i].Tag != null &&
                        calendarDates[i + 1].Tag == null)
                    {
                        calendarDates.RemoveAt(i);
                        i--;
                    }
                }
            }

            // convert to calendar objects again.
            var currentServiceId = string.Empty;
            var currentCalendars = new List<Calendar>();
            var scheduleIds = new Dictionary<string, uint>();
            for(var i = 0; i < calendarDates.Count; i++)
            {
                var current = calendarDates[i];
                if (currentServiceId != current.ServiceId)
                { // start new calendars.
                    if(currentCalendars.Count > 0)
                    { // add previous calendars.
                        currentCalendars[currentCalendars.Count - 1].TrimEndDate();
                        var newScheduleId = db.AddCalendars(currentCalendars);
                        scheduleIds.Add(currentServiceId, newScheduleId);
                        currentCalendars.Clear();
                    }

                    // start working on the next one.
                    currentServiceId = current.ServiceId;

                    if (current.ExceptionType == global::GTFS.Entities.Enumerations.ExceptionType.Added)
                    { // ok, use this as the first new date.
                        var calendarForCurrent = current.Date.CreateCalendar(
                            current.ServiceId);
                        calendarForCurrent.ExpandWeek();
                        calendarForCurrent.TrimStartDate();
                        currentCalendars.Add(calendarForCurrent);
                    }
                    else
                    { // not Add so don't create a week yet, go for the next one.
                        currentServiceId = string.Empty;
                    }
                }
                else
                { // add to existing calendars.
                    var existing = currentCalendars[currentCalendars.Count - 1];
                    if (existing.EndDate >= current.Date)
                    { // should be part of the last calendar.
                        existing.Set(current.Date,
                            current.ExceptionType == global::GTFS.Entities.Enumerations.ExceptionType.Added);
                    }
                    else if (current.ExceptionType == global::GTFS.Entities.Enumerations.ExceptionType.Added)
                    { // add new calendar.
                        var calendarForCurrent = current.Date.CreateCalendar(
                            current.ServiceId);
                        calendarForCurrent.ExpandWeek();
                        calendarForCurrent.TrimStartDate();
                        currentCalendars.Add(calendarForCurrent);
                    }
                }
            }
            if (currentCalendars.Count > 0)
            { // add last calendars.
                currentCalendars[currentCalendars.Count - 1].TrimEndDate();
                var newScheduleId = db.AddCalendars(currentCalendars);
                scheduleIds.Add(currentServiceId, newScheduleId);
            }

            // load trips.
            var tripsIndex = new Dictionary<string, uint>();
            var tripServiceIds = new Dictionary<string, string>();
            for (var i = 0; i < feed.Trips.Count; i++)
            {
                var trip = feed.Trips.Get(i);
                tripServiceIds[trip.Id] = trip.ServiceId;
                var route = feed.Routes.Get(trip.RouteId);
                uint agencyId, scheduleId;
                if(agenciesIndex.TryGetValue(route.AgencyId, out agencyId) &&
                   scheduleIds.TryGetValue(trip.ServiceId, out scheduleId))
                {
                    tripsIndex[trip.Id] = db.AddTrip(trip, agencyId, scheduleId);
                }
            }

            // load stops.
            var stopsIndex = new Dictionary<string, uint>();
            for (var i = 0; i < feed.Stops.Count; i++)
            {
                var stop = feed.Stops.Get(i);
                stopsIndex[stop.Id] = db.AddStop(stop);
            }

            // load connections.
            var stopTimes = new List<StopTime>(feed.StopTimes);
            stopTimes.Sort((x, y) =>
            {
                var c = x.TripId.CompareTo(y.TripId);
                if (c == 0)
                {
                    return x.StopSequence.CompareTo(y.StopSequence);
                }
                return c;
            });
            var currentTripId = string.Empty;
            for (var i = 0; i < stopTimes.Count; i++)
            {
                var stopTime = stopTimes[i];
                var stopTimeServiceId = string.Empty;
                uint tripId;
                if(tripServiceIds.TryGetValue(stopTime.TripId, out stopTimeServiceId) &&
                   tripsIndex.TryGetValue(stopTime.TripId, out tripId))
                {
                    if (currentTripId != stopTime.TripId)
                    { // start a new sequence.
                        currentTripId = stopTime.TripId;
                    }
                    else
                    { // the previous stop time has the same id, add them as a connection.
                        var previousStopTime = stopTimes[i - 1];
                        uint stop1, stop2;
                        if(stopsIndex.TryGetValue(previousStopTime.StopId, out stop1) &&
                           stopsIndex.TryGetValue(stopTime.StopId, out stop2))
                        {
                            db.AddConnection(stop1, stop2, tripId,
                                (uint)previousStopTime.DepartureTime.TotalSeconds, (uint)stopTime.ArrivalTime.TotalSeconds);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds all given calendars in a way as compact as possible.
        /// </summary>
        public static uint AddCalendars(this TransitDb db, List<Calendar> calendars)
        {
            // merge mergeable calendars.
            int c = 0;
            while (c + 1 < calendars.Count)
            {
                Calendar mergedCalendar = null;
                if (calendars[c].TryMerge(calendars[c + 1], out mergedCalendar))
                {
                    calendars[c] = mergedCalendar;
                    calendars.RemoveAt(c + 1);
                }
                else
                {
                    c++;
                }
            }

            // add to db.
            var newScheduleId = db.AddSchedule();
            for (var j = 0; j < calendars.Count; j++)
            {
                db.AddScheduleEntry(newScheduleId, calendars[j].StartDate,
                    calendars[j].EndDate, calendars[j].Mask);
            }
            return newScheduleId;
        }

        /// <summary>
        /// Adds an agency.
        /// </summary>
        public static uint AddAgency(this TransitDb db, Agency agency)
        {
            var attributes = new TagsCollection();
            attributes.Add("id", agency.Id);
            attributes.AddNotNullOrWhiteSpace("fare_url", agency.FareURL);
            attributes.AddNotNullOrWhiteSpace("language_code", agency.LanguageCode);
            attributes.AddNotNullOrWhiteSpace("name", agency.Name);
            attributes.AddNotNullOrWhiteSpace("phone", agency.Phone);
            attributes.AddNotNullOrWhiteSpace("timezone", agency.Timezone);
            attributes.AddNotNullOrWhiteSpace("url", agency.URL);

            return db.AgencyAttributes.Add(attributes);
        }

        /// <summary>
        /// Adds a trip for the given agency.
        /// </summary>
        public static uint AddTrip(this TransitDb db, Trip trip, uint agencyId, uint scheduleId)
        {
            var attributes = new TagsCollection();
            attributes.Add("id", trip.Id);
            attributes.AddNotNullOrWhiteSpace("accessibility_type", trip.AccessibilityType == null ? 
                string.Empty : trip.AccessibilityType.ToInvariantString());
            attributes.AddNotNullOrWhiteSpace("block_id", trip.BlockId);
            attributes.AddNotNullOrWhiteSpace("direction", trip.Direction == null ?
                string.Empty : trip.Direction.ToInvariantString());
            attributes.AddNotNullOrWhiteSpace("headsign", trip.Headsign);
            attributes.AddNotNullOrWhiteSpace("route_id", trip.RouteId);
            attributes.AddNotNullOrWhiteSpace("service_id", trip.ServiceId);
            attributes.AddNotNullOrWhiteSpace("shape_id", trip.ShapeId);
            attributes.AddNotNullOrWhiteSpace("short_name", trip.ShortName);

            var metaId = db.TripAttributes.Add(attributes);

            return db.AddTrip(scheduleId, agencyId, metaId);
        }

        /// <summary>
        /// Adds a stop.
        /// </summary>
        /// <returns></returns>
        public static uint AddStop(this TransitDb db, Stop stop)
        {
            var attributes = new TagsCollection();
            attributes.Add("id", stop.Id);
            attributes.AddNotNullOrWhiteSpace("code", stop.Code);
            attributes.AddNotNullOrWhiteSpace("description", stop.Description);
            attributes.AddNotNullOrWhiteSpace("location_type", stop.LocationType == null ? 
                string.Empty : stop.LocationType.Value.ToInvariantString());
            attributes.AddNotNullOrWhiteSpace("name", stop.Name);
            attributes.AddNotNullOrWhiteSpace("timezone", stop.Timezone);
            attributes.AddNotNullOrWhiteSpace("url", stop.Url);
            attributes.AddNotNullOrWhiteSpace("wheelchairboarding", stop.WheelchairBoarding);
            attributes.AddNotNullOrWhiteSpace("zone", stop.Zone);

            var metaId = db.StopAttributes.Add(attributes);

            return db.AddStop((float)stop.Latitude, (float)stop.Longitude, metaId);
        }

        /// <summary>
        /// Adds the key value pair if the value is not null, empty or whitespace.
        /// </summary>
        public static void AddNotNullOrWhiteSpace(this TagsCollectionBase attibutes, string key, string value)
        {
            if(!string.IsNullOrWhiteSpace(value))
            {
                attibutes.Add(key, value);
            }
        }

        /// <summary>
        /// Expand the start and end date to the beginning and end of the week respectively.
        /// </summary>
        public static void ExpandWeek(this Calendar calendar)
        {
            calendar.StartDate = calendar.StartDate.FirstDayOfWeek();
            calendar.EndDate = calendar.EndDate.LastDayOfWeek();
        }
    }
}