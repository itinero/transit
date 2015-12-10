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
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms
{
    /// <summary>
    /// A helper for implicit searches.
    /// </summary>
    public class SearchHelper
    {
        private readonly Dykstra _search;
        private readonly RouterPoint _target;
        private readonly Path[] _targetPaths;

        /// <summary>
        /// Creates a new search helper.
        /// </summary>
        public SearchHelper(RouterDb routerDb, Dykstra search, Profile profile, RouterPoint target)
        {
            _search = search;
            _target = target;

            _targets = new HashSet<uint>();
            _targetPaths = target.ToPaths(routerDb, profile, false);
            for (var i = 0; i < _targetPaths.Length; i++)
            {
                _targets.Add(_targetPaths[i].Vertex);
            }
        }

        private readonly HashSet<uint> _targets;
        private Path _best = null;
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
                Path path;
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
        public List<uint> GetPath()
        {
            if (!this.HasSucceeded)
            {
                throw new Exception("No results available, algorithm was not successful!");
            }

            for(var i = 0; i < _targetPaths.Length; i++)
            {
                if(_targetPaths[i].Vertex == _bestVertex)
                {
                    var path = new List<uint>();
                    _best.AddToList(path);
                    _targetPaths[i].AddToListReverse(path);
                    return path;
                }
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}
