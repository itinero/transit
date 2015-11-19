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
    public class ProfileSearch : RoutingAlgorithmBase, IConnectionList
    {
        private readonly ConnectionsView _connections;
        private readonly int _sourceStop;
        private readonly int _targetStop;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<int, DateTime, bool> _isTripPossible;
        private readonly int _defaultTransferPentaly = 5 * 60;
        private Dictionary<int, TripStatus> _tripStatuses;

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
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
        public ProfileSearch(GTFSConnectionsDb connections, int sourceStop, int targetStop, DateTime departureTime,
            int minimumTransferTime, int maxmumSearchTime, Func<int, DateTime, bool> isTripPossible)
        {
            _connections = connections.GetDepartureTimeView();
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
            _minimumTransferTime = minimumTransferTime;
            _maximumSearchTime = maxmumSearchTime;
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
            var tripPerRoute = new Dictionary<int, int>(100);
            _tripStatuses = new Dictionary<int, TripStatus>();

            // initialize stops status.
            _profiles = new Dictionary<int, ProfileCollection>();
            _profiles.Add(_sourceStop, new ProfileCollection(new Profile()
            {
                PreviousConnectionId = Constants.NoConnectionId,
                Seconds = startTime
            }));
            ProfileCollection targetProfiles = null;

            // keep a list of possible target stops.
            var targetProfilesTime = double.MaxValue;
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
                if (departureTime - targetProfilesTime >= (_defaultTransferPentaly * 5))
                { // the current status at 'to' is the best status it's ever going to get.
                    break;
                }

                // check if route was visited already.
                int routeTripId;
                if (connection.TripId != Constants.NoRouteId &&
                    tripPerRoute.TryGetValue(connection.TripId, out routeTripId) &&
                    routeTripId != connection.TripId)
                { // a different trip, but same route, do not consider again.
                    continue;
                }
                tripPerRoute[connection.RouteId] = connection.TripId;

                ProfileCollection departureProfiles;
                if (_profiles.TryGetValue(connection.DepartureStop, out departureProfiles))
                { // stop was visited, has a status.// first time on this trip.
                    var tripFound = false;
                    var tripStatus = new TripStatus();
                    if (connection.TripId != Constants.PseudoConnectionTripId &&
                        _tripStatuses.TryGetValue(connection.TripId, out tripStatus))
                    { // trip was found.
                        tripFound = true;
                    }

                    // latest arrival time in case of a transfer.
                    var latestArrivalTime = connection.DepartureTime - _minimumTransferTime;

                    // build new profile.
                    ProfileCollection arrivalProfiles = null;
                    if (!_profiles.TryGetValue(connection.ArrivalStop, out arrivalProfiles))
                    { // create new empty arrival profiles.
                        arrivalProfiles = new ProfileCollection();
                    }

                    if (tripFound)
                    { // if trip was found, there is only one good option.
                        arrivalProfiles.UpdateStatus(tripStatus.Transfers, new Profile()
                        {
                            PreviousConnectionId = connectionId,
                            Seconds = connection.ArrivalTime
                        });

                        // check if now this trip was reached with less transfers at this stop.
                        for (var i = 0; i < tripStatus.Transfers - 2 && i < departureProfiles.Count; i++)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.Seconds == Constants.NoSeconds)
                            { // no source at this transfer count.
                                continue;
                            }

                            // check if connection is reachable.
                            if (sourceProfile.Seconds > latestArrivalTime)
                            { // source arrives too late for this connection, all other source have higher or equal arrival times.
                                continue;
                            }

                            // ok here, this should lead to one less transfer.
                            _tripStatuses[connection.TripId] = new TripStatus()
                            {
                                Transfers = i + 1,
                                StopId = connection.DepartureStop
                            };
                            break;
                        }
                    }
                    else
                    { // if trip was not found have a look and see if we can tranfer to this connection.
                        var tripTransfers = int.MaxValue;
                        for (var i = departureProfiles.Count - 1; i >= 0; i--)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.Seconds == Constants.NoSeconds)
                            { // no source at this transfer count.
                                continue;
                            }

                            // check if connection is reachable.
                            var transfer = 1;
                            if (sourceProfile.Seconds > latestArrivalTime)
                            { // source arrives too late for this connection, all other source have higher or equal arrival times.
                                var previousIsPseudo = false;
                                if (sourceProfile.PreviousConnectionId >= 0)
                                { // check if previous connection is a pseudo connection.
                                    var previousConnection = _connections[sourceProfile.PreviousConnectionId];
                                    previousIsPseudo = previousConnection.TripId == Constants.PseudoConnectionTripId;
                                }
                                if (connection.TripId != Constants.PseudoConnectionTripId && 
                                    !previousIsPseudo)
                                { // connection is a regular trip.
                                    continue;
                                }
                                if (sourceProfile.Seconds > connection.DepartureTime)
                                { // connection is a pseudo connection, but arrival is too late.
                                    break;
                                }
                                if (connection.TripId == Constants.PseudoConnectionTripId)
                                { // going to a pseudo connection is not a transfer, but coming from one is.
                                    transfer = 0; 
                                }
                            }
                            else
                            {
                                if (connection.PreviousConnectionId == Constants.PseudoConnectionTripId)
                                { // going to a pseudo connection is not a transfer.
                                    transfer = 0;
                                }
                            }

                            // ok, there is an actual move possible here.
                            arrivalProfiles.UpdateStatus(i + transfer, new Profile()
                            {
                                PreviousConnectionId = connectionId,
                                Seconds = connection.ArrivalTime
                            });
                            if (i + transfer < tripTransfers)
                            { // keep the lowest transfer count for this trip.
                                tripTransfers = i + transfer;
                            }
                        }

                        if (tripTransfers < int.MaxValue &&
                            connection.TripId != Constants.PseudoConnectionTripId)
                        { // trip was not found, but was reached.
                            _tripStatuses[connection.TripId] = new TripStatus()
                            {
                                StopId = connection.DepartureStop,
                                Transfers = tripTransfers
                            };
                        }
                    }

                    if (arrivalProfiles.Count > 0)
                    { // make sure that the arrival profiles are set.
                        _profiles[connection.ArrivalStop] = arrivalProfiles;

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
            if (!_profiles.TryGetValue(stopId, out profiles))
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
        /// A collections of profiles indexed per #transfers.
        /// </summary>
        public class ProfileCollection : List<Profile>
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
            {
                this.UpdateStatus(0, profile);
            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public ProfileCollection(int transfers, Profile profile)
            {
                this.UpdateStatus(transfers, profile);
            }

            /// <summary>
            /// Updates a profile for the given number of transfers.
            /// </summary>
            public bool UpdateStatus(int transfers, Profile profile)
            {
                if (this.Count > 0)
                { // check if dominated by latest entry.
                    if (this.Count - 1 <= transfers &&
                        this[this.Count - 1].Seconds <= profile.Seconds)
                    { // dominated by latest, do nothing.
                        return false;
                    }
                }
                if (this.Count - 1 < transfers)
                { // no profile yet at this transfer, just update list and insert.
                    do
                    {
                        this.Add(Profile.Empty);
                    } while (this.Count - 1 < transfers);
                    this[transfers] = profile;
                    return true;
                }
                else
                { // yes, there is a profile, compare it and remove dominated entries if needed.
                    for (var i = this.Count - 1; i > transfers; i--)
                    {
                        if ((this[i].PreviousConnectionId != Constants.NoConnectionId &&
                             this[i].Seconds >= profile.Seconds) ||
                            (this[i].PreviousConnectionId == -1 &&
                             this[i].Seconds == -1))
                        {
                            if (i == this.Count - 1)
                            { // remove last if it would be set to empty.
                                this.RemoveAt(i);
                            }
                            else
                            { // ... or empty out if not the last entry.
                                this[i] = Profile.Empty;
                            }
                        }
                    }

                    if (this[transfers].PreviousConnectionId == Constants.NoConnectionId)
                    {
                        this[transfers] = profile;
                        return true;
                    }
                    else if (this[transfers].Seconds > profile.Seconds)
                    {
                        this[transfers] = profile;
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Gets the seconds.
            /// </summary>
            public int Seconds
            {
                get
                {
                    return this[this.Count - 1].Seconds;
                }
            }
        }
    }

    /// <summary>
    /// Represents a trip status.
    /// 
    /// Keep the first stop possible to reach this trip.
    /// Keep the #transfers to reach this trip.
    /// </summary>
    public struct TripStatus
    {
        /// <summary>
        /// Gets or sets the stop id.
        /// </summary>
        public int StopId { get; set; }

        /// <summary>
        /// Gets or sets the transfer count.
        /// </summary>
        public int Transfers { get; set; }

        /// <summary>
        /// Returns a description of this trip status.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("@{0}", this.StopId);
        }
    }

    /// <summary>
    /// Represents a stop status. 
    /// </summary>
    /// <remarks>A stop status represents information about how the current stop was reached.</remarks>
    public struct Profile
    {
        /// <summary>
        /// Gets or sets the previous connection id.
        /// </summary>
        public int PreviousConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the second.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Return a description of this profile.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}@{1}", this.PreviousConnectionId, this.Seconds);
        }

        /// <summary>
        /// Gets the default empty profile.
        /// </summary>
        public static Profile Empty = new Profile()
        {
            PreviousConnectionId = Constants.NoConnectionId,
            Seconds = Constants.NoSeconds
        };
    }
}