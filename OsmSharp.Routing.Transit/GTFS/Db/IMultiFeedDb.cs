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

namespace OsmSharp.Routing.Transit.GTFS.Db
{
    /// <summary>
    /// Holds the multi feed db.
    /// </summary>
    public interface IMultiFeedDb
    {
        /// <summary>
        /// Adds a new feed to this multifeed db and returns the id.
        /// </summary>
        /// <param name="feed">The feed to add.</param>
        /// <returns></returns>
        int AddFeed(IGTFSFeed feed);

        /// <summary>
        /// Returns a feed for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IGTFSFeed GetFeed(int id);
    }
}