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

using GTFS.Entities;
using OsmSharp.Routing.Transit.GTFS.Db;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.RouteCalculators.CSA
{
    /// <summary>
    /// Holds all relevant data for the CSA to work.
    /// </summary>
    public class CSADb
    {
        /// <summary>
        /// Creates a new CSA connections collection.
        /// </summary>
        /// <param name="connections"></param>
        public CSADb()
        {
            _multiFeedDb = new MultiFeedDb();
            _connections = new List<CSAConnection>();
        }

        /// <summary>
        /// Holds the multi feed db.
        /// </summary>
        private IMultiFeedDb _multiFeedDb;

        /// <summary>
        /// Holds the multi feed db.
        /// </summary>
        public IMultiFeedDb FeedDb
        {
            get
            {
                return _multiFeedDb;
            }
        }

        /// <summary>
        /// Holds all connections.
        /// </summary>
        private List<CSAConnection> _connections;

        /// <summary>
        /// Gets all connections.
        /// </summary>
        public List<CSAConnection> Connections
        {
            get
            {
                return _connections;
            }
        }


    }
}