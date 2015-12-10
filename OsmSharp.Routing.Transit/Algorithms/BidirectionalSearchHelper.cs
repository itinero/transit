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
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms
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
                Path forwardVisit;
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
        public List<uint> GetPath()
        {
            if (!this.HasSucceeded)
            {
                throw new Exception("No results available, algorithm was not successful!");
            }

            Path fromSource;
            Path toTarget;
            if (_sourceSearch.TryGetVisit(_bestVertex, out fromSource) &&
               _targetSearch.TryGetVisit(_bestVertex, out toTarget))
            {
                var path = new List<uint>();
                fromSource.AddToList(path);
                if (toTarget.From != null)
                {
                    toTarget.From.AddToListReverse(path);
                }
                return path;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}