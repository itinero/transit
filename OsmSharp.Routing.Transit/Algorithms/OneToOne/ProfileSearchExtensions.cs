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

using System.Collections.Generic;
namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// Profile search extensions.
    /// </summary>
    public static class ProfileSearchExtensions
    {
        /// <summary>
        /// Gets the profile that precedes the given one on a transit route.
        /// </summary>
        public static Profile GetPreceding(this ProfileSearch algorithm, IReadOnlyList<Profile> profiles,
            int i, out int stopId, out int tranfers)
        {
            var profile = profiles[i];
            if (profile.PreviousConnectionId == Constants.NoConnectionId)
            { // no previous connection, no preceding profile.
                stopId = Constants.NoStopId;
                tranfers = Constants.NoTransfers;
                return Profile.Empty;
            }

            // get preceding connection.
            var connection = algorithm.GetConnection(profile.PreviousConnectionId);
            stopId = connection.DepartureStop;
            var profileCandidates = algorithm.GetStopProfiles(connection.DepartureStop);

            // find exact match, same trip, no transfer.
            Profile candidate;
            if (i < profileCandidates.Count)
            { // check at same transfer count.
                candidate = profileCandidates[i];
                if (candidate.PreviousConnectionId == connection.PreviousConnectionId)
                {
                    tranfers = i;
                    return candidate;
                }
            }

            // not an exact match, return the one with the least transfers.
            for (var c = 0; c < i; c++)
            {
                candidate = profileCandidates[c];
                if(candidate.Seconds < 0)
                { // empty.
                    continue;
                }
                if(candidate.Seconds < connection.DepartureTime)
                {
                    tranfers = c;
                    return candidate;
                }
            }
            tranfers = Constants.NoTransfers;
            stopId = Constants.NoStopId;
            return Profile.Empty;
        }
    }
}