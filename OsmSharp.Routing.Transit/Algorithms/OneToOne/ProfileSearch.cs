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

using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two stops, with a given departure time, that has the best arrival time.
    /// </summary>
    public class ProfileSearch : RoutingAlgorithmBase
    {
        private readonly ConnectionsView _connections;
        private readonly int _sourceStop;
        private readonly int _targetStop;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly Func<Profile, Profile, int> _compareStatuses = (status1, status2) =>
        {
            if(status1.Seconds == status2.Seconds)
            {
                return status1.Transfers.CompareTo(status2.Transfers);
            }
            return status1.Seconds.CompareTo(status2.Seconds);
        };

        /// <summary>
        /// Creates a new instance of the earliest arrival algorithm.
        /// </summary>
        /// <param name="connections">The connections db.</param>
        /// <param name="sourceStop">The stop to start at.</param>
        /// <param name="targetStop">The stop to end at.</param>
        /// <param name="departureTime">The departure time.</param>
        public ProfileSearch(GTFSConnectionsDb connections, int sourceStop, int targetStop, DateTime departureTime)
        {
            _connections = connections.GetDepartureTimeView();
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = connections.IsTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        /// <param name="connections">The connections db.</param>
        /// <param name="sourceStop">The stop to start at.</param>
        /// <param name="targetStop">The stop to end at.</param>
        /// <param name="departureTime">The departure time.</param>
        /// <param name="minimumTransferTime">The minimum transfer time (default: 3 * 60s).</param>
        /// <param name="maxmumSearchTime">The maximum search time (default: one day in seconds).</param>
        /// <param name="isTripPossible">The function to check if a trip is possible (default: connections.IsTripPossible).</param>
        /// <param name="compareStatuses">The function to compare a status at a stop of two distinct statuses are found at one stop (default: the lowest seconds, than the lowest transfer count).</param>
        public ProfileSearch(GTFSConnectionsDb connections, int sourceStop, int targetStop, DateTime departureTime,
            int minimumTransferTime, int maxmumSearchTime, Func<int, DateTime, bool> isTripPossible, Func<Profile, Profile, int> compareStatuses)
        {
            _connections = connections.GetDepartureTimeView();
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
            _minimumTransferTime = minimumTransferTime;
            _maximumSearchTime = maxmumSearchTime;
            _compareStatuses = compareStatuses;
        }

        /// <summary>
        /// Gets the source stop.
        /// </summary>
        public int SourceStop
        {
            get
            {
                return _sourceStop;
            }
        }

        /// <summary>
        /// Gets the target stop.
        /// </summary>
        public int TargetStop
        {
            get
            {
                return _targetStop;
            }
        }

        /// <summary>
        /// Holds all the statuses of all stops that have been touched.
        /// </summary>
        private Dictionary<int, ProfileCollection> _profiles;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // Remarks:
            // - Use the number of seconds from the previous midnight, this is also what is used to sort the connections.
            // - Use the date to determine if a trip is possible.
            // - When the midnight barries is passed, increase the date.
            var date = _departureTime.Date;
            var day = 0;
            var startTime = (int)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();

            // initialize stops status.
            _profiles = new Dictionary<int, ProfileCollection>();
            _profiles.Add(_sourceStop, new ProfileCollection(new Profile()
            {
                ConnectionId = Constants.NoConnectionId,
                Seconds = startTime,
                Transfers = 0
            }));
            ProfileCollection targetProfiles = null;

            for (var connectionId = 0; connectionId < _connections.Count; connectionId++)
            { // scan all connections.
                var connection = _connections[connectionId];
                var departureTime = connection.DepartureTime + (day * Constants.OneDayInSeconds);

                // check max search time.
                if (departureTime - startTime > _maximumSearchTime)
                { // maximum search time exceeded.
                    break; // stop searching without finding a result.
                }

                // check if target has been reached and if departure time exceeds target arrival time.
                if (targetProfiles != null && departureTime >= targetProfiles.Seconds)
                { // the current status at 'to' is the best status it's ever going to get.
                    break;
                }

                ProfileCollection departureProfiles;
                if (_profiles.TryGetValue(connection.DepartureStop, out departureProfiles))
                { // stop was visited, has a status.
                    foreach (var departureProfile in departureProfiles)
                    {
                        var transferTime = _minimumTransferTime;
                        var tripPossible = false;
                        var transfer = 1;
                        var departureProfileTripId = Constants.NoConnectionId;
                        if (departureProfile.ConnectionId != Constants.NoConnectionId)
                        { // connection is set.
                            departureProfileTripId = _connections[departureProfile.ConnectionId].TripId;
                        }
                        if (departureProfileTripId == connection.TripId ||
                            (departureProfileTripId != Constants.PseudoConnectionTripId && connection.TripId == Constants.PseudoConnectionTripId) ||
                            (departureProfileTripId == Constants.PseudoConnectionTripId && connection.TripId != Constants.PseudoConnectionTripId))
                        { // the same trip.
                            transferTime = 0;
                            tripPossible = true; // trip is possible because it is already used.
                            transfer = 0;
                            if (connection.TripId == Constants.PseudoConnectionTripId)
                            { // consider this a transfer because the connection itself is a transfer.
                                transfer = 1;
                            }
                        }

                        if (departureProfile.Seconds <= departureTime - transferTime)
                        { // a departure here is possible if the trip is possible.
                            if (!tripPossibilities.TryGetValue(connection.TripId, out tripPossible))
                            { // trip was not checked yet.
                                tripPossible = _isTripPossible.Invoke(connection.TripId, date);
                                tripPossibilities.Add(connection.TripId, tripPossible);
                            }

                            if (tripPossible)
                            { // trip is possible.
                                // calculate status at the target stop if this trip is taken.
                                var arrivalProfile = new Profile()
                                {
                                    ConnectionId = connectionId,
                                    Transfers = (short)(departureProfile.Transfers + transfer),
                                    Seconds = connection.ArrivalTime
                                };

                                ProfileCollection arrivalProfiles;
                                if (_profiles.TryGetValue(connection.ArrivalStop, out arrivalProfiles))
                                { // compare statuses if already a status there.
                                    arrivalProfiles.Add(arrivalProfile);
                                }
                                else
                                { // no status yet, just set it.
                                    _profiles.Add(connection.ArrivalStop, new ProfileCollection(arrivalProfile));
                                }

                                // check target.
                                if (connection.ArrivalStop == _targetStop)
                                { // update toStatus.
                                    targetProfiles = _profiles[connection.ArrivalStop];
                                    this.HasSucceeded = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the calculated arrival time.
        /// </summary>
        /// <returns></returns>
        public DateTime ArrivalTime()
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(_profiles[_targetStop].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public int Duration()
        {
            this.CheckHasRunAndHasSucceeded();

            return _profiles[_targetStop].Seconds - (int)(_departureTime - _departureTime.Date).TotalSeconds;
        }

        /// <summary>
        /// Gets the status for the given stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public IReadOnlyList<Profile> GetStopProfiles(int stopId)
        {
            this.CheckHasRunAndHasSucceeded();

            ProfileCollection profiles;
            if(!_profiles.TryGetValue(stopId, out profiles))
            { // status not found.
                return new ProfileCollection();
            }
            return profiles;
        }

        /// <summary>
        /// Gets the connection with the given id.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Connection GetConnection(int connectionId)
        {
            return _connections[connectionId];
        }

        /// <summary>
        /// A profile collection that just points to the first and last profile of a linked-list.
        /// </summary>
        private class ProfileCollection : List<Profile>
        {
            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection()
            {

            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection(Profile profile)
                : base(1)
            {
                this.Add(profile);

                this.Seconds = profile.Seconds;
                this.Lazyness = 0;
            }

            /// <summary>
            /// Gets or sets the time in seconds.
            /// </summary>
            public int Seconds { get; private set; }

            /// <summary>
            /// Gets or sets the lazyness.
            /// </summary>
            public int Lazyness { get; private set; }

            /// <summary>
            /// Tries to add a profile and removes all entries that are dominated.
            /// </summary>
            /// <param name="profile"></param>
            /// <returns></returns>
            public bool TryAdd(Profile profile)
            {
                var i = this.Count - 1;
                while(i >= 0)
                {
                    if(profile.ConnectionId != this[i].ConnectionId)
                    { // no use comparing profiles on different connections.
                        break;
                    }
                    var domination = this.Dominates(this[i], profile);
                    if(domination.HasValue)
                    {
                        if(domination.Value)
                        { // profile is dominated, do not add.
                            return false;
                        }
                        else
                        { // remove newly dominated profile.
                            this.RemoveAt(i);
                        }
                    }
                    i--;
                }

                // when we get to this point no domination of profile.
                this.Add(profile);
                if(this.Lazyness + this.Seconds > profile.Seconds)
                {
                    this.Seconds = profile.Seconds;
                    this.Lazyness = 0;
                }
                return true;
            }

            /// <summary>
            /// Figures out if, of the two given profiles, one dominates the other.
            /// </summary>
            /// <returns>null if there is no domination, true if profile1 dominates profile2 and false if profile2 dominates profile1.</returns>
            private bool? Dominates(Profile profile1, Profile profile2)
            {
                if(profile1.ConnectionId == profile2.ConnectionId)
                { // only compare profiles on the same connection.
                    if(profile1.Transfers != profile2.Transfers)
                    { // transfer count is different, one will definetly dominate.
                        return profile1.Transfers.CompareTo(profile2.Transfers) > 0;
                    }
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Represents a stop status. 
    /// </summary>
    /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
    public struct Profile
    {
        /// <summary>
        /// Gets or sets the connection id of the connection used.
        /// </summary>
        public int ConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the # of transfers.
        /// </summary>
        public short Transfers { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds from source.
        /// </summary>
        public int Seconds { get; set; }
    }

    /// <summary>
    /// Extension methods for profile collections.
    /// </summary>
    public static class ProfileCollectionExtensions
    {
        /// <summary>
        /// Gets one of the best profiles.
        /// </summary>
        /// <returns></returns>
        public static Profile GetBest(this IReadOnlyList<Profile> profileCollection)
        {
            Profile? found = null;
            foreach(var profile in profileCollection)
            {
                if(found == null)
                {
                    found = profile;
                }
                else if(found.Value.Seconds > profile.Seconds)
                {
                    found = profile;
                }
                else if(found.Value.Seconds == profile.Seconds &&
                    found.Value.Transfers > profile.Transfers)
                {
                    found = profile;
                }
            }
            return found.Value;
        }
    }
}