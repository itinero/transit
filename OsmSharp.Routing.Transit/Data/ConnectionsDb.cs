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

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A database containing all transit-connections.
    /// </summary>
    public abstract class ConnectionsDb
    {
        /// <summary>
        /// Gets a view on the connections sorted by departure time.
        /// </summary>
        /// <returns></returns>
        public abstract ConnectionsView GetDepartureTimeView();

        /// <summary>
        /// Gets a view on the connections sorted by arrival time.
        /// </summary>
        /// <returns></returns>
        public abstract ConnectionsView GetArrivalTimeView();

        /// <summary>
        /// Gets the function to determine if a trip is possible on a given day.
        /// </summary>
        public abstract Func<int, DateTime, bool> IsTripPossible { get; }
    }
}