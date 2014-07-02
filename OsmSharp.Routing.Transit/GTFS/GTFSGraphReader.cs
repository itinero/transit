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
using OsmSharp.Routing.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.GTFS
{
    /// <summary>
    /// A reader to read a GTFS feed and convert into a routable network.
    /// </summary>
    public class GTFSGraphReader
    {
        /// <summary>
        /// Reads and converts a GTFS feed into a routable graph.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public static IDynamicGraph<TransitEdge> CreateGraph(GTFSFeed feed)
        {
            return GTFSGraphReader.CreateGraph(feed, new Dictionary<string, uint>(), new Dictionary<string, uint>());
        }

        /// <summary>
        /// Reads and converts a GTFS feed into a routable graph.
        /// </summary>
        /// <param name="feed">The feed to convert.</param>
        /// <param name="stopVertices">The vertex for each stop.</param>
        /// <param name="tripIds">The trip ids per trip.</param>
        /// <returns></returns>
        public static IDynamicGraph<TransitEdge> CreateGraph(GTFSFeed feed, Dictionary<string, uint> stopVertices, Dictionary<string, uint> tripIds)
        {
            if (feed == null) { throw new ArgumentNullException("feed"); }
            if (feed.Stops == null || feed.Stops.Count == 0) { throw new ArgumentException("No stops in the given feed."); }
            if (feed.StopTimes == null || feed.StopTimes.Count == 0) { throw new ArgumentException("No stop times in the given feed."); }

            // instantiate the graph.
            var graph = new MemoryDynamicGraph<TransitEdge>(feed.Stops.Count);

            // read all the stops.
            foreach (var stop in feed.Stops)
            {
                var stopVertex = graph.AddVertex((float)stop.Latitude, (float)stop.Longitude);

                stopVertices.Add(stop.Id, stopVertex);
            }

            // build trip ids.
            for (int tripIdx = 0; tripIdx < feed.Trips.Count; tripIdx++)
            {
                if (!tripIds.ContainsKey(feed.Trips[tripIdx].Id))
                {
                    tripIds[feed.Trips[tripIdx].Id] = (uint)tripIdx;
                }
            }

            // sort stop-times.
            feed.StopTimes.Sort((x, y) =>
            {
                if (x.TripId.Equals(y.TripId))
                { // trip id's equal compare stop sequence.
                    return x.StopSequence.CompareTo(y.StopSequence);
                }
                return x.TripId.CompareTo(y.TripId);
            });

            // loop over all stoptimes.
            var previousStopTime = feed.StopTimes[0];
            for (int stopTimeIdx = 1; stopTimeIdx < feed.StopTimes.Count; stopTimeIdx++)
            {
                var stopTime = feed.StopTimes[stopTimeIdx];

                // check if two stop times belong to the same trip.
                if (previousStopTime.TripId == stopTime.TripId)
                { // we have two stops in the same trip
                    // parse arrival/departure.
                    var arrival = stopTime.ArrivalTime.TotalSeconds;
                    var departure = previousStopTime.DepartureTime.TotalSeconds;

                    // get start/end vertex.
                    uint previousVertex = stopVertices[previousStopTime.StopId];
                    uint vertex = stopVertices[stopTime.StopId];

                    // FORWARD: add edge or get the edge data.
                    if (!graph.HasArc(previousVertex, vertex))
                    { // the arc is not there yet, add it.
                        graph.AddArc(previousVertex, vertex, new TransitEdge(), null);
                    }
                    var transitEdge = graph.GetArc(previousVertex, vertex);

                    // get the schedule and add entry.
                    var schedule = transitEdge.ForwardSchedule;
                    schedule.Add(tripIds[stopTime.TripId], departure, arrival);

                    // BACKWARD: add edge or get the edge data.
                    if (!graph.HasArc(vertex, previousVertex))
                    { // the arc is not there yet, add it.
                        graph.AddArc(vertex, previousVertex, new TransitEdge(), null);
                    }
                    transitEdge = graph.GetArc(vertex, previousVertex);

                    // get the schedule and add entry.
                    schedule = transitEdge.BackwardSchedule;
                    schedule.Add(tripIds[stopTime.TripId], departure, arrival);
                }
                previousStopTime = stopTime;
            }

            for (uint vertex = 1; vertex < graph.VertexCount + 1; vertex++)
            {
                var arcs = graph.GetArcs(vertex);
                foreach (var arc in arcs)
                {
                    if (arc.Value.ForwardSchedule != null)
                    {
                        arc.Value.ForwardSchedule.Entries.Sort();
                    }
                    if (arc.Value.BackwardSchedule != null)
                    {
                        arc.Value.BackwardSchedule.Entries.Sort();
                    }
                }
            }

            return graph;
        }

        /// <summary>
        /// Reads and converts a GTFS feed into a transit router.
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public static TransitRouter CreateRouter(GTFSFeed feed)
        {
            var stopVertices = new Dictionary<string, uint>();
            var tripIds = new Dictionary<string, uint>();
            var graph = GTFSGraphReader.CreateGraph(feed, stopVertices, tripIds);
            return new TransitRouter(feed, stopVertices, graph);
        }
    }
}