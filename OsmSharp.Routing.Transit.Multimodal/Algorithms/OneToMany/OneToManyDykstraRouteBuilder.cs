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
using OsmSharp.Math.Geo;
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

        /// <summary>
        /// Creates a new one-to-many dykstra routing algorithm.
        /// </summary>
        public OneToManyDykstraRouteBuilder(RouterDataSource<Edge> graph, IDykstraAlgorithm algorithm, uint target)
            : base(algorithm)
        {
            _graph = graph;
            _target = target;
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
            float previousLatitude = 0, previousLongitude = 0;
            var time = 0.0;
            var distance = 0.0;
            TagsCollectionBase tags = null;
            var route = new Route();
            route.Vehicle = vehicle.UniqueName;
            var segments = new List<RouteSegment>();
            if (!this.Algorithm.Backward)
            { // build forward route.
                for (var i = 0; i < path.Count; i++)
                {
                    visit = path[i].Item2;
                    if (visit != null && visit.From != 0)
                    { // visit is there.
                        tags = _graph.TagsIndex.Get(visit.Edge.Tags);
                        var edgeTime = (visit.Edge.Distance / ((MeterPerSecond)vehicle.ProbableSpeed(tags)).Value);
                        var localDistance = 0.0;
                        var localTime = 0.0;
                        if (visit.Coordinates != null && visit.Coordinates.Count > 0)
                        { // insert segment for each shape point.
                            foreach (var coordinate in visit.Coordinates)
                            {
                                var localLatitude = coordinate.Latitude;
                                var localLongitude = coordinate.Longitude;
                                localDistance = localDistance + GeoCoordinate.DistanceEstimateInMeter(
                                    localLatitude, localLongitude, previousLatitude, previousLongitude);
                                localTime = localTime + (localDistance / visit.Edge.Distance) * edgeTime;

                                segments.Add(new RouteSegment()
                                {
                                    Latitude = localLatitude,
                                    Longitude = localLongitude,
                                    Type = segments.Count == 0 ? RouteSegmentType.Start : RouteSegmentType.Along,
                                    Time = localTime,
                                    Distance = localDistance
                                });

                                previousLongitude = localLongitude;
                                previousLatitude = localLatitude;
                            }
                        }
                        time = time + edgeTime;
                        distance = distance + visit.Edge.Distance;
                    }
                    float latitude, longitude;
                    _graph.GetVertex(path[i].Item1, out latitude, out longitude);
                    segments.Add(new RouteSegment()
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        Type = segments.Count == 0 ? RouteSegmentType.Start : RouteSegmentType.Along,
                        Time = time,
                        Distance = distance
                    });

                    previousLatitude = latitude;
                    previousLongitude = longitude;
                }
            }
            else
            { // build backward route.
                visit = default(DykstraVisit);
                time = 0;
                distance = 0;
                for(var i = path.Count - 1; i >= 0; i--)
                {
                    visit = path[i].Item2;
                    float latitude, longitude;
                    _graph.GetVertex(path[i].Item1, out latitude, out longitude);
                    segments.Add(new RouteSegment()
                    {
                        Latitude = latitude,
                        Longitude = longitude,
                        Type = i == path.Count - 1 ? RouteSegmentType.Start : RouteSegmentType.Along,
                        Time = time,
                        Distance = distance,
                        Tags = tags != null ? tags.ConvertFrom() : null
                    });
                    if (visit != null)
                    { // visit is there.
                        tags = _graph.TagsIndex.Get(visit.Edge.Tags);
                        var edgeTime = (visit.Edge.Distance / ((MeterPerSecond)vehicle.ProbableSpeed(tags)).Value);
                        var localDistance = 0.0;
                        var localTime = 0.0;
                        if (visit.Coordinates != null)
                        {
                            foreach (var coordinate in visit.Coordinates.Reverse())
                            {
                                var localLatitude = coordinate.Latitude;
                                var localLongitude = coordinate.Longitude;
                                localDistance = localDistance + GeoCoordinate.DistanceEstimateInMeter(
                                    localLatitude, localLongitude, previousLatitude, previousLongitude);
                                localTime = localTime + (localDistance / visit.Edge.Distance) * edgeTime;

                                segments.Add(new RouteSegment()
                                {
                                    Latitude = localLatitude,
                                    Longitude = localLongitude,
                                    Type = segments.Count == 0 ? RouteSegmentType.Start : RouteSegmentType.Along,
                                    Time = localTime,
                                    Distance = localDistance
                                });

                                previousLongitude = localLongitude;
                                previousLatitude = localLatitude;
                            }
                        }
                        time = time + edgeTime;
                        distance = distance + visit.Edge.Distance;
                    }
                }
            }
            route.Segments = segments.ToArray();
            
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

            if(this.Algorithm.Backward)
            {
                path = path.Reverse();
            }
            return path;
        }
    }
}