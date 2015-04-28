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

using GTFS;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.GTFS.Db
{
    /// <summary>
    /// An implementation of the IMultiFeedDb.
    /// </summary>
    public class MultiFeedDb : IMultiFeedDb
    {
        /// <summary>
        /// Holds all feeds.
        /// </summary>
        private List<IGTFSFeed> _feeds;

        /// <summary>
        /// Adds a new feed to this multifeed db and returns the id.
        /// </summary>
        /// <param name="feed">The feed to add.</param>
        /// <returns></returns>
        public int AddFeed(IGTFSFeed feed)
        {
            int id = _feeds.Count;
            _feeds.Add(feed);
            return id;
        }

        /// <summary>
        /// Returns a feed for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IGTFSFeed GetFeed(int id)
        {
            return _feeds[id];
        }
    }
}