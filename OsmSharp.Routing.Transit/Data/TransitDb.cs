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
        private readonly ConnectionsDb _connectionsDb;
        private readonly Dictionary<string, TransfersDb> _transfersDbs;

        /// <summary>
        /// Creates a new transit db.
        /// </summary>
        public TransitDb()
        {
            _connectionsDb = new ConnectionsDb();
            _transfersDbs = new Dictionary<string, TransfersDb>();
        }

        /// <summary>
        /// Gets the connections db.
        /// </summary>
        public ConnectionsDb ConnectionsDb
        {
            get
            {
                return _connectionsDb;
            }
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
