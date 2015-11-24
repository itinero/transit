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
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// A transit db containing connections and tranfers.
    /// </summary>
    public class TransitDb
    {
        private readonly StopsDb _stopsDb;
        private readonly ConnectionsDb _connectionsDb;
        private readonly TripsDb _tripsDb;
        private readonly Dictionary<string, TransfersDb> _transfersDbs;

        /// <summary>
        /// Creates a new transit db.
        /// </summary>
        public TransitDb()
        {
            _connectionsDb = new ConnectionsDb();
            _stopsDb = new StopsDb();
            _tripsDb = new TripsDb();
            _transfersDbs = new Dictionary<string, TransfersDb>();
        }
        
        /// <summary>
        /// Adds a new stop.
        /// </summary>
        public uint AddStop(float latitude, float longitude, uint metaId)
        {
            return _stopsDb.Add(latitude, longitude, metaId);
        }

        /// <summary>
        /// Sorts the stops.
        /// </summary>
        public void SortStops(Action<uint, uint> switchStops)
        {
            _stopsDb.Sort(switchStops);
        }

        /// <summary>
        /// Gets the stops enumerator.
        /// </summary>
        /// <returns></returns>
        public StopsDb.Enumerator GetStopsEnumerator()
        {
            return _stopsDb.GetEnumerator();
        }

        /// <summary>
        /// Adds a connection.
        /// </summary>
        public uint AddConnection(uint stop1, uint stop2, uint tripId, uint departureTime, uint arrivalTime)
        {
            if (stop1 >= _stopsDb.Count) { throw new ArgumentOutOfRangeException("stop1"); }
            if (stop2 >= _stopsDb.Count) { throw new ArgumentOutOfRangeException("stop2"); }
            if (tripId >= _tripsDb.Count) { throw new ArgumentOutOfRangeException("tripId"); }

            return _connectionsDb.Add(stop1, stop2, tripId, departureTime, arrivalTime);
        }

        /// <summary>
        /// Gets the sorting.
        /// </summary>
        public DefaultSorting? ConnectionSorting
        {
            get
            {
                return _connectionsDb.Sorting;
            }
        }

        /// <summary>
        /// Sorts the connections.
        /// </summary>
        public void SortConnections(DefaultSorting sorting, Action<uint, uint> switchConnections)
        {
            _connectionsDb.Sort(sorting, switchConnections);
        }

        /// <summary>
        /// Gets the connection enumerator with the given sorting.
        /// </summary>
        /// <returns></returns>
        public ConnectionsDb.Enumerator GetConnectionsEnumerator(DefaultSorting sorting)
        {
            return _connectionsDb.GetEnumerator(sorting);
        }

        /// <summary>
        /// Adds a new trip.
        /// </summary>
        public uint AddTrip(uint scheduleId, uint agencyId, uint metaId)
        {
            return _tripsDb.Add(scheduleId, agencyId, metaId);
        }

        /// <summary>
        /// Gets the trips enumerator.
        /// </summary>
        /// <returns></returns>
        public TripsDb.Enumerator GetTripsEnumerator()
        {
            return _tripsDb.GetEnumerator();
        }

        /// <summary>
        /// Adds the transfers db.
        /// </summary>
        public void AddTransfersDb(Profiles.Profile profile, TransfersDb db)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }
            if (db == null) { throw new ArgumentNullException("db"); }

            _transfersDbs[profile.Name] = db;
        }

        /// <summary>
        /// Returns true if there is a transfers db for the given profile.
        /// </summary>
        public bool HasTransfersDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            return _transfersDbs.ContainsKey(profile.Name);
        }

        /// <summary>
        /// Gets the transfers db.
        /// </summary>
        public TransfersDb GetTransfersDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            return _transfersDbs[profile.Name];
        }
    }
}
