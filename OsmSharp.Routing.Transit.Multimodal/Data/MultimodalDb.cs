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

using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Profiles;
using OsmSharp.Routing.Transit.Data;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Data
{
    /// <summary>
    /// A multimodal db.
    /// </summary>
    public class MultimodalDb
    {
        private readonly RouterDb _routerDb;
        private readonly Dictionary<string, StopLinksDb> _links;
        private readonly GTFSConnectionsDb _connectionsDb;

        /// <summary>
        /// Creates a new multimodal db.
        /// </summary>
        public MultimodalDb(RouterDb routerDb, GTFSConnectionsDb connectionsDb)
        {
            _routerDb = routerDb;
            _connectionsDb = connectionsDb;

            _links = new Dictionary<string, StopLinksDb>();
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
        /// Gets the connections db.
        /// </summary>
        public GTFSConnectionsDb ConnectionsDb
        {
            get
            {
                return _connectionsDb;
            }
        }

        /// <summary>
        /// Adds a new stop links db for the given profile.
        /// </summary>
        public void AddStopLinksDb(Profile profile, StopLinksDb linksDb)
        {
            _links[profile.Name] = linksDb;
        }

        /// <summary>
        /// Tries to get a links db for the given profile.
        /// </summary>
        public bool TryGetStopLinksDb(Profile profile, out StopLinksDb linksDb)
        {
            return _links.TryGetValue(profile.Name, out linksDb);
        }
    }
}