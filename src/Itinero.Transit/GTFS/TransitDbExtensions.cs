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
using Itinero.Attributes;
using Itinero.LocalGeo;
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

            // index the shapes.
            var shapeIndex = new Dictionary<string, ShapePoint[]>();
            if (feed.Shapes != null)
            {
                var originalShapeIndex = new Dictionary<string, List<Shape>>();
                List<Shape> shape = null;
                foreach(var shapePoint in feed.Shapes)
                {
                    if (!originalShapeIndex.TryGetValue(shapePoint.Id, out shape))
                    {
                        shape = new List<Shape>();
                        originalShapeIndex.Add(shapePoint.Id, shape);
                    }

                    shape.Add(shapePoint);
                }
                                
                foreach(var pair in originalShapeIndex)
                {
                    pair.Value.Sort((x, y) =>
                    {
                        if (x.Id == y.Id)
                        {
                            return x.Sequence.CompareTo(y.Sequence);
                        }
                        return x.Id.CompareTo(y.Id);
                    });

                    var shapePoints = new ShapePoint[pair.Value.Count];
                    for (var i = 0; i < shapePoints.Length; i++)
                    {
                        float distanceTravelled = 0;
                        if (pair.Value[i].DistanceTravelled.HasValue)
                        {
                            distanceTravelled = (float)pair.Value[i].DistanceTravelled.Value;
                        }
                        else
                        {
                            if (i > 0)
                            {
                                distanceTravelled = Coordinate.DistanceEstimateInMeter(
                                    (float)pair.Value[i].Latitude, (float)pair.Value[i].Longitude,
                                    shapePoints[i - 1].Latitude, shapePoints[i - 1].Longitude) +
                                    shapePoints[i - 1].DistanceTravelled;
                            }
                        }

                        shapePoints[i] = new ShapePoint()
                        {
                            Latitude = (float)pair.Value[i].Latitude,
                            Longitude = (float)pair.Value[i].Longitude,
                            DistanceTravelled = distanceTravelled
                        };
                    }
                    shapeIndex[pair.Key] = shapePoints;
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
            var currentShapeId = string.Empty;
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
                        var trip = feed.Trips.Get(currentTripId);
                        currentShapeId = trip.ShapeId;
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

                            if (previousStopTime.ShapeDistTravelled.HasValue &&
                                stopTime.ShapeDistTravelled.HasValue)
                            {
                                var shape = db.ShapesDb.Get(stop1, stop2);
                                if (shape == null)
                                {
                                    ShapePoint[] shapePoints = null;
                                    if (shapeIndex.TryGetValue(currentShapeId, out shapePoints))
                                    {
                                        var shapeBetweenStops = ShapePoint.ExtractShape(
                                            shapePoints, (float)previousStopTime.ShapeDistTravelled.Value, (float)stopTime.ShapeDistTravelled.Value);
                                        db.ShapesDb.Add(stop1, stop2, new Graphs.Geometric.Shapes.ShapeEnumerable(shapeBetweenStops));
                                    }
                                }
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

        private class ShapePoint
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
            public float DistanceTravelled { get; set; }

            public static List<Coordinate> ExtractShape(ShapePoint[] shapePoints, float distance1, float distance2)
            {
                var coordinates = new List<Coordinate>();
                for (var i = 0; i < shapePoints.Length; i++)
                {
                    if (coordinates.Count == 0)
                    {
                        if (shapePoints[i].DistanceTravelled >= distance1)
                        { // include first point.
                            if (i == 0)
                            {
                                coordinates.Add(new Coordinate()
                                {
                                    Latitude = shapePoints[i].Latitude,
                                    Longitude = shapePoints[i].Longitude
                                });
                            }
                            else
                            {
                                coordinates.Add(Between(shapePoints[i - 1], shapePoints[i], distance1));
                            }
                        }
                    }
                    else
                    {
                        if (shapePoints[i].DistanceTravelled >= distance2)
                        {
                            coordinates.Add(Between(shapePoints[i - 1], shapePoints[i], distance2));
                            break;
                        }
                        else
                        {
                            coordinates.Add(new Coordinate()
                            {
                                Latitude = shapePoints[i].Latitude,
                                Longitude = shapePoints[i].Longitude
                            });
                        }
                    }
                }
                return coordinates;
            }

            private static Coordinate Between(ShapePoint shapePoint1, ShapePoint shapePoint2, float distance)
            {
                if (shapePoint1.DistanceTravelled == distance)
                {
                    return new Coordinate()
                    {
                        Latitude = shapePoint1.Latitude,
                        Longitude = shapePoint1.Longitude
                    };
                }
                if (shapePoint2.DistanceTravelled == distance)
                {
                    return new Coordinate()
                    {
                        Latitude = shapePoint2.Latitude,
                        Longitude = shapePoint2.Longitude
                    };
                }

                var ratio = (distance - shapePoint1.DistanceTravelled) / (shapePoint2.DistanceTravelled - shapePoint1.DistanceTravelled);

                return new Coordinate()
                {
                    Latitude = shapePoint1.Latitude + (shapePoint2.Latitude - shapePoint1.Latitude) * ratio,
                    Longitude = shapePoint1.Longitude + (shapePoint2.Longitude - shapePoint1.Longitude) * ratio,
                };
            }
        }
    }
}