using GTFS;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Units.Distance;
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
        /// Holds the maximum distance a station access point can be from the actual station node.
        /// </summary>
        private static Meter MAX_ACCESS_POINT_DISTANCE = 1000;

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
            if (feed.GetStops() == null || feed.GetStops().Count() == 0) { throw new ArgumentException("No stops in the given feed."); }
            if (feed.GetStopTimes() == null || feed.GetStopTimes().Count() == 0) { throw new ArgumentException("No stop times in the given feed."); }

            // read all the stops.
            foreach (var stop in feed.GetStops())
            {
                var stopVertex = graph.AddVertex((float)stop.Latitude, (float)stop.Longitude);
                stopVertices.Add(stop.Id, stopVertex);
            }

            // build trip ids.
            int tripIdx = 0;
            foreach (var trip in feed.GetTrips())
            {
                if (!tripIds.ContainsKey(trip.Id))
                {
                    tripIds[trip.Id] = (uint)tripIdx;
                }
                tripIdx++;
            }

            // loop over all stoptimes.
            var stopTimesEnumerator = feed.GetStopTimes().GetEnumerator();
            stopTimesEnumerator.MoveNext();
            var previousStopTime = stopTimesEnumerator.Current;
            while (stopTimesEnumerator.MoveNext())
            {
                var stopTime = stopTimesEnumerator.Current;

                // check if two stop times belong to the same trip.
                if (previousStopTime.TripId == stopTime.TripId)
                { // we have two stops in the same trip
                    // parse arrival/departure.
                    var arrival = stopTime.ArrivalTime.TotalSeconds;
                    var departure = previousStopTime.DepartureTime.TotalSeconds;

                    // get start/end vertex.
                    uint previousVertex = stopVertices[previousStopTime.StopId];
                    uint vertex = stopVertices[stopTime.StopId];

                    if (previousVertex != vertex)
                    { // vertices have to be different.

                        // FORWARD: add edge or get the edge data.
                        LiveEdge transitEdge;
                        if (!graph.GetEdge(previousVertex, vertex, out transitEdge))
                        { // the arc is not there yet, add it.
                            transitEdge = new LiveEdge();
                            var schedulePair = new TransitEdgeSchedulePair();
                            schedules.Add(schedulePair);
                            transitEdge.Tags = Extensions.EncodeScheduleId(schedules.Count - 1);
                            graph.AddEdge(previousVertex, vertex, transitEdge, null);
                        }

                        // get the schedule and add entry.
                        if (transitEdge.Forward)
                        { // edge is defined from previousVertex -> vertex
                            var schedule = transitEdge.GetForwardSchedule(schedules);
                            schedule.Add(tripIds[stopTime.TripId], departure, arrival);
                        }
                        else
                        { // edge is defined from vertex -> previousVertex.
                            var schedule = transitEdge.GetBackwardSchedule(schedules);
                            schedule.Add(tripIds[stopTime.TripId], departure, arrival);
                        }

                        //// BACKWARD: add edge or get the edge data.
                        //if (!graph.GetEdge(vertex, previousVertex, out transitEdge))
                        //{ // the arc is not there yet, add it.
                        //    transitEdge = new LiveEdge();
                        //    var schedulePair = new TransitEdgeSchedulePair();
                        //    schedules.Add(schedulePair);
                        //    transitEdge.Tags = Extensions.EncodeScheduleId(schedules.Count - 1);
                        //    graph.AddEdge(vertex, previousVertex, transitEdge, null);
                        //}

                        //// get the schedule and add entry.
                        //schedule = transitEdge.GetBackwardSchedule(schedules);
                        //schedule.Add(tripIds[stopTime.TripId], departure, arrival);
                    }
                }
                previousStopTime = stopTime;
            }

            // sort all schedules in all arcs.
            for (uint vertex = 1; vertex < graph.VertexCount + 1; vertex++)
            {
                var arcs = graph.GetEdges(vertex);
                foreach (var arc in arcs)
                {
                    var forwardSchedule = arc.EdgeData.GetForwardSchedule(schedules);
                    if (forwardSchedule != null)
                    { // sort the forward schedule.
                        forwardSchedule.Entries.Sort();
                    }
                    var backwardSchedule = arc.EdgeData.GetBackwardSchedule(schedules);
                    if (backwardSchedule != null)
                    { // sort the backward schedule.
                        backwardSchedule.Entries.Sort();
                    }
                }
            }

            // link stops to network.
            float latitude, longitude;
            int max = 2;
            foreach (var stop in stopVertices.Values)
            {
                // get stop location.
                if (graph.GetVertex(stop, out latitude, out longitude))
                { // found the vertex! duh!
                    // keep stop location
                    var stopLocation = new GeoCoordinate(latitude, longitude);

                    // neighbouring vertices.
                    var arcs = graph.GetEdges(new Math.Geo.GeoCoordinateBox(new Math.Geo.GeoCoordinate(latitude - 0.005, longitude - 0.0025),
                        new Math.Geo.GeoCoordinate(latitude + 0.005, longitude + 0.0025)));

                    foreach (var vehicle in vehicles)
                    {
                        // keep a sorted list.
                        var closestVertices = new Dictionary<uint, double>();

                        // link this station to the road network for the current vehicle.
                        foreach (var arc in arcs)
                        {
                            bool isRoutable = arc.Value.Value.IsRoad();
                            if (isRoutable)
                            { // the arc is already a road.
                                if (graph.TagsIndex.Contains(arc.Value.Value.Tags))
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
                                    var keyDistance = stopLocation.DistanceEstimate(
                                        new GeoCoordinate(latitude, longitude)).Value;
                                    closestVertices[arc.Key] = keyDistance;
                                }
                                if (arc.Value.Key != stop &&
                                    graph.GetVertex(arc.Value.Key, out latitude, out longitude))
                                { // check distance.
                                    double keyDistance = stopLocation.DistanceEstimate(
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
                                if (sorted.Current.Value < MAX_ACCESS_POINT_DISTANCE.Value)
                                { // only attach stations that are relatively close.
                                    var closest = sorted.Current.Key;
                                    if (!graph.ContainsEdge(stop, closest))
                                    {
                                        graph.AddEdge(stop, closest, new LiveEdge(), null);
                                    }
                                    if (!graph.ContainsEdge(closest, stop))
                                    {
                                        graph.AddEdge(closest, stop, new LiveEdge(), null);
                                    }
                                }
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