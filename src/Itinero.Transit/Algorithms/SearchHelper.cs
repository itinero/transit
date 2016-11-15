// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms;
using Itinero.Algorithms.Default;
using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Transit.Algorithms
{
    /// <summary>
    /// A helper for implicit searches.
    /// </summary>
    public class SearchHelper
    {
        private readonly Dykstra _search;
        private readonly RouterPoint _target;
        private readonly EdgePath<float>[] _targetPaths;

        /// <summary>
        /// Creates a new search helper.
        /// </summary>
        public SearchHelper(RouterDb routerDb, Dykstra search, Profile profile, RouterPoint target)
        {
            _search = search;
            _target = target;

            _targets = new HashSet<uint>();
            _targetPaths = target.ToEdgePaths(routerDb, new DefaultWeightHandler(profile.GetGetFactor(routerDb)), false);
            for (var i = 0; i < _targetPaths.Length; i++)
            {
                _targets.Add(_targetPaths[i].Vertex);
            }
        }

        private readonly HashSet<uint> _targets;
        private EdgePath<float> _best = null;
        private uint _bestVertex = uint.MaxValue;
        private float _bestWeight = float.MaxValue;

        /// <summary>
        /// Gets the has succeeded flag.
        /// </summary>
        public bool HasSucceeded { get; private set; }

        /// <summary>
        /// Called when source was found.
        /// </summary>
        public bool SourceWasFound(uint vertex, float weight)
        {
            if(_targets.Contains(vertex))
            {
                EdgePath<float> path;
                if(_search.TryGetVisit(vertex, out path))
                {
                    for(var i = 0; i < _targetPaths.Length; i++)
                    {
                        if(_targetPaths[i].Vertex == vertex &&
                           _targetPaths[i].Weight + weight < _bestWeight)
                        {
                            _best = path;
                            _bestWeight = _targetPaths[i].Weight + weight;
                            _bestVertex = _targetPaths[i].Vertex;
                            this.HasSucceeded = true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the best weight.
        /// </summary>
        public float BestWeight
        {
            get
            {
                if (!this.HasSucceeded)
                {
                    throw new Exception("No results available, algorithm was not successful!");
                }

                return _bestWeight;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public EdgePath<float> GetPath()
        {
            if (!this.HasSucceeded)
            {
                throw new Exception("No results available, algorithm was not successful!");
            }

            for(var i = 0; i < _targetPaths.Length; i++)
            {
                if(_targetPaths[i].Vertex == _bestVertex)
                {
                    return _best.Append(_targetPaths[i]);
                }
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}