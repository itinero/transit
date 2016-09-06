// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Profiles;
using System;

namespace Itinero.Transit
{
    /// <summary>
    /// Abstract representation of a multimodal router.
    /// </summary>
    public abstract class MultimodalRouterBase
    {
        /// <summary>
        /// Gets the core router.
        /// </summary>
        public abstract Router Router
        {
            get;
        }

        /// <summary>
        /// Tries to calculate an earliest arrival route from stop1 to stop2.
        /// </summary>
        public abstract Result<Route> TryEarliestArrival(DateTime departureTime,
            RouterPoint sourcePoint, Profile sourceProfile, RouterPoint targetPoint, Profile targetProfile, 
                EarliestArrivalSettings settings);
    }

    /// <summary>
    /// Contains earliest arrival settings.
    /// </summary>
    public class EarliestArrivalSettings
    {
        /// <summary>
        /// Gets or sets the maximum seconds for the source.
        /// </summary>
        public int MaxSecondsSource { get; set; }

        /// <summary>
        /// Gets or sets the maximum seconds for the target.
        /// </summary>
        public int MaxSecondsTarget { get; set; }

        /// <summary>
        /// Gets or sets the use agency function.
        /// </summary>
        public Func<uint, bool> UseAgency { get; set; }

        /// <summary>
        /// Gets the default earliest arrival settings.
        /// </summary>
        public static EarliestArrivalSettings Default
        {
            get
            {
                return new EarliestArrivalSettings()
                {
                    MaxSecondsSource = 1800,
                    MaxSecondsTarget = 1800,
                    UseAgency = null
                };
            }
        }
    }
}