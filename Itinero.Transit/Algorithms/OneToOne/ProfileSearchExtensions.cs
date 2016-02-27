// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using System;
using System.Collections.Generic;

namespace Itinero.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// Profile search extensions.
    /// </summary>
    public static class ProfileSearchExtensions
    {
        /// <summary>
        /// Gets the profile with the least transfers.
        /// </summary>
        /// <returns></returns>
        public static int GetLeastTransfers(this IReadOnlyList<StopProfile> profiles)
        {
            for (var i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Seconds != Constants.NoSeconds)
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException("profile", "Profile collection doesn't contain any valid entries.");
        }

        /// <summary>
        /// Gets the profile with the minimum travel time.
        /// </summary>
        /// <returns></returns>
        public static int GetMinimumTraveltime(this IReadOnlyList<StopProfile> profiles)
        {
            for (var i = profiles.Count - 1; i < 0; i++)
            {
                if (profiles[i].Seconds != Constants.NoSeconds)
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException("profile", "Profile collection doesn't contain any valid entries.");
        }

        /// <summary>
        /// Gets the best stop profile.
        /// </summary>
        public static int GetBest(this ProfileSearch algorithm, IReadOnlyList<StopProfile> profiles,
            uint maxTransferCostInSeconds)
        {
            uint bestTime = uint.MaxValue;
            int bestIndex = int.MaxValue;
            for (var i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Seconds != Constants.NoSeconds)
                {
                    uint time = ((uint)i * maxTransferCostInSeconds) +
                        profiles[i].Seconds;
                    if(time < bestTime)
                    {
                        bestIndex = i;
                        bestTime = time;
                    }
                }
            }
            if(bestIndex == int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("profile", "Profile collection doesn't contain any valid entries.");
            }
            return bestIndex;
        }

        /// <summary>
        /// Gets the profile that precedes the given one on a transit route.
        /// </summary>
        public static int GetPreceding(this IReadOnlyList<StopProfile> profiles, ProfileSearch search,
            int i)
        {
            var profile = profiles[i];
            if (profile.IsEmpty)
            { // no previous connection, no preceding profile.
                return -1;
            }

            // get preceding connection.
            var connections = search.Db.GetConnectionsEnumerator(Data.DefaultSorting.DepartureTime);
            connections.MoveTo(profile.PreviousConnectionId);
            var stopId = connections.DepartureStop;
            var departureTime = connections.DepartureTime;
            var profileId = connections.TripId;

            // get candidates and search best match.
            var profileCandidates = search.GetStopProfiles(connections.DepartureStop);

            // find exact match, same trip, no transfer.
            StopProfile candidate;
            if (i < profileCandidates.Count)
            { // check at same transfer count.
                candidate = profileCandidates[i];
                connections.MoveTo(candidate.PreviousConnectionId);
                if (connections.TripId == profileId)
                {
                    return i;
                }
            }

            // not an exact match, return the one with the least transfers.
            for (var c = 0; c < i; c++)
            {
                candidate = profileCandidates[c];
                if (candidate.Seconds < 0)
                { // empty.
                    continue;
                }
                if (candidate.Seconds < departureTime)
                {
                    return c;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the profile that precedes the given one on a transit route.
        /// </summary>
        public static StopProfile GetPreceding(this ProfileSearch algorithm, IReadOnlyList<StopProfile> profiles,
            int i, out uint stopId, out int tranfers)
        {
            var profile = profiles[i];
            if (profile.IsEmpty)
            { // no previous connection, no preceding profile.
                stopId = Constants.NoStopId;
                tranfers = Constants.NoTransfers;
                return StopProfile.Empty;
            }

            // get preceding connection.
            var connections = algorithm.Db.GetConnectionsEnumerator(Data.DefaultSorting.DepartureTime);
            connections.MoveTo(profile.PreviousConnectionId);
            stopId = connections.DepartureStop;
            var departureTime = connections.DepartureTime;
            var profileId = connections.TripId;

            // get candidates and search best match.
            var profileCandidates = algorithm.GetStopProfiles(connections.DepartureStop);

            // find exact match, same trip, no transfer.
            StopProfile candidate;
            if (i < profileCandidates.Count)
            { // check at same transfer count.
                candidate = profileCandidates[i];
                connections.MoveTo(candidate.PreviousConnectionId);
                if (connections.TripId == profileId)
                {
                    tranfers = i;
                    return candidate;
                }
            }

            // not an exact match, return the one with the least transfers.
            for (var c = 0; c < i; c++)
            {
                candidate = profileCandidates[c];
                if (candidate.Seconds < 0)
                { // empty.
                    continue;
                }
                if (candidate.Seconds < departureTime)
                {
                    tranfers = c;
                    return candidate;
                }
            }
            tranfers = Constants.NoTransfers;
            stopId = Constants.NoStopId;
            return StopProfile.Empty;
        }
    }
}