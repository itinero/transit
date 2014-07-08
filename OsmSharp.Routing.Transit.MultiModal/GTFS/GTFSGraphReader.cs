﻿using GTFS;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.GTFS
{ /// <summary>
    /// A reader to read a GTFS feed and convert into a routable network.
    /// </summary>
    public class GTFSGraphReader
    {
        /// <summary>
        /// Reads and converts a GTFS feed into a routable graph.
        /// </summary>
        /// <param name="graph">The graph to add the info to.</param>
        /// <param name="feed">The feed to convert.</param>
        /// <param name="stopVertices">The vertex for each stop.</param>
        /// <param name="tripIds">The trip ids per trip.</param>
        /// <param name="schedules">The schedules.</param>
        public static void AddToGraph(DynamicGraphRouterDataSource<LiveEdge> graph, GTFSFeed feed, Dictionary<string, uint> stopVertices,
            Dictionary<string, uint> tripIds, List<TransitEdgeSchedulePair> schedules)
        {
            var vehicles = new List<Vehicle>();
            vehicles.Add(Vehicle.Pedestrian);
            vehicles.Add(Vehicle.Bicycle);
            vehicles.Add(Vehicle.Car);

            var interpreter = new OsmRoutingInterpreter();

            GTFSGraphReader.AddToGraph(graph, feed, stopVertices, tripIds, schedules, interpreter, vehicles);
        }

        /// <summary>
        /// Reads and converts a GTFS feed into a routable graph.
        /// </summary>
        /// <param name="graph">The graph to add the info to.</param>
        /// <param name="feed">The feed to convert.</param>
        /// <param name="stopVertices">The vertex for each stop.</param>
        /// <param name="tripIds">The trip ids per trip.</param>
        /// <param name="schedules">The schedules.</param>
        /// <param name="interpreter">The routing interperter.</param>
        /// <param name="vehicles">The list of vehicles to tie stations to the road network.</param>
        /// <returns></returns>
        public static void AddToGraph(DynamicGraphRouterDataSource<LiveEdge> graph, GTFSFeed feed, Dictionary<string, uint> stopVertices,
            Dictionary<string, uint> tripIds, List<TransitEdgeSchedulePair> schedules, IRoutingInterpreter interpreter, List<Vehicle> vehicles)
        {
            if (feed == null) { throw new ArgumentNullException("feed"); }
            if (feed.Stops == null || feed.Stops.Count == 0) { throw new ArgumentException("No stops in the given feed."); }
            if (feed.StopTimes == null || feed.StopTimes.Count == 0) { throw new ArgumentException("No stop times in the given feed."); }

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
                        var edge = new LiveEdge();
                        var schedulePair = new TransitEdgeSchedulePair();
                        schedules.Add(schedulePair);
                        edge.Tags = Extensions.EncodeScheduleId(schedules.Count - 1);
                        graph.AddArc(previousVertex, vertex, edge, null);
                    }
                    var transitEdge = graph.GetArc(previousVertex, vertex);

                    // get the schedule and add entry.
                    var schedule = transitEdge.GetForwardSchedule(schedules);
                    schedule.Add(tripIds[stopTime.TripId], departure, arrival);

                    // BACKWARD: add edge or get the edge data.
                    if (!graph.HasArc(vertex, previousVertex))
                    { // the arc is not there yet, add it.
                        var edge = new LiveEdge();
                        var schedulePair = new TransitEdgeSchedulePair();
                        schedules.Add(schedulePair);
                        edge.Tags = Extensions.EncodeScheduleId(schedules.Count - 1);
                        graph.AddArc(vertex, previousVertex, edge, null);
                    }
                    transitEdge = graph.GetArc(vertex, previousVertex);

                    // get the schedule and add entry.
                    schedule = transitEdge.GetBackwardSchedule(schedules);
                    schedule.Add(tripIds[stopTime.TripId], departure, arrival);
                }
                previousStopTime = stopTime;
            }

            // sort all schedules in all arcs.
            for (uint vertex = 1; vertex < graph.VertexCount + 1; vertex++)
            {
                var arcs = graph.GetArcs(vertex);
                foreach (var arc in arcs)
                {
                    var forwardSchedule = arc.Value.GetForwardSchedule(schedules);
                    if (forwardSchedule != null)
                    { // sort the forward schedule.
                        forwardSchedule.Entries.Sort();
                    }
                    var backwardSchedule = arc.Value.GetBackwardSchedule(schedules);
                    if (backwardSchedule != null)
                    { // sort the backward schedule.
                        backwardSchedule.Entries.Sort();
                    }
                }
            }

            // link stops to network.
            foreach (var vehicle in vehicles)
            {
                float latitude, longitude;
                int max = 3;
                foreach (var stop in stopVertices.Values)
                {
                    // get stop location.
                    if (graph.GetVertex(stop, out latitude, out longitude))
                    { // found the vertex! duh!
                        // keep stop location
                        var stopLocation = new GeoCoordinate(latitude, longitude);

                        // neighbouring vertices.
                        var arcs = graph.GetArcs(new Math.Geo.GeoCoordinateBox(new Math.Geo.GeoCoordinate(latitude - 0.01, longitude - 0.01),
                            new Math.Geo.GeoCoordinate(latitude + 0.01, longitude + 0.01)));

                        // keep a sorted list.
                        var closestVertices = new Dictionary<uint, double>();

                        // link this station to the road network for the current vehicle.
                        foreach (var arc in arcs)
                        {
                            bool isRoutable = arc.Value.Value.IsRoad();
                            if(isRoutable)
                            { // the arc is already a road.
                                if(graph.TagsIndex.Contains(arc.Value.Value.Tags))
                                { // there is a tags collection.
                                    var tags = graph.TagsIndex.Get(arc.Value.Value.Tags);
                                    isRoutable = interpreter.EdgeInterpreter.CanBeTraversedBy(tags, vehicle);
                                }
                            }
                            if (isRoutable)                                
                            { // this arc is a road to keep it.
                                if (arc.Key != stop &&
                                    graph.GetVertex(arc.Key, out latitude, out longitude))
                                { // check distance.
                                    var keyDistance = stopLocation.DistanceReal(
                                        new GeoCoordinate(latitude, longitude)).Value;
                                    closestVertices[arc.Key] = keyDistance;
                                }
                                if (arc.Value.Key != stop &&
                                    graph.GetVertex(arc.Value.Key, out latitude, out longitude))
                                { // check distance.
                                    double keyDistance = stopLocation.DistanceReal(
                                        new GeoCoordinate(latitude, longitude)).Value;
                                    closestVertices[arc.Value.Key] = keyDistance;
                                }
                            }
                        }

                        // sort vertices.
                        var sorted = closestVertices.OrderBy(x => x.Value).GetEnumerator();
                        for (int idx = 0; idx < max; idx++)
                        {
                            if (sorted.MoveNext())
                            {
                                var closest = sorted.Current.Key;
                                graph.AddArc(stop, closest, new LiveEdge(), null);
                                graph.AddArc(closest, stop, new LiveEdge(), null);
                            }
                            else
                            { // no more sorted vertices.
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}