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
        private readonly RouterDataSource<Edge> _graph;
        private readonly IRoutingInterpreter _interpreter;
        private readonly Vehicle _vehicle;
        private readonly PathSegmentVisitList _source;
        private readonly int _sourceMax;
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

        private Dictionary<long, DykstraVisit> _visits;
        private DykstraVisit _current;
        private BinaryHeap<DykstraVisit> _heap;
        private Speed _noSpeed = new Speed() { Direction = null, MeterPerSecond = 0 };
        private Dictionary<uint, Speed> _speeds;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // initialize stuff.
            this.Initialize();

            // start the search.
            while (this.Step()) { }
        }

        /// <summary>
        /// Initializes and resets.
        /// </summary>
        public void Initialize()
        {
            // algorithm always succeeds, it may be dealing with an empty network and there are no targets.
            this.HasSucceeded = true;

            // initialize a dictionary of speeds per profile.
            _speeds = new Dictionary<uint, Speed>();

            // intialize dykstra data structures.
            _visits = new Dictionary<long, DykstraVisit>();
            _heap = new BinaryHeap<DykstraVisit>(1000);

            // queue all source vertices.
            foreach (long vertex in _source.GetVertices())
            {
                var path = _source.GetPathTo(vertex);
                _heap.Push(new DykstraVisit(path), (float)path.Weight);
            }
        }

        /// <summary>
        /// Executes one step in the search.
        /// </summary>
        public bool Step()
        {
            // while the visit list is not empty.
            _current = null;
            if (_heap.Count > 0)
            { // choose the next vertex.
                _current = _heap.Pop();
                while (_current != null && _visits.ContainsKey(_current.Vertex))
                { // keep dequeuing.
                    if(_heap.Count == 0)
                    { // nothing more to pop.
                        break;
                    }
                    _current = _heap.Pop();
                }
            }

            if (_current != null)
            { // we visit this one, set visit.
                _visits[_current.Vertex] = _current;
            }
            else
            { // route is not found, there are no vertices left
                // or the search went outside of the max bounds.
                return false;
            }

            if (this.WasFound != null && !this.WasFound(_current.Vertex, _current.Weight))
            { // vertex was found and false was returned.
                return false;
            }

            // get neighbours.
            var edges = _graph.GetEdges(Convert.ToUInt32(_current.Vertex));

            while (edges.MoveNext())
            {
                var edge = edges;
                var neighbour = edge.Neighbour;

                if (_current.From == neighbour)
                { // don't go back!
                    continue;
                }

                if (_visits.ContainsKey(neighbour))
                { // has already been choosen.
                    continue;
                }

                // get the speed from cache or calculate.
                var edgeData = edge.EdgeData;
                var speed = _noSpeed;
                if (!_speeds.TryGetValue(edgeData.Tags, out speed))
                { // speed not there, calculate speed.
                    var tags = _graph.TagsIndex.Get(edgeData.Tags);
                    speed = _noSpeed;
                    if (_vehicle.CanTraverse(tags))
                    { // can traverse, speed not null!
                        speed = new Speed()
                        {
                            MeterPerSecond = ((OsmSharp.Units.Speed.MeterPerSecond)_vehicle.ProbableSpeed(tags)).Value,
                            Direction = _vehicle.IsOneWay(tags)
                        };
                    }
                    _speeds.Add(edgeData.Tags, speed);
                }

                // check the tags against the interpreter.
                if (speed.MeterPerSecond > 0 && (!speed.Direction.HasValue ||
                    (!_backward && speed.Direction.Value == edgeData.Forward) ||
                    (_backward && speed.Direction.Value != edgeData.Forward)))
                { // it's ok; the edge can be traversed by the given vehicle.
                    if (_current.From == 0 ||
                        (!_backward && _interpreter.CanBeTraversed(_current.From, _current.Vertex, neighbour)) ||
                        (_backward && _interpreter.CanBeTraversed(neighbour, _current.Vertex, _current.From)))
                    { // the neighbour is forward and is not settled yet!
                        // calculate neighbors weight.
                        var totalWeight = _current.Weight + (edgeData.Distance / speed.MeterPerSecond);

                        if (totalWeight < _sourceMax)
                        { // update the visit list.
                            var neighbourVisit = new DykstraVisit(neighbour, _current.Vertex, (float)totalWeight, edge.EdgeData, edge.Intermediates);
                            _heap.Push(neighbourVisit, neighbourVisit.Weight);
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <param name="vertex">The vertex that was visited.</param>
        /// <param name="visit">The visit data.</param>
        /// <returns></returns>
        public bool TryGetVisit(long vertex, out DykstraVisit visit)
        {
            return _visits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Returns the source visit list.
        /// </summary>
        public PathSegmentVisitList Source
        {
            get
            {
                return _source;
            }
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
        /// Gets the backward flag.
        /// </summary>
        public bool Backward
        {
            get
            {
                return _backward;
            }
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