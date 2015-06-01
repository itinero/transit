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

using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Units.Speed;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany
{
    /// <summary>
    /// A class reponsable for building one of the routes obtained by a dykstra routing algorithm.
    /// </summary>
    public class OneToManyDykstraRouteBuilder : OneToOneRouteBuilder<IDykstraAlgorithm>
    {
        private readonly uint _target;
        private readonly RouterDataSource<Edge> _graph;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-many dykstra routing algorithm.
        /// </summary>
        public OneToManyDykstraRouteBuilder(RouterDataSource<Edge> graph, IDykstraAlgorithm algorithm, uint target)
            : this(graph, algorithm, target, false)
        {

        }

        /// <summary>
        /// Creates a new one-to-many dykstra routing algorithm.
        /// </summary>
        public OneToManyDykstraRouteBuilder(RouterDataSource<Edge> graph, IDykstraAlgorithm algorithm, uint target, bool backward)
            : base(algorithm)
        {
            _graph = graph;
            _target = target;
            _backward = backward;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        /// <returns></returns>
        public override Route DoBuild()
        {
            this.CheckHasRunAndSucceeded();

            var path = new List<Tuple<uint, DykstraVisit>>();
            var vertex = _target;
            DykstraVisit visit;
            while (this.Algorithm.TryGetVisit(vertex, out visit))
            {
                path.Insert(0, new Tuple<uint, DykstraVisit>(vertex, visit));
                vertex = (uint)visit.From;
            }

            var vehicle = this.Algorithm.Vehicle;
            var time = 0.0;
            var distance = 0.0;
            var route = new Route();
            route.Vehicle = vehicle.UniqueName;
            route.Segments = new RouteSegment[path.Count];
            if (!_backward)
            { // build forward route.
                for (var i = 0; i < path.Count; i++)
                {
                    visit = path[i].Item2;
                    if (visit != null && visit.From != 0)
                    { // visit is there.
                        var tags = _graph.TagsIndex.Get(visit.Edge.Tags);
                        time = time + (visit.Edge.Distance / ((MeterPerSecond)vehicle.ProbableSpeed(tags)).Value);
                        distance = distance + visit.Edge.Distance;
                    }
                    float latitude, longitude;
                    _graph.GetVertex(path[i].Item1, out latitude, out longitude);
                    route.Segments[i] = new RouteSegment()
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        Type = i > 0 ? RouteSegmentType.Start : RouteSegmentType.Along,
                        Time = time,
                        Distance = distance
                    };
                }
            }
            else
            { // build backward route.
                visit = default(DykstraVisit);
                for(var i = path.Count - 1; i >= 0; i--)
                {
                    TagsCollectionBase tags = null;
                    if (visit != null)
                    { // visit is there.
                        tags = _graph.TagsIndex.Get(visit.Edge.Tags);
                        time = time + (visit.Edge.Distance / ((MeterPerSecond)vehicle.ProbableSpeed(tags)).Value);
                        distance = distance + visit.Edge.Distance;
                    }
                    float latitude, longitude;
                    _graph.GetVertex(path[i].Item1, out latitude, out longitude);
                    route.Segments[path.Count - 1 - i] = new RouteSegment()
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        Type = i > 0 ? RouteSegmentType.Start : RouteSegmentType.Along,
                        Time = time,
                        Distance = distance,
                        Tags = tags != null ? tags.ConvertFrom() : null
                    };
                    visit = path[i].Item2;
                }
            }
            
            if (route.Segments.Length > 0)
            {
                route.Segments[0].Type = RouteSegmentType.Start;
                if (route.Segments.Length > 1)
                {
                    route.Segments[route.Segments.Length - 1].Type = RouteSegmentType.Stop;
                }
            }
            return route;
        }

        /// <summary>
        /// Builds a path.
        /// </summary>
        /// <returns></returns>
        public PathSegment<uint> BuildPath()
        {
            this.CheckHasRunAndSucceeded();

            var path = new PathSegment<uint>(_target);
            var currentPath = path;
            DykstraVisit visit;
            while (this.Algorithm.TryGetVisit(currentPath.VertexId, out visit))
            {
                if (visit.From == 0)
                {
                    break;
                }
                currentPath.Weight = visit.Weight;
                currentPath.From = new PathSegment<uint>((uint)visit.From);
                currentPath = currentPath.From;
            }

            if(_backward)
            {
                path = path.Reverse();
            }
            return path;
        }
    }
}