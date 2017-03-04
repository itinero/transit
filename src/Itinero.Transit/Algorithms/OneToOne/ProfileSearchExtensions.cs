// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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