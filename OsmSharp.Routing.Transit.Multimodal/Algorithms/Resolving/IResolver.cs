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

using OsmSharp.Math.Geo;

namespace OsmSharp.Routing.Transit.Multimodal.Algorithms.Resolving
{
    /// <summary>
    /// Abstract a resolver.
    /// </summary>
    /// <typeparam name="THook"></typeparam>
    /// <typeparam name="TRoutingPoint"></typeparam>
    public interface IResolver<THook, TRoutingPoint>
    {
        /// <summary>
        /// Resolves a location to a routing point.
        /// </summary>
        /// <param name="location"></param>
        /// <exception cref="System.Exception">The resolving operation failed.</exception>
        /// <returns>A routing point. This method should quarantee a non-null return.</returns>
        TRoutingPoint Resolve(GeoCoordinate location);

        /// <summary>
        /// Converts a resolved point to a routing hook that can serve as the start of a routing algorithm run.
        /// </summary>
        /// <param name="routingPoint"></param>
        /// <returns></returns>
        THook GetHook(TRoutingPoint routingPoint);
    }
}