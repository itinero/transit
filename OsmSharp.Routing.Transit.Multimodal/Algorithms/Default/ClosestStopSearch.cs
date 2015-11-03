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
using OsmSharp.Routing.Transit.Multimodal.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.Default
{
    /// <summary>
    /// A default closest stop search algorithm.
    /// </summary>
    public class ClosestStopSearch : ClosestStopSearchBase
    {
        private readonly bool _backward;
        private readonly RouterDb _routingDb;
        private readonly Profile _profile;
        private readonly RouterPoint _source;
        private readonly float _maxSeconds;
        private readonly StopLinksDb _stopLinksDb;

        /// <summary>
        /// Creates a new closest stop search.
        /// </summary>
        public ClosestStopSearch(RouterDb routingDb, Profile profile, StopLinksDb stopLinksDb, 
            RouterPoint source, float maxSeconds, bool backward)
        {
            if (profile.Metric != ProfileMetric.TimeInSeconds)
            { // oeps, this profile is not time-based!
                throw new ArgumentOutOfRangeException(
                    "The default closest stop search can only be used with profiles that use time in seconds as a metric.");
            }

            _routingDb = routingDb;
            _profile = profile;
            _source = source;
            _backward = backward;
            _maxSeconds = maxSeconds;
            _stopLinksDb = stopLinksDb;
        }

        private Dykstra _dykstra;
        private StopLinksDb.StopLinksDbEnumerator _stopLinksEnumerator;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _stopLinksEnumerator = _stopLinksDb.GetEnumerator();

            // report on source edges and build source paths.
            var sourcePaths = new List<Path>();
            var paths = _source.ToPaths(_routingDb, _profile, !_backward);
            for (var p = 0; p < paths.Length; p++)
            {
                sourcePaths.Add(paths[p]);

                if (this.WasEdgeFound(paths[p].Vertex, _source.EdgeId, paths[p].Weight))
                { // report this edge was found.
                    this.HasSucceeded = true;
                    return;
                }
            }

            // execute dykstra search from all sources.
            _dykstra = new Dykstra(_routingDb.Network.GeometricGraph.Graph, (profileId) =>
                {
                    var profileTags = _routingDb.EdgeProfiles.Get(profileId);
                    return _profile.Factor(profileTags);
                }, sourcePaths, _maxSeconds, _backward);
            _dykstra.WasEdgeFound = this.WasEdgeFound;
            _dykstra.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Called when a new edge was found.
        /// </summary>
        private bool WasEdgeFound(uint vertex, uint edgeId, float weight)
        {
            if(_stopLinksDb.HasLink(edgeId))
            { // ok, there is a link, get the router points and calculate weights.
                if(_dykstra == null)
                { // no dykstra search just yet, this is the source-edge.
                    if(_source.EdgeId != edgeId)
                    { // ok, this is not the source-ege.
                        throw new Exception("Cannot report on an edge that is not the source when not searching yet.");
                    }

                    // move the stop links enumerator.
                    _stopLinksEnumerator.MoveTo(edgeId);
                    while (_stopLinksEnumerator.MoveNext())
                    {
                        var stopId = _stopLinksEnumerator.StopId;
                        var routerPoint = _stopLinksEnumerator.RouterPoint;

                        if(!_backward)
                        { // forward, route from source to stop.
                            var path = _source.PathTo(_routingDb, _profile, routerPoint);
                            if(path != null)
                            { // there is a path, report on it.
                                this.StopFound(stopId, path.Weight);
                            }
                        }
                        else
                        { // backward, route from stop to source.
                            var path = routerPoint.PathTo(_routingDb, _profile, _source);
                            if (path != null)
                            { // there is a path, report on it.
                                this.StopFound(stopId, path.Weight);
                            }
                        }
                    }
                    return false;
                }
                var edge = _routingDb.Network.GeometricGraph.Graph.GetEdge(edgeId);
                var vertex1 = edge.GetOther(vertex);
                Path vertex1Visit;
                if(!_dykstra.TryGetVisit(vertex1, out vertex1Visit))
                {
                    throw new Exception("Could not get visit of other vertex for settled edge.");
                }

                // move the stop links enumerator.
                _stopLinksEnumerator.MoveTo(edgeId);
                while(_stopLinksEnumerator.MoveNext())
                {
                    var stopId = _stopLinksEnumerator.StopId;
                    var routerPoint = _stopLinksEnumerator.RouterPoint;

                    var paths = routerPoint.ToPaths(_routingDb, _profile, _backward);
                    if(paths[0].Vertex == vertex1)
                    { // report on the time.
                        this.StopFound(stopId, vertex1Visit.Weight + paths[0].Weight);
                    }
                    else if(paths[1].Vertex == vertex1)
                    { // report on the time.
                        this.StopFound(stopId, vertex1Visit.Weight + paths[1].Weight);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the path to the given stop.
        /// </summary>
        public override Path GetPath(uint stop)
        {
            Path best = null;
            var bestWeight = float.MaxValue;
            var pointLink = _stopLinksDb.Get(stop);
            while (pointLink != null)
            {
                var point = pointLink.Item;
                var paths = point.ToPaths(_routingDb, _profile, _backward);
                Path visit;
                if(_dykstra.TryGetVisit(paths[0].Vertex, out visit))
                { // check if this one is better.
                    if(visit.Weight + paths[0].Weight < bestWeight)
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
                if (_dykstra.TryGetVisit(paths[1].Vertex, out visit))
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

                pointLink = pointLink.Next; // move to next.
            }
            return best;
        }
    }
}