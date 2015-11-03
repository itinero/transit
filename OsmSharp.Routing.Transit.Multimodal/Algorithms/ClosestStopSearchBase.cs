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

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms
{
    /// <summary>
    /// Abstract representation of an algorithm executing a search for closest stops.
    /// </summary>
    public abstract class ClosestStopSearchBase : AlgorithmBase
    {
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
        /// Gets the path to the given stop.
        /// </summary>
        public abstract Path GetPath(uint stop);
    }
}