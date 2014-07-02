// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Transit.RouteCalculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// A transit router.
    /// </summary>
    public class TransitRouter
    {
        /// <summary>
        /// Holds the vertex for each stop.
        /// </summary>
        private Dictionary<string, uint> _stopVertices;

        /// <summary>
        /// Holds the trip ids.
        /// </summary>
        private Dictionary<uint, string> _tripIds;

        /// <summary>
        /// Holds the stop for each vertex.
        /// </summary>
        private string[] _verticesStops;

        /// <summary>
        /// Holds the graph.
        /// </summary>
        private IDynamicGraph<TransitEdge> _graph;

        /// <summary>
        /// Holds the GTFS feed.
        /// </summary>
        private GTFSFeed _feed;

        /// <summary>
        /// Holds the actual transit router implementation.
        /// </summary>
        private IRouteCalculator _transitRouter = new ReferenceCalculator();

        /// <summary>
        /// Creates a new transit router.
        /// </summary>
        /// <param name="stopVertices"></param>
        /// <param name="graph"></param>
        internal TransitRouter(GTFSFeed feed, Dictionary<string, uint> stopVertices, IDynamicGraph<TransitEdge> graph)
        {
            _feed = feed;
            _graph = graph;
            _stopVertices = stopVertices;

            _verticesStops = new string[_stopVertices.Count + 1];
            foreach(var pair in _stopVertices)
            {
                _verticesStops[pair.Value] = pair.Key;
            }

            _tripIds = new Dictionary<uint, string>();
            for (uint tripIdx = 0; tripIdx < feed.Trips.Count; tripIdx++)
            {
                _tripIds[tripIdx] = feed.Trips[(int)tripIdx].Id;
            }
            this.IndexFeed(_feed);
        }

        /// <summary>
        /// Returns the graph this router is using.
        /// </summary>
        public IDynamicGraph<TransitEdge> Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Calculates a route between two stops.
        /// </summary>
        /// <param name="fromStop"></param>
        /// <param name="toStop"></param>
        /// <param name="departureTime"></param>
        /// <returns></returns>
        public TransitRoute Calculate(string fromStop, string toStop, DateTime departureTime)
        {
            var fromStopVertex = _stopVertices[fromStop];
            var toStopVertex = _stopVertices[toStop];

            var path = _transitRouter.Calculate(_graph, fromStopVertex, toStopVertex, departureTime, IsTripPossible);

            return this.ConvertToTransitRoute(path, departureTime);
        }

        /// <summary>
        /// Converts a path to a transit route.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private TransitRoute ConvertToTransitRoute(PathSegment<VertexTimeAndTrip> path, DateTime departureTime)
        {
            if(path == null)
            { // an empty path, an emtpy route!
                return null;
            }

            // instantiate route.
            var route = new TransitRoute();
            route.Entries = new List<TransitRouteEntry>();

            // build entries.
            var pathArray = path.ToArray();
            var previous = pathArray[0];
            var currentEntries = new List<TransitRouteStop>();
            currentEntries.Add(new TransitRouteStop()
            {
                Stop = _feed.GetStop(_verticesStops[(int)previous.Vertex]),
                Time = departureTime.AddSeconds(previous.Seconds)
            });
            for (int idx = 1; idx < pathArray.Length; idx++)
            {
                var current = pathArray[idx];
                if(current.Trip != previous.Trip)
                { // no trip!
                    if (currentEntries.Count > 1)
                    { // drop entries with only one entry, they are transfers.
                        route.Entries.Add(new TransitRouteEntry()
                        {
                            Stops = currentEntries,
                            Trip = _feed.GetTrip(_tripIds[current.Trip])
                        });
                    }

                    // reset entries.
                    currentEntries = new List<TransitRouteStop>();
                }

                currentEntries.Add(new TransitRouteStop()
                {
                    Stop = _feed.GetStop(_verticesStops[(int)current.Vertex]),
                    Time = departureTime.AddSeconds(current.Seconds)
                });

                previous = current;
            }
            if (currentEntries.Count > 1)
            { // drop entries with only one entry, they are transfers.
                route.Entries.Add(new TransitRouteEntry()
                {
                    Stops = currentEntries,
                    Trip = _feed.GetTrip(_tripIds[previous.Trip])
                });
            }

            return route;
        }

        #region Feed Indexes

        /// <summary>
        /// Holds the calendars per trip.
        /// </summary>
        private Dictionary<string, Calendar> _calendars = new Dictionary<string,Calendar>();

        /// <summary>
        /// Holds the calendar dates per trip.
        /// </summary>
        private Dictionary<string, Dictionary<DateTime, CalendarDate>> _calendarDates = new Dictionary<string, Dictionary<DateTime, CalendarDate>>();

        /// <summary>
        /// Holds the services per trip.
        /// </summary>
        private Dictionary<string, string> _services = new Dictionary<string,string>();

        /// <summary>
        /// Indexes a GTFS feed for use in this router.
        /// </summary>
        /// <param name="feed"></param>
        private void IndexFeed(GTFSFeed feed)
        {
            foreach(var trip in feed.Trips)
            {
                if(!string.IsNullOrEmpty(trip.ServiceId))
                {
                    _services[trip.Id] = trip.ServiceId;
                }
            }

            if (feed.Calendars != null)
            { // there is calendar data.
                foreach (var calendar in feed.Calendars)
                {
                    _calendars[calendar.ServiceId] = calendar;
                }
            }
            if(feed.CalendarDates != null)
            { // there are calendar date exceptions.
                foreach (var calendarDate in feed.CalendarDates)
                {
                    Dictionary<DateTime, CalendarDate> calendarDates;
                    if(!_calendarDates.TryGetValue(calendarDate.ServiceId, out calendarDates))
                    { // there is not yet an index for the given service id.
                        calendarDates = new Dictionary<DateTime, CalendarDate>();
                        _calendarDates.Add(calendarDate.ServiceId, calendarDates);
                    }
                    calendarDates[calendarDate.Date] = calendarDate;
                }
            }
        }

        /// <summary>
        /// Returns true if the given trip is possible on the given date.
        /// </summary>
        /// <param name="trip"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private bool IsTripPossible(uint trip, DateTime date)
        {
            date = date.Date;

            string service;
            if (_services.TryGetValue(_tripIds[trip], out service))
            { // the trip and service exist.
                CalendarDate calendarDate;
                Dictionary<DateTime, CalendarDate> calendarDates;
                if (_calendarDates.TryGetValue(service, out calendarDates) &&
                    calendarDates.TryGetValue(date, out calendarDate))
                { // a calendar date exists, this will rule out anything else.
                    if (calendarDate.ExceptionType == global::GTFS.Entities.Enumerations.ExceptionType.Removed)
                    { // date was explicitly removed.
                        return false;
                    }
                    return true; // date was explicitly added.
                }

                Calendar calendar;
                if (_calendars.TryGetValue(service, out calendar))
                { // a calendar exists.
                    return calendar.CoversDate(date);
                }
            }
            return true; // if nothing is found, return true.
        }

        #endregion
    }
}