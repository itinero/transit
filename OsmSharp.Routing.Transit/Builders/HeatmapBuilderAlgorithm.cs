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

using System;

namespace OsmSharp.Routing.Transit.Builders
{
    /// <summary>
    /// An heatmap builder that fills an heatmap live based on a heatmap source.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeatmapBuilderAlgorithm<T> : IRoutingAlgorithm
        where T : IHeatmapSource
    {
        private readonly T _algorithm;
        private readonly Heatmap _heatmap;

        /// <summary>
        /// Creates a new heatmap builder.
        /// </summary>
        public HeatmapBuilderAlgorithm(T algorithm)
            : this(algorithm, new Heatmap(15))
        {

        }

        /// <summary>
        /// Creates a new heatmap builder.
        /// </summary>
        public HeatmapBuilderAlgorithm(T algorithm, Heatmap heatmap)
        {
            _heatmap = heatmap;
            _algorithm = algorithm;
            _algorithm.ReportSampleAction = (lat, lon, weight) =>
            {
                _heatmap.AddSample(lat, lon, weight);
            };
        }

        /// <summary>
        /// Gets the algorithm.
        /// </summary>
        public T Algorithm
        {
            get
            {
                return _algorithm;
            }
        }

        /// <summary>
        /// Returns true if this instance has run already.
        /// </summary>
        public bool HasRun
        {
            get { return _algorithm.HasRun; }
        }

        /// <summary>
        /// Returns true if this instance has run and it was succesfull.
        /// </summary>
        public bool HasSucceeded
        {
            get { return _algorithm.HasRun; }
        }

        /// <summary>
        /// Runs the algorithm.
        /// </summary>
        public void Run()
        {
            _algorithm.Run();
        }
    }
}