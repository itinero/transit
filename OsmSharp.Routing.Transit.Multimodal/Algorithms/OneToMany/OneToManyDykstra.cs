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

using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Transit.Algorithms.OneToOne;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToOne;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany
{
    /// <summary>
    /// An algorithm that calculates one-to-many paths between on source and all targets within a certain range.
    /// </summary>
    public class OneToManyDykstra : RoutingAlgorithmBase, IDykstraAlgorithm
    {
        /// <summary>
        /// Holds the routing graph.
        /// </summary>
        private readonly RouterDataSource<Edge> _graph;

        /// <summary>
        /// Hold the routing intepreter.
        /// </summary>
        private readonly IRoutingInterpreter _interpreter;

        /// <summary>
        /// Holds the source vehicle.
        /// </summary>
        private readonly Vehicle _vehicle;

        /// <summary>
        /// Holds the source location.
        /// </summary>
        private readonly PathSegmentVisitList _source;

        /// <summary>
        /// Holds the maximum seconds for the source vehicle.
        /// </summary>
        private readonly int _sourceMax;

        /// <summary>
        /// Holds the backward flag.
        /// </summary>
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-many dykstra algorithm instance.
        /// </summary>
        /// <param name="graph">The routing graph.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicle">The vehicle at the source.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceMax">The maximum seconds for the source vehicle to travel.</param>
        /// <param name="backward">The backward flag, search is backwards when true.</param>
        /// <param name="wasFound">The function called when a vertex has been reached.</param>
        public OneToManyDykstra(RouterDataSource<Edge> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, int sourceMax, bool backward)
        {
            _graph = graph;
            _interpreter = interpreter;
            _vehicle = vehicle;
            _source = source;
            _sourceMax = sourceMax;
            _backward = backward;
        }

        /// <summary>
        /// Holds all visits made during the search.
        /// </summary>
        private Dictionary<long, DykstraVisit> _visits;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // algorithm always succeeds, it may be dealing with an empty network and there are no targets.
            this.HasSucceeded = true;

            // initialize a dictionary of speeds per profile.
            var speeds = new Dictionary<uint, Speed>();
            var noSpeed = new Speed() { Direction = null, MeterPerSecond = 0 };

            // intialize dykstra data structures.
            _visits = new Dictionary<long, DykstraVisit>();
            var heap = new BinaryHeap<DykstraVisit>(1000);

            // queue all source vertices.
            foreach (long vertex in _source.GetVertices())
            {
                var path = _source.GetPathTo(vertex);
                heap.Push(new DykstraVisit(path), (float)path.Weight);
            }

            // start the search.
            var current = heap.Pop();
            while(true)
            {
                if (current != null)
                { // we visit this one, set visit.
                    _visits[current.Vertex] = current;
                }
                else
                { // route is not found, there are no vertices left
                    // or the search went outside of the max bounds.
                    break;
                }

                if (this.WasFound != null && !this.WasFound(current.Vertex, current.Weight))
                { // vertex was found and false was returned.
                    return;
                }

                // get neighbours.
                var edges = _graph.GetEdges(Convert.ToUInt32(current.Vertex));

                while(edges.MoveNext())
                {
                    var edge = edges;
                    var neighbour = edge.Neighbour;

                    if (current.From == neighbour)
                    { // don't go back!
                        continue;
                    }

                    if (_visits.ContainsKey(neighbour))
                    { // has already been choosen.
                        continue;
                    }

                    // get the speed from cache or calculate.
                    var edgeData = edge.EdgeData;
                    var speed = noSpeed;
                    if (!speeds.TryGetValue(edgeData.Tags, out speed))
                    { // speed not there, calculate speed.
                        var tags = _graph.TagsIndex.Get(edgeData.Tags);
                        speed = noSpeed;
                        if (_vehicle.CanTraverse(tags))
                        { // can traverse, speed not null!
                            speed = new Speed()
                            {
                                MeterPerSecond = ((OsmSharp.Units.Speed.MeterPerSecond)_vehicle.ProbableSpeed(tags)).Value,
                                Direction = _vehicle.IsOneWay(tags)
                            };
                        }
                        speeds.Add(edgeData.Tags, speed);
                    }

                    // check the tags against the interpreter.
                    if (speed.MeterPerSecond > 0 && (!speed.Direction.HasValue || 
                        (!_backward && speed.Direction.Value == edgeData.Forward) || 
                        (_backward && speed.Direction.Value != edgeData.Forward)))
                    { // it's ok; the edge can be traversed by the given vehicle.
                        if (current.From == 0 || 
                            (!_backward && _interpreter.CanBeTraversed(current.From, current.Vertex, neighbour)) ||
                            (_backward && _interpreter.CanBeTraversed(neighbour, current.Vertex, current.From)))
                        { // the neighbour is forward and is not settled yet!
                            // calculate neighbors weight.
                            var totalWeight = current.Weight + (edgeData.Distance / speed.MeterPerSecond);

                            if (totalWeight < _sourceMax)
                            { // update the visit list.
                                var neighbourVisit = new DykstraVisit(neighbour, current.Vertex, (float)totalWeight, edge.EdgeData);
                                heap.Push(neighbourVisit, neighbourVisit.Weight);
                            }
                        }
                    }
                }

                // while the visit list is not empty.
                current = null;
                if (heap.Count > 0)
                { // choose the next vertex.
                    current = heap.Pop();
                    while (current != null && _visits.ContainsKey(current.Vertex))
                    { // keep dequeuing.
                        current = heap.Pop();
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <param name="vertex">The vertex that was visited.</param>
        /// <param name="visit">The visit data.</param>
        /// <returns></returns>
        public bool TryGetVisit(long vertex, out DykstraVisit visit)
        {
            this.CheckHasRunAndHasSucceeded();

            return _visits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Func<long, float, bool> WasFound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the vehicle.
        /// </summary>
        public Vehicle Vehicle
        {
            get
            {
                return _vehicle;
            }
        }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        public RouterDataSource<Edge> Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Represents speed.
        /// </summary>
        private struct Speed
        {
            /// <summary>
            /// Gets or sets the meters per second.
            /// </summary>
            public double MeterPerSecond { get; set; }

            /// <summary>
            /// Gets or sets the direction.
            /// </summary>
            public bool? Direction { get; set; }
        }
    }
}