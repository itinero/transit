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
using System;

namespace Itinero.Transit.Algorithms
{
    /// <summary>
    /// A helper for implicit bidirectional search.
    /// </summary>
    public class BidirectionalSearchHelper
    {
        private readonly Dykstra _sourceSearch;
        private readonly Dykstra _targetSearch;

        /// <summary>
        /// Creates a new bidirectional search helper.
        /// </summary>
        public BidirectionalSearchHelper(Dykstra sourceSearch, Dykstra targetSearch)
        {
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;

            this.HasSucceeded = false;
        }

        private uint _bestVertex = uint.MaxValue;
        private float _bestWeight = float.MaxValue;

        /// <summary>
        /// Gets the has succeeded flag.
        /// </summary>
        public bool HasSucceeded { get; private set; }

        /// <summary>
        /// Called when target was found.
        /// </summary>
        public bool TargetWasFound(uint vertex, float weight)
        {
            if (!this.HasSucceeded)
            { // not succeeded yet.
                // check forward search for the same vertex.
                EdgePath<float> forwardVisit;
                if (_sourceSearch.TryGetVisit(vertex, out forwardVisit))
                { // there is a status for this vertex in the source search.
                    weight = weight + forwardVisit.Weight;
                    if (weight < _bestWeight)
                    { // this vertex is a better match.
                        _bestWeight = weight;
                        _bestVertex = vertex;
                        this.HasSucceeded = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the best found weight.
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

            EdgePath<float> fromSource;
            EdgePath<float> toTarget;
            if (_sourceSearch.TryGetVisit(_bestVertex, out fromSource) &&
               _targetSearch.TryGetVisit(_bestVertex, out toTarget))
            {
                return fromSource.Append(toTarget);
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}