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

using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Transit.Test.Transit.Data
{
    /// <summary>
    /// A stub connections db.
    /// </summary>
    public class StubConnectionsDb : ConnectionsDb
    {
        /// <summary>
        /// Creates a new connections db stub.
        /// </summary>
        public StubConnectionsDb()
        {
            this.IsTripPossibleSettable = (trip, date) => { return true; };
            this.DepartureTimeConnections = new List<Connection>();
            this.ArrivalTimeConnections = new List<Connection>();
            this.Stops = new List<Stop>();
        }

        /// <summary>
        /// Gets or sets the stops list.
        /// </summary>
        public List<Stop> Stops { get; set; }

        /// <summary>
        /// Gets the departure time view.
        /// </summary>
        /// <returns></returns>
        public override StopsView GetStops()
        {
            return new StopsListView(this.Stops);
        }

        /// <summary>
        /// Gets or sets the departure time-sorted connections list.
        /// </summary>
        public List<Connection> DepartureTimeConnections { get; set; }

        /// <summary>
        /// Gets the departure time view.
        /// </summary>
        /// <returns></returns>
        public override ConnectionsView GetDepartureTimeView()
        {
            return new ConnectionsListView(this.DepartureTimeConnections);
        }

        /// <summary>
        /// Gets or sets the arrival time-sorted connections list.
        /// </summary>
        public List<Connection> ArrivalTimeConnections { get; set; }

        /// <summary>
        /// Gets the arrival time view.
        /// </summary>
        /// <returns></returns>
        public override ConnectionsView GetArrivalTimeView()
        {
            return new ConnectionsListView(this.ArrivalTimeConnections);
        }

        /// <summary>
        /// Get or sets the is trip possible function.
        /// </summary>
        public Func<int, System.DateTime, bool> IsTripPossibleSettable
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets the is trip possible function.
        /// </summary>
        public override Func<int, System.DateTime, bool> IsTripPossible
        {
            get
            {
                return this.IsTripPossibleSettable;
            }
        }
    }
}
