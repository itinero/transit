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

using OsmSharp.Routing.Vehicles;
namespace OsmSharp.Routing.Transit.Multimodal.Algorithms
{
    /// <summary>
    /// Abstract representation of a dykstra routing algorithm.
    /// </summary>
    public interface IDykstraAlgorithm : IRoutingAlgorithm
    {
        /// <summary>
        /// Gets the vehicle.
        /// </summary>
        Vehicle Vehicle
        {
            get;
        }

        /// <summary>
        /// Gets the backward flag.
        /// </summary>
        bool Backward
        {
            get;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <param name="vertex">The vertex that was visited.</param>
        /// <param name="visit">The visit data.</param>
        /// <returns></returns>
        bool TryGetVisit(long vertex, out DykstraVisit visit);
    }
}