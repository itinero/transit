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
    /// A stops view based on a list.
    /// </summary>
    public class StopsListView : StopsView
    {
        /// <summary>
        /// Holds the list of stops.
        /// </summary>
        private readonly List<Stop> _stops;

        /// <summary>
        /// Creates a new stops list view.
        /// </summary>
        /// <param name="stops"></param>
        public StopsListView(List<Stop> stops)
        {
            _stops = stops;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the stops in the order represented by this view.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Stop> GetEnumerator()
        {
            return _stops.GetEnumerator();
        }

        /// <summary>
        /// Returns the number of stops in this view.
        /// </summary>
        public override int Count
        {
            get { return _stops.Count; }
        }

        /// <summary>
        /// Returns the stop at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public override Stop this[int idx]
        {
            get { return _stops[idx]; }
        }
    }
}