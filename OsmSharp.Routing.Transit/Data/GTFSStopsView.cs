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

using GTFS.Entities.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A stops view based on a GTFS stops collection.
    /// </summary>
    public class GTFSStopsView
    {
        /// <summary>
        /// Holds the gtfs stops collection.
        /// </summary>
        private IUniqueEntityCollection<global::GTFS.Entities.Stop> _gtfsStops;

        /// <summary>
        /// Creates a new GTFS stops view.
        /// </summary>
        /// <param name="stops"></param>
        public GTFSStopsView(IUniqueEntityCollection<global::GTFS.Entities.Stop> stops)
        {
            _gtfsStops = stops;
        }

        /// <summary>
        /// Returns the enumerator of the stops in this view.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Stop> GetEnumerator()
        {
            return _gtfsStops.Select<global::GTFS.Entities.Stop, Stop>(s =>
                {
                    return new Stop()
                    {
                        Latitude = (float)s.Latitude,
                        Longitude = (float)s.Longitude
                    };
                }).GetEnumerator();
        }

        /// <summary>
        /// Returns the number of stops in this view.
        /// </summary>
        public int Count
        {
            get { return _gtfsStops.Count; }
        }

        /// <summary>
        /// Returns the stop at the given index.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Stop this[int idx]
        {
            get
            {
                var stop = _gtfsStops.Get(idx);
                return new Stop()
                {
                    Latitude = (float)stop.Latitude,
                    Longitude = (float)stop.Longitude
                };
            }
        }
    }
}