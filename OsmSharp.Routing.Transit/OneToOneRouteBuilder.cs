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

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Abstract implementation of a route builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class OneToOneRouteBuilder<T>
        where T : IRoutingAlgorithm
    {
        /// <summary>
        /// Holds the algorithm after it's run.
        /// </summary>
        private T _algorithm;

        /// <summary>
        /// Creates a new route builder.
        /// </summary>
        /// <param name="algorithm"></param>
        public OneToOneRouteBuilder(T algorithm)
        {
            _algorithm = algorithm;
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
        /// Builds a route.
        /// </summary>
        /// <returns></returns>
        public Route Build()
        {
            this.CheckHasRunAndSucceeded();

            return this.DoBuild();
        }

        /// <summary>
        /// Checks has run and succeeded and throws an exception if not true.
        /// </summary>
        protected void CheckHasRunAndSucceeded()
        {
            if (_algorithm.HasRun && _algorithm.HasSucceeded)
            {
                return;
            }
            throw new InvalidOperationException("Cannot build a route when algorithm has not succeeded.");
        }

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        public abstract Route DoBuild();
    }
}