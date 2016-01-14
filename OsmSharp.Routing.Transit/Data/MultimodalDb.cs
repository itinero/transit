// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a multimodal db, a transit db and a router db.
    /// </summary>
    public class MultimodalDb
    {
        private readonly RouterDb _routerDb;
        private readonly TransitDb _transitDb;
        private Dictionary<string, StopLinksDb> _stoplinksDbs;

        /// <summary>
        /// Creates a new multimodal db.
        /// </summary>
        public MultimodalDb(RouterDb routerDb, TransitDb transitDb)
        {
            _routerDb = routerDb;
            _transitDb = transitDb;

            _stoplinksDbs = new Dictionary<string, StopLinksDb>();
        }

        /// <summary>
        /// Gets the router db.
        /// </summary>
        public RouterDb RouterDb
        {
            get
            {
                return _routerDb;
            }
        }

        /// <summary>
        /// Gets the transit db.
        /// </summary>
        public TransitDb TransitDb
        {
            get
            {
                return _transitDb;
            }
        }


        /// <summary>
        /// Adds a stop links db.
        /// </summary>
        public void AddStopLinksDb(StopLinksDb db)
        {
            if (db == null) { throw new ArgumentNullException("db"); }

            if (db.Guid != _routerDb.Guid)
            {
                throw new Exception("Cannot add this stop links db. It was built based on a different network.");
            }

            _stoplinksDbs[db.ProfileName] = db;
        }

        /// <summary>
        /// Returns true if there is a stop links db for the given profile.
        /// </summary>
        public bool HasStopLinksDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            return _stoplinksDbs.ContainsKey(profile.Name);
        }

        /// <summary>
        /// Gets the stop links db.
        /// </summary>
        public StopLinksDb GetStopLinksDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            return _stoplinksDbs[profile.Name];
        }

        /// <summary>
        /// Serializes to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            var position = stream.Position;
            stream.WriteByte(1); // writes the version #.

            // write routerdb.
            _routerDb.Serialize(stream);

            // write transitdb.
            _transitDb.Serialize(stream);

            // write stop links db.
            stream.WriteByte((byte)_stoplinksDbs.Count);
            foreach(var stopLinksDb in _stoplinksDbs)
            {
                stopLinksDb.Value.Serialize(stream);
            }
            return stream.Position - position;
        }

        /// <summary>
        /// Deserializes from the given stream.
        /// </summary>
        public static MultimodalDb Deserialize(Stream stream)
        {
            if (stream.ReadByte() != 1)
            {
                throw new Exception("Cannot deserialize db, version # doesn't match.");
            }

            // deserialize router db.
            var routerDb = RouterDb.Deserialize(stream);

            // read transit db.
            var transitDb = TransitDb.Deserialize(stream);

            // create db.
            var db = new MultimodalDb(routerDb, transitDb);

            // read stop links db count.
            var stopLinksDbCount = stream.ReadByte();
            for(var i = 0;i < stopLinksDbCount;i++)
            {
                db.AddStopLinksDb(StopLinksDb.Deserialize(stream));
            }
            return db;
        }
    }
}
