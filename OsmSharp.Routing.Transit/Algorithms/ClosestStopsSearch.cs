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

using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Algorithms.Default;
using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Profiles;
using OsmSharp.Routing.Transit.Algorithms.Search;
using OsmSharp.Routing.Transit.Data;
using System;

namespace OsmSharp.Routing.Transit.Algorithms
{
    /// <summary>
    /// Closest stop search.
    /// </summary>
    public class ClosestStopsSearch : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly TransitDb _db;
        private readonly Profile _profile;
        private readonly RouterPoint _routerPoint;
        private readonly float _maxSeconds;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new closest stop search.
        /// </summary>
        public ClosestStopsSearch(RouterDb routerDb, TransitDb transitDb, Profile profile,
            RouterPoint routerPoint, float maxSeconds, bool backward)
        {
            if (profile.Metric != ProfileMetric.TimeInSeconds)
            { // oeps, this profile is not time-based!
                throw new ArgumentOutOfRangeException(
                    "The default closest stop search can only be used with profiles that use time in seconds as a metric.");
            }

            _routerDb = routerDb;
            _db = transitDb;
            _profile = profile;
            _routerPoint = routerPoint;
            _maxSeconds = maxSeconds;
            _backward = backward;

            // build search.
            _dykstra = new Dykstra(_routerDb.Network.GeometricGraph.Graph, (profileId) =>
            {
                var profileTags = _routerDb.EdgeProfiles.Get(profileId);
                return _profile.Factor(profileTags);
            }, _routerPoint.ToPaths(_routerDb, _profile, !_backward), _maxSeconds, _backward);
        }

        private System.Collections.Generic.Dictionary<uint, LinkedStopRouterPoint> _stopsPerEdge;
        private Dykstra _dykstra = null;
        private StopLinksDb.Enumerator _stopLinksDbEnumerator;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // get the stop links db for the profile given.
            var stopLinksDb = _db.GetStopLinksDb(_profile);
            _stopLinksDbEnumerator = stopLinksDb.GetEnumerator();

            // find the closest stops within the range of the 'maxSeconds' parameter.
            var distance = _profile.MinSpeed().Value * _maxSeconds;
            var stopsDbEnumerator = _db.GetStopsEnumerator();
            var location = new Math.Geo.GeoCoordinate(_routerPoint.Latitude, _routerPoint.Longitude);
            var box = new OsmSharp.Math.Geo.GeoCoordinateBox(
                location.OffsetWithDirection(distance, Math.Geo.Meta.DirectionEnum.West)
                    .OffsetWithDirection(distance, Math.Geo.Meta.DirectionEnum.North),
                location.OffsetWithDirection(distance, Math.Geo.Meta.DirectionEnum.East)
                    .OffsetWithDirection(distance, Math.Geo.Meta.DirectionEnum.South));
            var closestStops = stopsDbEnumerator.Search((float)box.MinLat, (float)box.MinLon, 
                (float)box.MaxLat, (float)box.MaxLon);
            _stopsPerEdge = new System.Collections.Generic.Dictionary<uint, LinkedStopRouterPoint>();
            foreach(var stop in closestStops)
            {
                _stopLinksDbEnumerator.MoveTo(stop);

                while (_stopLinksDbEnumerator.MoveNext())
                {
                    var newStopRouterPoint = new LinkedStopRouterPoint()
                        {
                            RouterPoint = new RouterPoint(0, 0, _stopLinksDbEnumerator.EdgeId, _stopLinksDbEnumerator.Offset),
                            StopId = stop
                        };
                    LinkedStopRouterPoint stopRouterPoint;
                    if (_stopsPerEdge.TryGetValue(_stopLinksDbEnumerator.EdgeId, out stopRouterPoint))
                    {
                        stopRouterPoint.Next = newStopRouterPoint;
                    }
                    else
                    {
                        _stopsPerEdge[_stopLinksDbEnumerator.EdgeId] = newStopRouterPoint;
                    }
                }
            }

