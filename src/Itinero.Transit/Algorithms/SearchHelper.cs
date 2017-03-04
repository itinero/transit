// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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