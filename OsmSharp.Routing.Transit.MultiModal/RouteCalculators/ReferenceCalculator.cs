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

using OsmSharp.Logging;
using OsmSharp.Routing.Constraints;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Transit.Graphs;
using OsmSharp.Routing.Transit.MultiModal.PriorityQueues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Transit.MultiModal.RouteCalculators
{
    /// <summary>
    /// A simple implementation of a multimodal shortest route calculator.
    /// </summary>
    public class ReferenceCalculator : OsmSharp.Routing.Graph.Router.Dykstra.DykstraRoutingBase<LiveEdge>, IBasicRouter<LiveEdge>
    {
        /// <summary>
        /// Holds the maximum search time being 24h here.
        /// </summary>
        private const uint MAX_SEARCH_TIME = 25 * 60 * 60;

        /// <summary>
        /// Holds the minimum transfer time being 10 mins here.
        /// </summary>
        private const uint MIN_TRANSFER_TIME = 3 * 60;

        /// <summary>
        /// Holds the maximum transfer count.
        /// </summary>
        private const uint MAX_TRANSFER_COUNT = 10;

        /// <summary>
        /// Holds the transfer time penalty being 1 min here.
        /// </summary>
        private const uint TRANSFER_PENALTY = 0;//3 * 60;

        /// <summary>
        /// Holds the start time parameter key.
        /// </summary>
        public const string START_TIME_KEY = "start_time";

        /// <summary>
        /// Holds the is trip possible key.
        /// </summary>
        public const string IS_TRIP_POSSIBLE_KEY = "is_trip_possible";

        /// <summary>
        /// Holds key for the parameter defining the time between arriving and being able to tranfer to/from a bus/train/tram...
        /// </summary>
        public const string MODAL_TRANSFER_TIME_KEY = "modal_transfer_time";

        /// <summary>
        /// Holds the default intermodal transfer time being 1 min here.
        /// </summary>
        public const float MODAL_TRANSFER_TIME_DEFAULT = 1 * 60;

        /// <summary>
        /// Holds the schedules key.
        /// </summary>
        public const string SCHEDULES_KEY = "schedules";

        /// <summary>
        /// Holds the no-transit key.
        /// </summary>
        public const string NO_TRANSIT_KEY = "no_transit";

        /// <summary>
        /// Holds the max mode time key.
        /// </summary>
        public const string MAX_MODE_TIME = "max_mode_time";

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> Calculate(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList to, double max, Dictionary<string, object> parameters)
        {
            return this.CalculateToClosest(graph, interpreter, vehicle, from,
                new PathSegmentVisitList[] { to }, max, parameters);
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <param name="currentHandler"></param>
        /// <returns></returns>
        public PathSegment<VertexTimeAndTrip> CalculateAndTime(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList to, double max, Dictionary<string, object> parameters, Action<PathSegment<VertexTimeAndTrip>> currentHandler)
        {
            return this.CalculateToClosestAndTime(graph, interpreter, vehicle, from,
                new PathSegmentVisitList[] { to }, max, parameters, currentHandler);
        }

        /// <summary>
        /// Calculates the shortest path from all sources to all targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <param name="maxSearch"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long>[][] CalculateManyToMany(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double maxSearch, Dictionary<string, object> parameters)
        {
            var results = new PathSegment<long>[sources.Length][];
            for (int sourceIdx = 0; sourceIdx < sources.Length; sourceIdx++)
            {
                results[sourceIdx] = this.DoCalculation(graph, interpreter, vehicle,
                   sources[sourceIdx], targets, maxSearch, false, false, parameters).ConvertTo();
            }
            return results;
        }

        /// <summary>
        /// Calculates the shortest path from the given vertex to the given vertex given the weights in the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double CalculateWeight(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList from, PathSegmentVisitList to, double max, Dictionary<string, object> parameters)
        {
            var closest = this.CalculateToClosest(graph, interpreter, vehicle, from,
                new PathSegmentVisitList[] { to }, max, parameters);
            if (closest != null)
            {
                return closest.Weight;
            }
            return double.MaxValue;
        }

        /// <summary>
        /// Calculates a shortest path between the source vertex and any of the targets and returns the shortest.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public PathSegment<long> CalculateToClosest(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters)
        {
            var result = this.DoCalculation(graph, interpreter, vehicle,
                from, targets, max, false, false, parameters).ConvertTo();
            if (result != null && result.Length == 1)
            {
                return result[0];
            }
            return null;
        }

        /// <summary>
        /// Calculates a shortest path between the source vertex and any of the targets and returns the shortest.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <param name="currentHandler"></param>
        /// <returns></returns>
        public PathSegment<VertexTimeAndTrip> CalculateToClosestAndTime(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList from, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, Action<PathSegment<VertexTimeAndTrip>> currentHandler)
        {
            //parameters[NO_TRANSIT_KEY] = true;
            //var resultNoTransit = this.DoCalculation(graph, interpreter, vehicle,
            //    from, targets, max, false, false, parameters);
            //if (resultNoTransit != null && resultNoTransit.Length == 1)
            //{
            //    parameters[NO_TRANSIT_KEY] = false;
            //    max = System.Math.Min(max, resultNoTransit[0].Weight);
            // parameters[MAX_MODE_TIME] = (uint)(60 * 30);
                var result = this.DoCalculation(graph, interpreter, vehicle,
                    from, targets, max, false, false, parameters);
                if (result != null && result.Length == 1)
                {
                    return result[0];
                }
           // }
            return null;
        }

        /// <summary>
        /// Calculates all routes from a given source to all given targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double[] CalculateOneToManyWeight(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters)
        {
            var many = this.DoCalculation(graph, interpreter, vehicle,
                   source, targets, max, false, false, parameters).ConvertTo();

            var weights = new double[many.Length];
            for (int idx = 0; idx < many.Length; idx++)
            {
                if (many[idx] != null)
                {
                    weights[idx] = many[idx].Weight;
                }
                else
                {
                    weights[idx] = double.MaxValue;
                }
            }
            return weights;
        }

        /// <summary>
        /// Calculates all routes from a given sources to all given targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters)
        {
            var results = new double[sources.Length][];
            for (int idx = 0; idx < sources.Length; idx++)
            {
                results[idx] = this.CalculateOneToManyWeight(graph, interpreter, vehicle, sources[idx], targets, max, parameters);

                OsmSharp.Logging.Log.TraceEvent("DykstraRoutingLive", TraceEventType.Information, "Calculating weights... {0}%",
                    (int)(((float)idx / (float)sources.Length) * 100));
            }
            return results;
        }

        /// <summary>
        /// Returns true, range calculation is supported.
        /// </summary>
        public bool IsCalculateRangeSupported
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public HashSet<long> CalculateRange(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            return this.CalculateRange(graph, interpreter, vehicle, source, weight, true, parameters);
        }

        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="forward"></param>
        /// <param name="parameters"></param>
        public HashSet<long> CalculateRange(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, double weight, bool forward, Dictionary<string, object> parameters)
        {
            return this.CalculateRange(graph, interpreter, vehicle, source, weight, true, parameters, null);
        }

        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="forward"></param>
        /// <param name="parameters"></param>
        /// <param name="currentHandler"></param>
        /// <returns></returns>
        public HashSet<long> CalculateRange(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, double weight, bool forward, Dictionary<string, object> parameters, Action<PathSegment<VertexTimeAndTrip>> currentHandler)
        {
            var result = this.DoCalculation(graph, interpreter, vehicle,
                   source, new PathSegmentVisitList[0], weight, false, true, forward, parameters, currentHandler).ConvertTo();

            var resultVertices = new HashSet<long>();
            for (int idx = 0; idx < result.Length; idx++)
            {
                resultVertices.Add(result[idx].VertexId);
            }
            return resultVertices;
        }

        /// <summary>
        /// Returns true if the search can move beyond the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool CheckConnectivity(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters)
        {
            var range = this.CalculateRange(graph, interpreter, vehicle, source, weight, true, parameters);

            if (range.Count > 0)
            {
                range = this.CalculateRange(graph, interpreter, vehicle, source, weight, false, parameters);
                if (range.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        #region Algorithm Implementation


        /// <summary>
        /// Does forward dykstra calculation(s) with several options.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="weight"></param>
        /// <param name="stopAtFirst"></param>
        /// <param name="returnAtWeight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private PathSegment<VertexTimeAndTrip>[] DoCalculation(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList source, PathSegmentVisitList[] targets, double weight,
            bool stopAtFirst, bool returnAtWeight, Dictionary<string, object> parameters)
        {
            return this.DoCalculation(graph, interpreter, vehicle, source, targets, weight, stopAtFirst, returnAtWeight, true, parameters);
        }

        /// <summary>
        /// Does dykstra calculation(s) with several options.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="sourceList"></param>
        /// <param name="targetList"></param>
        /// <param name="weight"></param>
        /// <param name="stopAtFirst"></param>
        /// <param name="returnAtWeight"></param>
        /// <param name="forward"></param>
        /// <param name="parameters"></param>
        /// <param name="currentHandler"></param>
        /// <returns></returns>
        private PathSegment<VertexTimeAndTrip>[] DoCalculation(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList sourceList, PathSegmentVisitList[] targetList, double weight,
            bool stopAtFirst, bool returnAtWeight, bool forward, Dictionary<string, object> parameters, Action<PathSegment<VertexTimeAndTrip>> currentHandler = null)
        {
            // get parameters.
            Func<uint, DateTime, bool> isTripPossible = (x, y) => { return true; };
            if (parameters != null && parameters.ContainsKey(IS_TRIP_POSSIBLE_KEY))
            { // the is trip possible parameter is set.
                isTripPossible = (Func<uint, DateTime, bool>)parameters[IS_TRIP_POSSIBLE_KEY];
            }
            var startTime = DateTime.Now;
            if (parameters != null && parameters.ContainsKey(START_TIME_KEY))
            { // there is a start time parameter set.
                startTime = (DateTime)parameters[START_TIME_KEY];
            }
            var modalTransferTime = MODAL_TRANSFER_TIME_DEFAULT;
            if(parameters != null && parameters.ContainsKey(MODAL_TRANSFER_TIME_KEY))
            { // there is a modal transfer time set.
                modalTransferTime = (float)parameters[MODAL_TRANSFER_TIME_KEY];
            }
            var schedules = new List<TransitEdgeSchedulePair>();
            if (parameters != null && parameters.ContainsKey(SCHEDULES_KEY))
            { // there is a schedule parameter.
                schedules = (List<TransitEdgeSchedulePair>)parameters[SCHEDULES_KEY];
            }
            var maxTransferCount = MAX_TRANSFER_COUNT;
            var noTransit = false;
            if(parameters != null && parameters.ContainsKey(NO_TRANSIT_KEY))
            {
                noTransit = (bool)parameters[NO_TRANSIT_KEY];
            }

            var onlyTransit = false;
            if(vehicle == null)
            { // only transit, no vehicle given!
                onlyTransit = true;
            }

            // make sure the maximum search weight is set.
            if (weight > MAX_SEARCH_TIME)
            { // set to default max if greater.
                weight = MAX_SEARCH_TIME;
            }

            // get weight mode.
            var weightMode = weight;
            if(parameters != null && parameters.ContainsKey(MAX_MODE_TIME))
            {
                weightMode = (uint)parameters[MAX_MODE_TIME];
            }

            // instantiate possible trips data structures.
            var possibleTrips = new Dictionary<DateTime, Dictionary<uint, bool>>();
            Func<VertexTimeAndTrip, VertexTimeAndTrip, int> comparePaths = (vertex1, vertex2) =>
            {
                return vertex1.Vertex.CompareTo(vertex2.Vertex);
            };

            // make copies of the target and source visitlist.
            var source = sourceList.Clone() as PathSegmentVisitList;
            var targets = new PathSegmentVisitList[targetList.Length];
            var targetsCount = new int[targetList.Length];
            for (int targetIdx = 0; targetIdx < targetList.Length; targetIdx++)
            {
                targets[targetIdx] = targetList[targetIdx].Clone() as PathSegmentVisitList;
                targetsCount[targetIdx] = targetList[targetIdx].Count;
            }

            // expand the target lists by using a small backward search.
            this.DoBackwardSearch(graph, interpreter, vehicle, targetList, weight / 20);

            //  initialize the result data structures.
            var segmentsAtWeight = new List<PathSegment<VertexTimeAndTrip>>();
            var segmentsToTarget = new PathSegment<VertexTimeAndTrip>[targets.Length]; // the resulting target segments.

            // initialize pt-specific data structures.
            var chosenStations = new HashSet<long>(); // keep a list of stations that have been visited up until now, visited meaning foot on the ground.

            // intialize dykstra data structures.
            var heap = new ComparableBinairyHeap<PathSegment<VertexTimeAndTrip>, ModalWeight>();
            var chosenVertices = new DykstraVisitList();
            var labels = new Dictionary<VertexTimeAndTrip, IList<RoutingLabel>>();
            foreach (long vertex in source.GetVertices())
            {
                labels[new VertexTimeAndTrip((uint)vertex, 0)] = new List<RoutingLabel>();

                var path = source.GetPathTo(vertex).ConvertFrom();
                heap.Push(path, new ModalWeight((float)path.Weight));
            }

            // set the from node as the current node and put it in the correct data structures.
            // initialize the source's neighbors.
            var current = heap.Pop();
            while (current.Item != null &&
                chosenVertices.HasBeenVisited(current.Item))
            { // keep dequeuing.
                current = heap.Pop();
            }

            // test each target for the source.
            // test each source for any of the targets.
            var pathsFromSource = new Dictionary<long, PathSegment<VertexTimeAndTrip>>();
            foreach (long sourceVertex in source.GetVertices())
            { // get the path to the vertex.
                PathSegment<long> sourcePath = source.GetPathTo(sourceVertex); // get the source path.
                sourcePath = sourcePath.From;
                while (sourcePath != null)
                { // add the path to the paths from source.
                    pathsFromSource[sourcePath.VertexId] = sourcePath.ConvertFrom();
                    sourcePath = sourcePath.From;
                }
            }
            // loop over all targets
            for (int idx = 0; idx < targets.Length; idx++)
            { // check for each target if there are paths to the source.
                foreach (long targetVertex in new List<long>(targets[idx].GetVertices()))
                {
                    var targetPath = targets[idx].GetPathTo(targetVertex).ConvertFrom(); // get the target path.
                    targetPath = targetPath.From;
                    while (targetPath != null)
                    { // add the path to the paths from source.
                        PathSegment<VertexTimeAndTrip> pathFromSource;
                        if (pathsFromSource.TryGetValue(targetPath.VertexId.Vertex, out pathFromSource))
                        { // a path is found.
                            // get the existing path if any.
                            PathSegment<VertexTimeAndTrip> existing = segmentsToTarget[idx];
                            if (existing == null)
                            { // a path did not exist yet!
                                segmentsToTarget[idx] = targetPath.Reverse().ConcatenateAfter(pathFromSource, comparePaths);
                                targets[idx].Remove(targetVertex);
                            }
                            else if (existing.Weight > targetPath.Weight + pathFromSource.Weight)
                            { // a new path is found with a lower weight.
                                segmentsToTarget[idx] = targetPath.Reverse().ConcatenateAfter(pathFromSource, comparePaths);
                            }
                        }
                        targetPath = targetPath.From;
                    }
                }
            }
            if (targets.Length > 0 && targets.All(x => x.Count == 0))
            { // routing is finished!
                return segmentsToTarget.ToArray();
            }

            if (stopAtFirst)
            { // only one entry is needed.
                var oneFound = false;
                for (int idx = 0; idx < targets.Length; idx++)
                {
                    if (targets[idx].Count < targetsCount[idx])
                    {
                        oneFound = true;
                        break;
                    }
                }

                if (oneFound)
                { // targets found, return the shortest!
                    PathSegment<VertexTimeAndTrip> shortest = null;
                    foreach (var foundTarget in segmentsToTarget)
                    {
                        if (shortest == null)
                        {
                            shortest = foundTarget;
                        }
                        else if (foundTarget != null &&
                            shortest.Weight > foundTarget.Weight)
                        {
                            shortest = foundTarget;
                        }
                    }
                    segmentsToTarget = new PathSegment<VertexTimeAndTrip>[1];
                    segmentsToTarget[0] = shortest;
                    return segmentsToTarget;
                }
                else
                { // not targets found yet!
                    segmentsToTarget = new PathSegment<VertexTimeAndTrip>[1];
                }
            }

            // test for identical start/end point.
            for (int idx = 0; idx < targets.Length; idx++)
            {
                var target = targets[idx];
                if (returnAtWeight)
                { // add all the reached vertices larger than weight to the results.
                    if (current.Item.Weight > weight)
                    {
                        var toPath = target.GetPathTo(current.Item.VertexId.Vertex).ConvertFrom();
                        toPath.Reverse();
                        toPath = toPath.ConcatenateAfter(current.Item, comparePaths);
                        segmentsAtWeight.Add(toPath);
                    }
                }
                else if (target.Contains(current.Item.VertexId.Vertex))
                { // the current is a target!
                    var toPath = target.GetPathTo(current.Item.VertexId.Vertex).ConvertFrom();
                    toPath = toPath.Reverse();
                    toPath = toPath.ConcatenateAfter(current.Item, comparePaths);

                    if (stopAtFirst)
                    { // stop at the first occurrence.
                        segmentsToTarget[0] = toPath;
                        return segmentsToTarget;
                    }
                    else
                    { // normal one-to-many; add to the result.
                        // check if routing is finished.
                        if (segmentsToTarget[idx] == null)
                        { // make sure only the first route is set.
                            segmentsToTarget[idx] = toPath;
                            if (targets.All(x => x.Count == 0))
                            { // routing is finished!
                                return segmentsToTarget.ToArray();
                            }
                        }
                        else if (segmentsToTarget[idx].Weight > toPath.Weight)
                        { // check if the second, third or later is shorter.
                            segmentsToTarget[idx] = toPath;
                        }
                    }
                }
            }

            // start OsmSharp.Routing.
            var arcs = graph.GetEdges(Convert.ToUInt32(current.Item.VertexId.Vertex));
            chosenVertices.SetVisited(current.Item);

            // loop until target is found and the route is the shortest!
            while (true)
            {
                // get the current labels list (if needed).
                IList<RoutingLabel> currentLabels = null;
                if (interpreter.Constraints != null)
                { // there are constraints, get the labels.
                    currentLabels = labels[current.Item.VertexId];
                    labels.Remove(current.Item.VertexId);
                }

                // check turn-restrictions.
                List<uint[]> restrictions = null;
                bool isRestricted = false;
                if (current.Item.From != null &&
                    vehicle != null &&
                    graph.TryGetRestrictionAsStart(vehicle, (uint)current.Item.From.VertexId.Vertex, out restrictions))
                { // there are restrictions!
                    // search for a restriction that ends in the currently selected vertex.
                    for (int idx = 0; idx < restrictions.Count; idx++)
                    {
                        var restriction = restrictions[idx];
                        if (restriction[restriction.Length - 1] == current.Item.VertexId.Vertex)
                        { // oeps, do not consider the neighbours of this vertex.
                            isRestricted = true;
                            break;
                        }

                        for (int restrictedIdx = 0; restrictedIdx < restriction.Length; restrictedIdx++)
                        { // make sure the restricted vertices can be choosen multiple times.
                            // restrictedVertices.Add(restriction[restrictedIdx]);
                            chosenVertices.SetRestricted(restriction[restrictedIdx]);
                        }
                    }
                }

                // update the visited nodes.
                bool currentIsStation = false;
                if (!isRestricted)
                {
                    while (arcs.MoveNext())
                    // foreach (var neighbour in arcs)
                    {
                        var neighbour = arcs;
                        var neighbourKey = new VertexTimeAndTrip(neighbour.Neighbour, 0);
                        //if (chosenVertices.HasBeenVisited(neighbourKey, current.Item.VertexId))
                        //{ // this neighbour has already been visited.
                        //    continue;
                        //}
                        if (neighbour.EdgeData.IsRoad() && !onlyTransit)
                        { // a 'road' edge.
                            // prevent u-turns.
                            if (current.Item.From != null)
                            { // a possible u-turn.
                                if (current.Item.From.VertexId.Vertex == neighbour.Neighbour)
                                { // a u-turn, don't do this please!
                                    continue;
                                }
                            }

                            // check the tags against the interpreter.
                            var tags = graph.TagsIndex.Get(neighbour.EdgeData.Tags);
                            if (vehicle.CanTraverse(tags))
                            { // it's ok; the edge can be traversed by the given vehicle.
                                bool? oneWay = vehicle.IsOneWay(tags);
                                bool canBeTraversedOneWay = (!oneWay.HasValue || oneWay.Value == neighbour.EdgeData.Forward);
                                if ((current.Item.From == null ||
                                    interpreter.CanBeTraversed(current.Item.From.VertexId.Vertex, current.Item.VertexId.Vertex, neighbour.Neighbour)) && // test for turning restrictions.
                                    canBeTraversedOneWay)
                                { // the neighbor is forward and is not settled yet!
                                    bool restrictionsOk = true;
                                    if (restrictions != null)
                                    { // search for a restriction that ends in the currently selected neighbour and check if it's via-vertex matches.
                                        for (int idx = 0; idx < restrictions.Count; idx++)
                                        {
                                            var restriction = restrictions[idx];
                                            if (restriction[restriction.Length - 1] == neighbour.Neighbour)
                                            { // oeps, do not consider the neighbours of this vertex.
                                                if (restriction[restriction.Length - 2] == current.Item.VertexId.Vertex)
                                                { // damn this route-part is restricted!
                                                    restrictionsOk = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // check the labels (if needed).
                                    bool constraintsOk = true;
                                    if (restrictionsOk && interpreter.Constraints != null)
                                    { // check if the label is ok.
                                        RoutingLabel neighbourLabel = interpreter.Constraints.GetLabelFor(
                                            graph.TagsIndex.Get(neighbour.EdgeData.Tags));

                                        // only test labels if there is a change.
                                        if (currentLabels.Count == 0 || !neighbourLabel.Equals(currentLabels[currentLabels.Count - 1]))
                                        { // labels are different, test them!
                                            constraintsOk = interpreter.Constraints.ForwardSequenceAllowed(currentLabels,
                                                neighbourLabel);

                                            if (constraintsOk)
                                            { // update the labels.
                                                var neighbourLabels = new List<RoutingLabel>(currentLabels);
                                                neighbourLabels.Add(neighbourLabel);

                                                labels[neighbourKey] = neighbourLabels;
                                            }
                                        }
                                        else
                                        { // set the same label(s).
                                            labels[neighbourKey] = currentLabels;
                                        }
                                    }

                                    if (constraintsOk && restrictionsOk)
                                    { // all constraints are validated or there are none.
                                        // calculate neighbors weight.
                                        double relativeWeight = vehicle.Weight(tags, neighbour.EdgeData.Distance);
                                        double totalWeight = current.Item.Weight + relativeWeight;
                                        double totalWeightWithoutWaiting = current.Weight.TimeWithoutWaiting + relativeWeight;
                                        uint secondsMode = current.Item.VertexId.SecondsMode + (uint)relativeWeight;

                                        // update the visit list.
                                        if (returnAtWeight || (totalWeight < weight &&
                                            secondsMode < weightMode))
                                        {
                                            // create new vertex and time containing secondsMode.
                                            neighbourKey = new VertexTimeAndTrip(neighbour.Neighbour, secondsMode);

                                            var neighbourRoute = new PathSegment<VertexTimeAndTrip>(neighbourKey, totalWeight, current.Item);
                                            heap.Push(neighbourRoute, new ModalWeight((float)neighbourRoute.Weight, (float)totalWeightWithoutWaiting, current.Weight.Transfers));
                                        }
                                    }
                                }
                            }
                        }
                        else if (neighbour.EdgeData.IsTransit() && !noTransit)
                        { // transit edge.
                            // calculate ticks.
                            var currentTicks = startTime.AddSeconds(current.Item.Weight).Ticks;
                            var ticksDate = new DateTime(currentTicks);
                            TransitEdgeSchedule forwardSchedule = null;
                            if(neighbour.EdgeData.Forward)
                            { // edge is forward, use forward schedule.
                                forwardSchedule = neighbour.EdgeData.GetForwardSchedule(schedules);
                            }
                            else
                            { // edge is backward, use backward schedule as forward.
                                forwardSchedule = neighbour.EdgeData.GetBackwardSchedule(schedules);
                            }

                            if (current.Item.VertexId.Seconds == 0 &&
                                current.Item.VertexId.Trip == 0)
                            { // current vertex is a station (because it has a transit edge) but it does not have a trip.
                                currentIsStation = true;
                                if (current.Weight.Transfers >= maxTransferCount)
                                { // reached maxim tranfers, do no transfer anymore please!
                                    continue;
                                }

                                if (chosenStations.Contains(current.Item.VertexId.Vertex))
                                {
                                    continue;
                                }

                                // add all points at this station at a time later than the current timestamp.
                                var entriesAfter = forwardSchedule.GetAfter(ticksDate);
                                // ALL TRANSFERS
                                foreach (var entry in entriesAfter)
                                {
                                    var seconds = entry.DepartsIn(ticksDate);
                                    uint secondsNeighbour = (uint)(seconds + current.Item.Weight);
                                    if (secondsNeighbour + entry.Duration < weight)
                                    { // departure + duration still not over the search threshold.
                                        Dictionary<uint, bool> tripsForDate;
                                        if (!possibleTrips.TryGetValue(ticksDate.Date, out tripsForDate))
                                        {
                                            tripsForDate = new Dictionary<uint, bool>();
                                            possibleTrips.Add(ticksDate.Date, tripsForDate);
                                        }
                                        bool isTripPossibleResult;
                                        if (!tripsForDate.TryGetValue(entry.Trip, out isTripPossibleResult))
                                        {
                                            isTripPossibleResult = isTripPossible.Invoke(entry.Trip, startTime.AddSeconds(secondsNeighbour));
                                            tripsForDate.Add(entry.Trip, isTripPossibleResult);
                                        }
                                        if (isTripPossibleResult)
                                        { // ok trip is possible.
                                            var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(current.Item.VertexId.Vertex, secondsNeighbour, entry.Trip), secondsNeighbour, current.Item);
                                            heap.Push(path, new ModalWeight(secondsNeighbour + TRANSFER_PENALTY, current.Weight.TimeWithoutWaiting,  current.Weight.Transfers + 1));
                                        }
                                    }
                                }
                            }
                            else
                            { // current vertex is a station (because it has a transit edge) and it has a current trip.
                                // MOVE TO NEXT STATION.
                                // NO TRANSFER: find the next entry along the same trip.
                                var entry = forwardSchedule.GetForTrip(current.Item.VertexId.Trip, ticksDate);
                                if (entry.HasValue)
                                { // there is a next entry along the same trip.
                                    var seconds = entry.Value.DepartsIn(ticksDate);
                                    uint secondsNeighbour = (uint)(seconds + current.Item.Weight + entry.Value.Duration);
                                    float secondsWithoutWaiting = seconds + current.Weight.TimeWithoutWaiting + entry.Value.Duration;
                                    if (secondsNeighbour < weight)
                                    { // still not over the search threshold.
                                        var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(neighbour.Neighbour, secondsNeighbour, entry.Value.Trip), secondsNeighbour, current.Item);
                                        heap.Push(path, new ModalWeight(secondsNeighbour, secondsWithoutWaiting, current.Weight.Transfers));
                                    }
                                }

                                // STAY IN THE SAME STATION.
                                // TRANSFER: find the first entry to transfer to in the same station.
                                if (current.Weight.Transfers >= maxTransferCount)
                                { // reached maxim tranfers, do no transfer anymore please!
                                    continue;
                                }
                                if (current.Item.From != null &&
                                    current.Item.From.VertexId.Vertex != current.Item.VertexId.Vertex)
                                { // the previous station was a different station, this means leaving the current trip.
                                    if (chosenStations.Contains(current.Item.VertexId.Vertex))
                                    { // a 'foot-on-the-ground' already exists at this station, no use leaving the current trip.
                                        // a 'foot-on-the-ground' can only be created once at every station.
                                        continue;
                                    }
                                    chosenStations.Add(current.Item.VertexId.Vertex); // 'foot-on-the-ground' in this station already.
                                }
                                var minTransferTime = MIN_TRANSFER_TIME;
                                var transfers = current.Weight.Transfers + 1;
                                if (neighbour.EdgeData.Forward)
                                { // edge is forward, use forward schedule.
                                    forwardSchedule = neighbour.EdgeData.GetForwardSchedule(schedules);
                                }
                                else
                                { // edge is backward, use backward schedule as forward.
                                    forwardSchedule = neighbour.EdgeData.GetBackwardSchedule(schedules);
                                }
                                entry = forwardSchedule.GetNext(ticksDate.AddSeconds(minTransferTime), current.Item.VertexId.Trip);
                                if (entry.HasValue)
                                { // there is a next entry in the same station.
                                    var seconds = entry.Value.DepartsIn(ticksDate);
                                    uint secondsNeighbour = (uint)(seconds + current.Item.Weight);
                                    if (secondsNeighbour + entry.Value.Duration < weight)
                                    { // departure + duration still not over the search threshold.
                                        Dictionary<uint, bool> tripsForDate;
                                        if (!possibleTrips.TryGetValue(ticksDate.Date, out tripsForDate))
                                        {
                                            tripsForDate = new Dictionary<uint, bool>();
                                            possibleTrips.Add(ticksDate.Date, tripsForDate);
                                        }
                                        bool isTripPossibleResult;
                                        if (!tripsForDate.TryGetValue(entry.Value.Trip, out isTripPossibleResult))
                                        {
                                            isTripPossibleResult = isTripPossible.Invoke(entry.Value.Trip, startTime.AddSeconds(secondsNeighbour));
                                            tripsForDate.Add(entry.Value.Trip, isTripPossibleResult);
                                        }
                                        if (isTripPossibleResult)
                                        { // ok trip is possible.
                                            var path = new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(current.Item.VertexId.Vertex, secondsNeighbour, entry.Value.Trip), secondsNeighbour, current.Item);
                                            heap.Push(path, new ModalWeight(secondsNeighbour, current.Weight.TimeWithoutWaiting, transfers));
                                        }
                                    }
                                }
                            }
                        }
                        else if (!neighbour.EdgeData.IsTransit() && !neighbour.EdgeData.IsRoad())
                        { // no transit no road, just a connection between modes.
                            var secondsNeighbour = current.Weight.Time + modalTransferTime;
                            if (secondsNeighbour < weight)
                            {
                                var neighbourRoute = new PathSegment<VertexTimeAndTrip>(neighbourKey, secondsNeighbour, current.Item);
                                heap.Push(neighbourRoute, new ModalWeight((float)secondsNeighbour, current.Weight.TimeWithoutWaiting, current.Weight.Transfers));
                            }
                        }
                    }
                }

                // add to stations list if needed.
                if(currentIsStation)
                {
                    chosenStations.Add(current.Item.VertexId.Vertex);
                }

                // while the visit list is not empty.
                current = null;
                if (heap.Count > 0)
                { // choose the next vertex.
                    current = heap.Pop();
                    while (current != null &&
                        chosenVertices.HasBeenVisited(current.Item))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                    if (current != null)
                    {
                        chosenVertices.SetVisited(current.Item);
                    }
                }
                while (current != null && current.Item.Weight > weight)
                {
                    if (returnAtWeight)
                    { // add all the reached vertices larger than weight to the results.
                        segmentsAtWeight.Add(current.Item);
                    }

                    // choose the next vertex if the weight has been reached.
                    current = heap.Pop();
                    while (current != null &&
                        chosenVertices.HasBeenVisited(current.Item))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                }

                if (current == null)
                { // route is not found, there are no vertices left
                    // or the search went outside of the max bounds.
                    break;
                }

                // report current if needed.
                if (currentHandler != null)
                {
                    currentHandler.Invoke(current.Item);
                }

                // check target.
                for (int idx = 0; idx < targets.Length; idx++)
                {
                    var target = targets[idx];
                    if (target.Contains(current.Item.VertexId.Vertex))
                    { // the current is a target!
                        var toPath = target.GetPathTo(current.Item.VertexId.Vertex).ConvertFrom();
                        toPath = toPath.Reverse();
                        toPath = toPath.ConcatenateAfter(current.Item, comparePaths);

                        if (stopAtFirst)
                        { // stop at the first occurrence.
                            segmentsToTarget[0] = toPath;
                            return segmentsToTarget;
                        }
                        else
                        { // normal one-to-many; add to the result.
                            // check if routing is finished.
                            if (segmentsToTarget[idx] == null)
                            { // make sure only the first route is set.
                                segmentsToTarget[idx] = toPath;
                            }
                            else if (segmentsToTarget[idx].Weight > toPath.Weight)
                            { // check if the second, third or later is shorter.
                                segmentsToTarget[idx] = toPath;
                            }

                            // remove this vertex from this target's paths.
                            target.Remove(current.Item.VertexId.Vertex);

                            // if this target is empty it's optimal route has been found.
                            if (target.Count == 0)
                            { // now the shortest route has been found for sure!
                                if (targets.All(x => x.Count == 0))
                                { // routing is finished!
                                    return segmentsToTarget.ToArray();
                                }
                            }
                        }
                    }
                }

                // get the neighbors of the current node.
                arcs = graph.GetEdges(Convert.ToUInt32(current.Item.VertexId.Vertex));
            }

            // return the result.
            if (!returnAtWeight)
            {
                return segmentsToTarget.ToArray();
            }
            return segmentsAtWeight.ToArray();
        }

        /// <summary>
        /// Does a backward search for all the given targets.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="targetList"></param>
        /// <param name="maxWeight"></param>
        /// <returns>All the vertices that were chosen.</returns>
        private HashSet<long>[] DoBackwardSearch(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle, PathSegmentVisitList[] targetList, double maxWeight)
        {
            var sets = new HashSet<long>[targetList.Length];
            for(int idx = 0; idx < targetList.Length; idx++)
            {
                sets[idx] = this.DoBackwardSearch(graph, interpreter, vehicle, targetList[idx], maxWeight);
            }
            return sets;
        }

        /// <summary>
        /// Does a backward search for the given target.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="target"></param>
        /// <param name="maxWeight"></param>
        /// <returns>All the vertices that were chosen.</returns>
        private HashSet<long> DoBackwardSearch(IBasicRouterDataSource<LiveEdge> graph, IRoutingInterpreter interpreter, Vehicle vehicle, PathSegmentVisitList target, double maxWeight)
        {
            // intialize dykstra data structures.
            var heap = new OsmSharp.Collections.PriorityQueues.BinairyHeap<PathSegment<long>>();
            var chosenVertices = new HashSet<long>();
            var labels = new Dictionary<long, IList<RoutingLabel>>();
            foreach (long vertex in target.GetVertices())
            {
                labels[vertex] = new List<RoutingLabel>();

                var path = target.GetPathTo(vertex);
                heap.Push(path, (float)path.Weight);
            }

            // set the from node as the current node and put it in the correct data structures.
            // initialize the source's neighbors.
            var current = heap.Pop();
            while (current != null &&
                chosenVertices.Contains(current.VertexId))
            { // keep dequeuing.
                current = heap.Pop();
            }

            // start OsmSharp.Routing.
            var arcs = graph.GetEdges(Convert.ToUInt32(current.VertexId));
            chosenVertices.Add(current.VertexId);

            // loop until target is found and the route is the shortest!
            while (true)
            {
                // add the current path to the visit list.
                target.UpdateVertex(current);

                // get the current labels list (if needed).
                IList<RoutingLabel> currentLabels = null;
                if (interpreter.Constraints != null)
                { // there are constraints, get the labels.
                    currentLabels = labels[current.VertexId];
                    labels.Remove(current.VertexId);
                }

                // update the visited nodes.
                foreach (var neighbour in arcs)
                {
                    if (chosenVertices.Contains(neighbour.Neighbour))
                    { // this neighbour has already been visited.
                        continue;
                    }
                    if (neighbour.EdgeData.IsRoad())
                    { // a 'road' edge.
                        // check the tags against the interpreter.
                        var tags = graph.TagsIndex.Get(neighbour.EdgeData.Tags);
                        if (vehicle.CanTraverse(tags))
                        { // it's ok; the edge can be traversed by the given vehicle.
                            bool? oneWay = vehicle.IsOneWay(tags);
                            bool canBeTraversedOneWay = (!oneWay.HasValue || oneWay.Value != neighbour.EdgeData.Forward); // BACKWARD!
                            if ((current.From == null ||
                                interpreter.CanBeTraversed(current.VertexId, current.From.VertexId, neighbour.Neighbour)) && // test for turning restrictions.
                                canBeTraversedOneWay)
                            { // the neighbor is forward and is not settled yet!
                                // check the labels (if needed).
                                bool constraintsOk = true;
                                if (interpreter.Constraints != null)
                                { // check if the label is ok.
                                    RoutingLabel neighbourLabel = interpreter.Constraints.GetLabelFor(
                                        graph.TagsIndex.Get(neighbour.EdgeData.Tags));

                                    // only test labels if there is a change.
                                    if (currentLabels.Count == 0 || !neighbourLabel.Equals(currentLabels[currentLabels.Count - 1]))
                                    { // labels are different, test them!
                                        constraintsOk = interpreter.Constraints.ForwardSequenceAllowed(currentLabels,
                                            neighbourLabel);

                                        if (constraintsOk)
                                        { // update the labels.
                                            var neighbourLabels = new List<RoutingLabel>(currentLabels);
                                            neighbourLabels.Add(neighbourLabel);

                                            labels[neighbour.Neighbour] = neighbourLabels;
                                        }
                                    }
                                    else
                                    { // set the same label(s).
                                        labels[neighbour.Neighbour] = currentLabels;
                                    }
                                }

                                if (constraintsOk)
                                { // all constraints are validated or there are none.
                                    // calculate neighbors weight.
                                    double relativeWeight = vehicle.Weight(tags, neighbour.EdgeData.Distance);
                                    double totalWeight = current.Weight + relativeWeight;

                                    // update the visit list.
                                    if (totalWeight < maxWeight)
                                    {
                                        var neighbourRoute = new PathSegment<long>(neighbour.Neighbour, totalWeight, current);
                                        heap.Push(neighbourRoute, (float)totalWeight);
                                    }
                                }
                            }
                        }
                    }
                }

                // while the visit list is not empty.
                current = null;
                if (heap.Count > 0)
                { // choose the next vertex.
                    current = heap.Pop();
                    while (current != null &&
                        chosenVertices.Contains(current.VertexId))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                    if (current != null)
                    {
                        chosenVertices.Add(current.VertexId);
                    }
                }

                if (current == null)
                { // route is not found, there are no vertices left
                    // or the search went outside of the max bounds.
                    break;
                }

                arcs = graph.GetEdges(Convert.ToUInt32(current.VertexId));
            }
            return chosenVertices;
        }

        #endregion

        /// <summary>
        /// Returns the unit of the weight type used in this router implementation.
        /// </summary>
        public RouterWeightType WeightType
        {
            get { return RouterWeightType.Time; }
        }
    }
}