            // report on source edges and build source paths.
            var paths = _routerPoint.ToPaths(_routerDb, _profile, !_backward);
            for (var p = 0; p < paths.Length; p++)
            {
                var edgeId = _routerPoint.EdgeId;
                LinkedStopRouterPoint stopRouterPoint;
                if (_stopsPerEdge.TryGetValue(edgeId, out stopRouterPoint))
                {
                    if (_routerPoint.EdgeId != edgeId)
                    { // ok, this is not the source-ege.
                        throw new Exception("Cannot report on an edge that is not the source when not searching yet.");
                    }

                    // move the stop links enumerator.
                    while (stopRouterPoint != null)
                    {
                        var stopId = stopRouterPoint.StopId;
                        var routerPoint = stopRouterPoint.RouterPoint;

                        if (!_backward)
                        { // forward, route from source to stop.
                            var path = _routerPoint.PathTo(_routerDb, _profile, routerPoint);
                            if (path != null)
                            { // there is a path, report on it.
                                if (this.StopFound(stopId, path.Weight))
                                { // report this edge was found.
                                    this.HasSucceeded = true;
                                    return;
                                }
                            }
                        }
                        else
                        { // backward, route from stop to source.
                            var path = routerPoint.PathTo(_routerDb, _profile, _routerPoint);
                            if (path != null)
                            { // there is a path, report on it.
                                if (this.StopFound(stopId, path.Weight))
                                { // report this edge was found.
                                    this.HasSucceeded = true;
                                    return;
                                }
                            }
                        }

                        stopRouterPoint = stopRouterPoint.Next;
                    }
                }
            }

            // execute dykstra search from all sources.
            _dykstra.WasEdgeFound = this.WasEdgeFoundInternal;
            _dykstra.WasFound = this.WasFoundInternal;
            _dykstra.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// A function to report that a stop was found and the number of seconds to travel to/from.
        /// </summary>
        public delegate bool StopFoundFunc(uint stop, float time);

        /// <summary>
        /// Gets or sets the stop found function.
        /// </summary>
        public virtual StopFoundFunc StopFound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the dykstra search.
        /// </summary>
        public Dykstra Search
        {
            get
            {
                return _dykstra;
            }
        }

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Dykstra.WasEdgeFoundDelegate WasEdgeFound { get; set; }

        /// <summary>
        /// Called when a new edge was found.
        /// </summary>
        private bool WasEdgeFoundInternal(uint vertex, uint edgeId, float weight)
        {
            if(this.WasEdgeFound != null &&
               this.WasEdgeFound(vertex, edgeId, weight))
            { // somewhere else the descision was made to stop the search here.
                return true;
            }

            LinkedStopRouterPoint stopRouterPoint;
            if(_stopsPerEdge.TryGetValue(edgeId, out stopRouterPoint))
            { // ok, there is a link, get the router points and calculate weights.
                if (_dykstra == null)
                { // no dykstra search just yet, this is the source-edge.
                    throw new Exception("Could not get visit of other vertex for settled edge.");
                }
                var edge = _routerDb.Network.GeometricGraph.Graph.GetEdge(edgeId);
                var vertex1 = edge.GetOther(vertex);
                Path vertex1Visit;
                if (!_dykstra.TryGetVisit(vertex1, out vertex1Visit))
                {
                    throw new Exception("Could not get visit of other vertex for settled edge.");
                }

                // move the stop links enumerator.
                while (stopRouterPoint != null)
                {
                    var stopId = stopRouterPoint.StopId;
                    var routerPoint = stopRouterPoint.RouterPoint;

                    var paths = routerPoint.ToPaths(_routerDb, _profile, _backward);
                    if (paths[0].Vertex == vertex1)
                    { // report on the time.
                        if (this.StopFound(stopId, vertex1Visit.Weight + paths[0].Weight))
                        {
                            return true;
                        }
                    }
                    else if (paths.Length > 1 && paths[1].Vertex == vertex1)
                    { // report on the time.
                        if (this.StopFound(stopId, vertex1Visit.Weight + paths[1].Weight))
                        {
                            return true;
                        }
                    }

                    stopRouterPoint = stopRouterPoint.Next;
                }
            }
            return false;
        }

        /// <summary>
        ///  Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Dykstra.WasFoundDelegate WasFound { get; set; }

