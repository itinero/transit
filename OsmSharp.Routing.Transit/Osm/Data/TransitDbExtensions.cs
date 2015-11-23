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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Transit.Data;

namespace OsmSharp.Routing.Transit.Osm.Data
{
    /// <summary>
    /// Contains extension methods for the transit db.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Holds the default edge profile for transfers.
        /// </summary>
        public static TagsCollectionBase DefaultEdgeProfile =
            new TagsCollection(Tag.Create("highway", "residential"));

        /// <summary>
        /// Adds a transfers db for pedestrians.
        /// </summary>
        public static void AddTransfersDbForPedestrians(this TransitDb db, float maxTimeInSeconds)
        {
            db.AddTransfersDb(OsmSharp.Routing.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), maxTimeInSeconds);
        }

        /// <summary>
        /// Adds a transfers db.
        /// </summary>
        public static void AddTransfersDb(this TransitDb db, Profiles.Profile profile,
            float maxTimeInSeconds)
        {
            db.AddTransfersDb(profile, TransitDbExtensions.DefaultEdgeProfile, maxTimeInSeconds);
        }
    }
}