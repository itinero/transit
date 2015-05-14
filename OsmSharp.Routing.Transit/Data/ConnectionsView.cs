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

using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A view to access connections that enables iteration over them in a particular order. Allows random access to the connections in the order this view represents them.
    /// </summary>
    public abstract class ConnectionsView : IEnumerable<Connection>
    {
        /// <summary>
        /// Returns an enumerator that iterates through the connections in the order represented by this view.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<Connection> GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the connections in the order represented by this view.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of connections in this view.
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Returns the connection at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public abstract Connection this[int idx]
        {
            get;
        }
    }
}