        /// <summary>
        /// Called when a new vertex was found.
        /// </summary>
        private bool WasFoundInternal(uint vertex, float weight)
        {
            if (this.WasFound != null)
            {
                return this.WasFound(vertex, weight);
            }
            return false;
        }

        /// <summary>
        /// Gets the weight to the given stop.
        /// </summary>
        public float GetWeight(uint stop)
        {
            var bestWeight = float.MaxValue;
            _stopLinksDbEnumerator.MoveTo(stop);
            while (_stopLinksDbEnumerator.MoveNext())
            {
                var point = new RouterPoint(0, 0, _stopLinksDbEnumerator.EdgeId,
                    _stopLinksDbEnumerator.Offset);
                if (point.EdgeId == _routerPoint.EdgeId)
                { // on the same edge.
                    Path path;
                    if (_backward)
                    { // from stop -> source.
                        path = point.PathTo(_routerDb, _profile, _routerPoint);
                    }
                    else
                    { // from source -> stop.
                        path = _routerPoint.PathTo(_routerDb, _profile, point);
                    }
                    if (path.Weight < bestWeight)
                    { // set as best because improvement.
                        bestWeight = path.Weight;
                    }
                }
                else
                { // on different edge, to the usual.
                    var paths = point.ToPaths(_routerDb, _profile, _backward);
                    Path visit;
                    if (_dykstra.TryGetVisit(paths[0].Vertex, out visit))
                    { // check if this one is better.
                        if (visit.Weight + paths[0].Weight < bestWeight)
                        { // concatenate paths and set best.
                            Path best;
                            if (paths[0].Weight == 0)
                            { // just use the visit.
                                best = visit;
                            }
                            else
                            { // there is a distance/weight.
                                best = new Path(OsmSharp.Routing.Constants.NO_VERTEX,
                                    paths[0].Weight + visit.Weight, visit);
                            }
                            bestWeight = best.Weight;
                        }
                    }
                    if (paths.Length > 1 && _dykstra.TryGetVisit(paths[1].Vertex, out visit))
                    { // check if this one is better.
                        if (visit.Weight + paths[1].Weight < bestWeight)
                        { // concatenate paths and set best.
                            Path best;
                            if (paths[1].Weight == 0)
                            { // just use the visit.
                                best = visit;
                            }
                            else
                            { // there is a distance/weight.
                                best = new Path(OsmSharp.Routing.Constants.NO_VERTEX,
                                    paths[1].Weight + visit.Weight, visit);
                            }
                            bestWeight = best.Weight;
                        }
                    }
                }
            }
            return bestWeight;
        }

