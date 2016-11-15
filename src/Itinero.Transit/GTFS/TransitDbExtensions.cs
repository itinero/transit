// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using GTFS;
using GTFS.Entities;
using Itinero.Attributes;
using Itinero.Transit.Data;
using System;
using System.Collections.Generic;

namespace Itinero.Transit.GTFS
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
                agenciesIndex[agency.Id.ToStringEmptyWhenNull()] = db.AddAgency(agency);
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

            // load routes.
            var routes = new Dictionary<string, global::GTFS.Entities.Route>();
            for (var i = 0; i < feed.Routes.Count; i++)
            {
                var route = feed.Routes.Get(i);
                routes[route.Id] = route;
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
                global::GTFS.Entities.Route gtfsRoute;
                if(agenciesIndex.TryGetValue(route.AgencyId.ToStringEmptyWhenNull(), out agencyId) &&
                   scheduleIds.TryGetValue(trip.ServiceId, out scheduleId) &&
                   routes.TryGetValue(trip.RouteId, out gtfsRoute))
                {
                    tripsIndex[trip.Id] = db.AddTrip(trip, gtfsRoute, agencyId, scheduleId);
                }
            }

            // load stops.
            var stopsReverseIndex = new Extensions.LinkedListNode<string>[feed.Stops.Count];
            var stopsIndex = new Dictionary<string, uint>();
            for (var i = 0; i < feed.Stops.Count; i++)
            {
                var stop = feed.Stops.Get(i);
                if (string.IsNullOrWhiteSpace(stop.ParentStation))
                { // only add stops that have no parent station.
                    var stopId = db.AddStop(stop);
                    stopsReverseIndex[stopId] = new Extensions.LinkedListNode<string>(stop.Id);
                    stopsIndex[stop.Id] = stopId;
                }
            }
            for(var i = 0; i < feed.Stops.Count; i++)
            {
                var stop = feed.Stops.Get(i);
                if(!string.IsNullOrWhiteSpace(stop.ParentStation))
                { // now add the stops that have parent stations.
                    uint stopId;
                    if(!stopsIndex.TryGetValue(stop.ParentStation, out stopId))
                    { // oeps, parent station not found.
                        throw new Exception("A station was found with a parent station that has a parent station of it's own. Only one level of station hierarchy is supported.");
                    }
                    var node = stopsReverseIndex[stopId];
                    while (node.Next != null)
                    {
                        node = node.Next;
                    }
                    node.Next = new Extensions.LinkedListNode<string>(stop.Id);
                }
            }

            // sort stops.
            db.SortStops((i, j) =>
                {
                    var temp = stopsReverseIndex[i];
                    stopsReverseIndex[i] = stopsReverseIndex[j];
                    stopsReverseIndex[j] = temp;
                });

            // re-index stops.
            for(uint i = 0; i < stopsReverseIndex.Length; i++)
            {
                var node = stopsReverseIndex[i];
                while(node != null)
                {
                    stopsIndex[node.Value] = i;
                    node = node.Next;
                }
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
            uint collisionOffset = 1;
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
                        collisionOffset = 1;
                    }
                    else
                    { // the previous stop time has the same id, add them as a connection.
                        var previousStopTime = stopTimes[i - 1];
                        uint stop1, stop2;
                        if(stopsIndex.TryGetValue(previousStopTime.StopId, out stop1) &&
                           stopsIndex.TryGetValue(stopTime.StopId, out stop2))
                        {
                            if (stopTime.ArrivalTime.TotalSeconds - previousStopTime.DepartureTime.TotalSeconds < collisionOffset)
                            { // make sure arrival and departure time differ at least one second.
                                db.AddConnection(stop1, stop2, tripId,
                                    (uint)previousStopTime.DepartureTime.TotalSeconds + (collisionOffset - 1), (uint)stopTime.ArrivalTime.TotalSeconds + collisionOffset);
                                collisionOffset++;
                            }
                            else if(collisionOffset > 1)
                            { // the previous time was offsetted, also offset this departure time.
                                db.AddConnection(stop1, stop2, tripId,
                                    (uint)previousStopTime.DepartureTime.TotalSeconds + (collisionOffset - 1), (uint)stopTime.ArrivalTime.TotalSeconds);
                                collisionOffset = 1;
                            }
                            else
                            { // arrival and departure time differ already.
                                db.AddConnection(stop1, stop2, tripId,
                                    (uint)previousStopTime.DepartureTime.TotalSeconds, (uint)stopTime.ArrivalTime.TotalSeconds);
                                collisionOffset = 1;
                            }
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
            var attributes = new AttributeCollection();
            attributes.AddOrReplace("id", agency.Id.ToStringEmptyWhenNull());
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
        public static uint AddTrip(this TransitDb db, Trip trip, global::GTFS.Entities.Route route, uint agencyId, uint scheduleId)
        {
            var attributes = new AttributeCollection();
            attributes.AddOrReplace("id", trip.Id);
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

            attributes.AddNotNullOrWhiteSpace("route_color", route.Color.ToHexColorString());
            attributes.AddNotNullOrWhiteSpace("route_description", route.Description);
            attributes.AddNotNullOrWhiteSpace("route_long_name", route.LongName);
            attributes.AddNotNullOrWhiteSpace("route_short_name", route.ShortName);
            attributes.AddNotNullOrWhiteSpace("route_text_color", route.TextColor.ToHexColorString());

            var metaId = db.TripAttributes.Add(attributes);

            return db.AddTrip(scheduleId, agencyId, metaId);
        }

        /// <summary>
        /// Adds a stop.
        /// </summary>
        /// <returns></returns>
        public static uint AddStop(this TransitDb db, Stop stop)
        {
            var attributes = new AttributeCollection();
            attributes.AddOrReplace("id", stop.Id);
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
        public static void AddNotNullOrWhiteSpace(this AttributeCollection attributes, string key, string value)
        {
            if(!string.IsNullOrWhiteSpace(value))
            {
                attributes.AddOrReplace(key, value);
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