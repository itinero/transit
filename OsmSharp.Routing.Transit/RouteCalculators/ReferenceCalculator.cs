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

using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Transit.Graphs;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.RouteCalculators
{
    /// <summary>
    /// An unoptimized version of a transit route calculator.
    /// </summary>
    public class ReferenceCalculator : IRouteCalculator
    {
        /// <summary>
        /// Holds the maximum search time being 24h here.
        /// </summary>
        private const uint MAX_SEARCH_TIME = 24 * 60 * 60;

        /// <summary>
        /// Holds the minimum transfer time being 2 mins here.
        /// </summary>
        private const uint MIN_TRANSFER_TIME = 10 * 60;

        /// <summary>
        /// Holds the transfer time penalty being 1 min here.
        /// </summary>
        private const uint TRANSFER_PENALTY = 60;

        /// <summary>
        /// Calculates a transit route between the two stops.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public PathSegment<VertexTimeAndTrip> Calculate(IDynamicGraphReadOnly<TransitEdge> graph, uint from, uint to, DateTime startTime, Func<uint, DateTime, bool> isTripPossible)
        {
            // initialize dykstra heap and visited set.
            var heap = new BinairyHeap<PathSegment<VertexTimeAndTrip>>(100);
            var visited = new HashSet<VertexTimeAndTrip>();
            var possibleTrips = new Dictionary<DateTime, Dictionary<uint, bool>>();

            // push all neighbours of first vertex.
            long currentTicks = startTime.Ticks; // calculate ticks.

            // get 'neighbours'.
            var arcs = graph.GetArcs(from);
            var ticksDate = new DateTime(currentTicks);
            foreach (var arc in arcs)
            {
                foreach (var entry in arc.Value.ForwardSchedule.GetAfter(ticksDate))
                {
                    var seconds = (uint)entry.DepartsIn(ticksDate);
                    if (isTripPossible.Invoke(entry.Trip, startTime.AddSeconds(seconds)))
                    { //ok trip is possible.
                        var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(from, seconds, entry.Trip));
                        heap.Push(path, seconds);
                    }
                }
            }
         
            while(heap.Count > 0)
            { // keep going until there are no more vertices to search.
                var current = heap.Pop();

                // check if we already had this one.
                if (!visited.Contains(current.VertexId))
                { // ok, we have not had this one, but keep it for next time.
                    visited.Add(current.VertexId);

                    if (current.VertexId.Vertex == to)
                    { // stop search at first alternative?
                        return current;
                    }

                    // calculate ticks.
                    currentTicks = startTime.AddSeconds(current.VertexId.Seconds).Ticks;

                    // get 'neighbours'.
                    arcs = graph.GetArcs((uint)current.VertexId.Vertex);
                    ticksDate = new DateTime(currentTicks);
                    foreach (var arc in arcs)
                    {
                        // find the next entry along the same trip.
                        var entry = arc.Value.ForwardSchedule.GetForTrip(current.VertexId.Trip, ticksDate);
                        if (entry.HasValue)
                        { // there is a next entry along the same trip.
                            var seconds = entry.Value.DepartsIn(ticksDate);
                            uint secondsNeighbour = (uint)(seconds + current.VertexId.Seconds + entry.Value.Duration);
                            if (secondsNeighbour < MAX_SEARCH_TIME)
                            { // still not over the search threshold.
                                var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(arc.Key, secondsNeighbour, entry.Value.Trip), secondsNeighbour, current);
                                heap.Push(path, secondsNeighbour);
                            }
                        }

                        // find the first entry to transfer to in the same station.
                        var minTransferTime = MIN_TRANSFER_TIME;
                        if(current.From == null || current.VertexId.Trip != current.From.VertexId.Trip)
                        { // the current vertex has not previous or previous vertex is already a transfer.
                            minTransferTime = 0; // allow a transfer time of zero.
                        }
                        entry = arc.Value.ForwardSchedule.GetNext(ticksDate.AddSeconds(minTransferTime), current.VertexId.Trip);
                        if (entry.HasValue)
                        { // there is a next entry in the same station.
                            var seconds = entry.Value.DepartsIn(ticksDate);
                            uint secondsNeighbour = (uint)(seconds + current.VertexId.Seconds);
                            if (secondsNeighbour < MAX_SEARCH_TIME)
                            { // still not over the search threshold.
                                Dictionary<uint, bool> tripsForDate;
                                if(!possibleTrips.TryGetValue(ticksDate.Date, out tripsForDate))
                                {
                                    tripsForDate = new Dictionary<uint, bool>();
                                    possibleTrips.Add(ticksDate.Date, tripsForDate);
                                }
                                bool isTripPossibleResult;
                                if(!tripsForDate.TryGetValue(entry.Value.Trip, out isTripPossibleResult))
                                {
                                    isTripPossibleResult = isTripPossible.Invoke(entry.Value.Trip, startTime.AddSeconds(secondsNeighbour));
                                    tripsForDate.Add(entry.Value.Trip, isTripPossibleResult);
                                }
                                if (isTripPossibleResult)
                                { // ok trip is possible.
                                    var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(current.VertexId.Vertex, secondsNeighbour, entry.Value.Trip), secondsNeighbour, current);
                                    heap.Push(path, secondsNeighbour + TRANSFER_PENALTY);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}