        /// <summary>
        /// Gets the path to the given stop.
        /// </summary>
        public Path GetPath(uint stop)
        {
            Path best = null;
            var bestWeight = float.MaxValue;
            _stopLinksDbEnumerator.MoveTo(stop);
            while (_stopLinksDbEnumerator.MoveNext())
            {
                var point = new RouterPoint(0, 0, _stopLinksDbEnumerator.EdgeId, 
                    _stopLinksDbEnumerator.Offset);
                if (point.EdgeId == _routerPoint.EdgeId)
                { // on the same edge.
                    Path path;
                    if (_backward)
                    { // from stop -> source.
                        path = point.PathTo(_routerDb, _profile, _routerPoint);
                    }
                    else
                    { // from source -> stop.
                        path = _routerPoint.PathTo(_routerDb, _profile, point);
                    }
                    if (path.Weight < bestWeight)
                    { // set as best because improvement.
                        best = path;
                        bestWeight = path.Weight;
                    }
                }
                else
                { // on different edge, to the usual.
                    var paths = point.ToPaths(_routerDb, _profile, _backward);
                    Path visit;
                    if (_dykstra.TryGetVisit(paths[0].Vertex, out visit))
                    { // check if this one is better.
                        if (visit.Weight + paths[0].Weight < bestWeight)
                        { // concatenate paths and set best.
                            if (paths[0].Weight == 0)
                            { // just use the visit.
                                best = visit;
                            }
                            else
                            { // there is a distance/weight.
                                best = new Path(OsmSharp.Routing.Constants.NO_VERTEX,
                                    paths[0].Weight + visit.Weight, visit);
                            }
                            bestWeight = best.Weight;
                        }
                    }
                    if (paths.Length > 1 && _dykstra.TryGetVisit(paths[1].Vertex, out visit))
                    { // check if this one is better.
                        if (visit.Weight + paths[1].Weight < bestWeight)
                        { // concatenate paths and set best.
                            if (paths[1].Weight == 0)
                            { // just use the visit.
                                best = visit;
                            }
                            else
                            { // there is a distance/weight.
                                best = new Path(OsmSharp.Routing.Constants.NO_VERTEX,
                                    paths[1].Weight + visit.Weight, visit);
                            }
                            bestWeight = best.Weight;
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Gets the target router point for the given stop.
        /// </summary>
        public RouterPoint GetTargetPoint(uint stop)
        {
            RouterPoint best = null;
            var bestWeight = float.MaxValue;
            var stopEnumerator = _db.GetStopsEnumerator();
            if (stopEnumerator.MoveTo(stop))
            {
                _stopLinksDbEnumerator.MoveTo(stop);
                while (_stopLinksDbEnumerator.MoveNext())
                {
                    var point = new RouterPoint(stopEnumerator.Latitude, stopEnumerator.Longitude, _stopLinksDbEnumerator.EdgeId,
                        _stopLinksDbEnumerator.Offset);
                    if (point.EdgeId == _routerPoint.EdgeId)
                    { // on the same edge.
                        Path path;
                        if (_backward)
                        { // from stop -> source.
                            path = point.PathTo(_routerDb, _profile, _routerPoint);
                        }
                        else
                        { // from source -> stop.
                            path = _routerPoint.PathTo(_routerDb, _profile, point);
                        }
                        if (path.Weight < bestWeight)
                        { // set as best because improvement.
                            best = point;
                            bestWeight = path.Weight;
                        }
                    }
                    else
                    { // on different edge, to the usual.
                        var paths = point.ToPaths(_routerDb, _profile, _backward);
                        Path visit;
                        if (_dykstra.TryGetVisit(paths[0].Vertex, out visit))
                        { // check if this one is better.
                            if (visit.Weight + paths[0].Weight < bestWeight)
                            { // concatenate paths and set best.
                                if (paths[0].Weight == 0)
                                { // just use the visit.
                                    best = point;
                                    bestWeight = visit.Weight;
                                }
                                else
                                { // there is a distance/weight.
                                    best = point;
                                    bestWeight = paths[0].Weight + visit.Weight;
                                }
                            }
                        }
                        if (paths.Length > 1 && _dykstra.TryGetVisit(paths[1].Vertex, out visit))
                        { // check if this one is better.
                            if (visit.Weight + paths[1].Weight < bestWeight)
                            { // concatenate paths and set best.
                                if (paths[1].Weight == 0)
                                { // just use the visit.
                                    best = point;
                                    bestWeight = visit.Weight;
                                }
                                else
                                { // there is a distance/weight.
                                    best = point;
                                    bestWeight = paths[1].Weight + visit.Weight;
                                }
                            }
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Gets the route to the given stop.
        /// </summary>
        public Route GetRoute(uint stop)
        {
            var path = this.GetPath(stop);
            var point = this.GetTargetPoint(stop);

            if (_backward)
            {
                path = path.Reverse();
                return RouteBuilder.Build(_routerDb, _profile, point, _routerPoint, path);
            }
            return RouteBuilder.Build(_routerDb, _profile, _routerPoint, point, path);
        }

        /// <summary>
        /// A linked stop router point.
        /// </summary>
        private class LinkedStopRouterPoint
        {
            /// <summary>
            /// Gets or sets the router point.
            /// </summary>
            public RouterPoint RouterPoint { get; set; }

            /// <summary>
            /// Gets or sets the stop id.
            /// </summary>
            public uint StopId { get; set; }

            /// <summary>
            /// Gets or sets the next link.
            /// </summary>
            public LinkedStopRouterPoint Next { get; set; }
        }
    }
}