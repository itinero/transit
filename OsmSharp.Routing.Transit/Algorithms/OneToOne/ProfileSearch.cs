﻿// OsmSharp - OpenStreetMap (OSM) SDK
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

using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Transit.Data;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two stops, with a given departure time, that has the best arrival time.
    /// </summary>
    public class ProfileSearch : AlgorithmBase
    {
        private readonly TransitDb _db;
        private readonly TransfersDb _transfersDb;
        private readonly uint _sourceStop;
        private readonly uint _targetStop;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<uint, DateTime, bool> _isTripPossible;
        private readonly uint _defaultTransferPentaly = 3 * 60;
        private Dictionary<uint, TripStatus> _tripStatuses;

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, uint sourceStop, uint targetStop, DateTime departureTime,
            Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = null;
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
        }

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, uint sourceStop, uint targetStop, DateTime departureTime,
            TransfersDb transfersDb, Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = transfersDb;
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, uint sourceStop, uint targetStop, DateTime departureTime,
            TransfersDb transfersDb, int minimumTransferTime, int maxmumSearchTime, Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = transfersDb;
            _sourceStop = sourceStop;
            _targetStop = targetStop;
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
            _minimumTransferTime = minimumTransferTime;
            _maximumSearchTime = maxmumSearchTime;
        }

        /// <summary>
        /// Gets the transit db.
        /// </summary>
        public TransitDb Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Gets the source stop.
        /// </summary>
        public uint SourceStop
        {
            get
            {
                return _sourceStop;
            }
        }

        /// <summary>
        /// Gets the target stop.
        /// </summary>
        public uint TargetStop
        {
            get
            {
                return _targetStop;
            }
        }

        /// <summary>
        /// Holds all the statuses of all stops that have been touched.
        /// </summary>
        private Dictionary<uint, StopProfileCollection> _profiles;

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
            var startTime = (uint)(_departureTime - _departureTime.Date).TotalSeconds;

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            var tripPerRoute = new Dictionary<int, int>(100);
            _tripStatuses = new Dictionary<uint, TripStatus>();

            // initialize stops status.
            _profiles = new Dictionary<uint, StopProfileCollection>();
            _profiles.Add(_sourceStop, new StopProfileCollection(new StopProfile()
            {
                PreviousConnectionId = Constants.NoConnectionId,
                Seconds = startTime
            }));
            StopProfileCollection targetProfiles = null;

            // keep a list of possible target stops.
            var targetProfilesTime = double.MaxValue;

            // get enumerators.
            var enumerator = _db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);
            TransfersDb.TransfersEnumerator transferEnumerator = null;
            if (_transfersDb != null)
            {
                transferEnumerator = _transfersDb.GetTransferEnumerator();

                if (transferEnumerator.MoveTo(_sourceStop))
                { // there may be transfers from the source-stop.
                    while (transferEnumerator.MoveNext())
                    {
                        StopProfileCollection transferTargetProfiles = null;
                        if (!_profiles.TryGetValue(transferEnumerator.Stop, out transferTargetProfiles))
                        { // create new empty transfer target profile collection.
                            transferTargetProfiles = new StopProfileCollection();
                            _profiles.Add(transferEnumerator.Stop, transferTargetProfiles);
                        }

                        transferTargetProfiles.UpdateStatus(1, new StopProfile()
                        {
                            PreviousConnectionId = Constants.TransferConnectionId,
                            Seconds = startTime + transferEnumerator.Seconds
                        });
                    }
                }
            }

            // search until first useful connection.
            if (!enumerator.MoveToDepartureTime(startTime))
            { // there are no connections with a departure later than start time.
                this.HasSucceeded = false;
                return;
            }

            do
            { // scan all connections.
                var connectionId = enumerator.Id;
                var departureTime = enumerator.DepartureTime + (day * Constants.OneDayInSeconds);

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

                //// check if route was visited already.
                //int routeTripId;
                //if (connection.TripId != Constants.NoRouteId &&
                //    tripPerRoute.TryGetValue(connection.TripId, out routeTripId) &&
                //    routeTripId != connection.TripId)
                //{ // a different trip, but same route, do not consider again.
                //    continue;
                //}
                //tripPerRoute[connection.RouteId] = connection.TripId;

                StopProfileCollection departureProfiles;
                if (_profiles.TryGetValue(enumerator.DepartureStop, out departureProfiles))
                { // stop was visited, has a status.// first time on this trip.
                    var tripReached = false;
                    var tripStatus = new TripStatus();
                    if (_tripStatuses.TryGetValue(enumerator.TripId, out tripStatus))
                    { // trip was already reached.
                        tripReached = true;
                    }

                    // latest arrival time in case of a transfer.
                    var latestArrivalTime = enumerator.DepartureTime - _minimumTransferTime;

                    // build new profile or get existing.
                    StopProfileCollection arrivalProfiles = null;
                    if (!_profiles.TryGetValue(enumerator.ArrivalStop, out arrivalProfiles))
                    { // create new empty arrival profiles.
                        arrivalProfiles = new StopProfileCollection();
                    }

                    if (tripReached)
                    { // if trip was reached, there is only one good option.
                        arrivalProfiles.UpdateStatus(tripStatus.Transfers, new StopProfile()
                        {
                            PreviousConnectionId = connectionId,
                            Seconds = enumerator.ArrivalTime
                        });

                        // check if this trip was reached with less transfers at this stop.
                        for (var i = 0; i < tripStatus.Transfers - 4 && i < departureProfiles.Count; i++)
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
                            _tripStatuses[enumerator.TripId] = new TripStatus()
                            {
                                Transfers = i + 2,
                                StopId = enumerator.DepartureStop
                            };
                            break;
                        }
                    }
                    else
                    { // if trip was not found have a look and see if we can transfer to this trip at this connection.
                        var tripTransfers = int.MaxValue;
                        for (var i = departureProfiles.Count - 1; i >= 0; i--)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.Seconds == Constants.NoSeconds)
                            { // no source at this transfer count.
                                continue;
                            }

                            // check if connection is reachable.
                            if (sourceProfile.Seconds > latestArrivalTime)
                            { // source arrives too late for this connection, all other source have higher or equal arrival times.
                                break;
                            }

                            // ok, there is an actual move possible here.
                            var transfers = 2;
                            if (sourceProfile.PreviousConnectionId == Constants.TransferConnectionId)
                            {
                                transfers = 1;
                            }
                            arrivalProfiles.UpdateStatus(i + transfers, new StopProfile()
                            {
                                PreviousConnectionId = connectionId,
                                Seconds = enumerator.ArrivalTime
                            });
                            if (i + transfers < tripTransfers)
                            { // keep the lowest transfer count for this trip.
                                tripTransfers = i + transfers;
                            }
                        }

                        if (tripTransfers < int.MaxValue)
                        { // trip was not found, but was reached.
                            _tripStatuses[enumerator.TripId] = new TripStatus()
                            {
                                StopId = enumerator.DepartureStop,
                                Transfers = tripTransfers
                            };
                        }
                    }

                    if (arrivalProfiles.Count > 0)
                    { // make sure that the arrival profiles are set.
                        _profiles[enumerator.ArrivalStop] = arrivalProfiles;

                        if (transferEnumerator != null &&
                            transferEnumerator.MoveTo(enumerator.ArrivalStop))
                        { // there may be transfers.
                            while (transferEnumerator.MoveNext())
                            {
                                StopProfileCollection transferTargetProfiles = null;
                                if (!_profiles.TryGetValue(transferEnumerator.Stop, out transferTargetProfiles))
                                { // create new empty transfer target profile collection.
                                    transferTargetProfiles = new StopProfileCollection();
                                    _profiles.Add(transferEnumerator.Stop, transferTargetProfiles);
                                }

                                for (var t = arrivalProfiles.Count - 1; t >= 0; t--)
                                {
                                    var sourceProfile = arrivalProfiles[t];
                                    if (sourceProfile.Seconds == Constants.NoSeconds)
                                    { // no source at this transfer count.
                                        continue;
                                    }

                                    transferTargetProfiles.UpdateStatus(t + 1, new StopProfile()
                                    {
                                        PreviousConnectionId = Constants.TransferConnectionId,
                                        Seconds = enumerator.ArrivalTime + transferEnumerator.Seconds
                                    });
                                }
                            }
                        }

                        // check target.
                        if (enumerator.ArrivalStop == _targetStop)
                        { // update target status.
                            targetProfiles = _profiles[enumerator.ArrivalStop];
                            this.HasSucceeded = true;
                        }
                    }
                }
            } while (enumerator.MoveNext());
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
        public long Duration()
        {
            this.CheckHasRunAndHasSucceeded();

            return _profiles[_targetStop].Seconds - (int)(_departureTime - _departureTime.Date).TotalSeconds;
        }

        /// <summary>
        /// Gets the status for the given stop.
        /// </summary>
        public IReadOnlyList<StopProfile> GetStopProfiles(uint stopId)
        {
            this.CheckHasRunAndHasSucceeded();

            StopProfileCollection profiles;
            if (!_profiles.TryGetValue(stopId, out profiles))
            { // status not found.
                return new StopProfileCollection();
            }
            return profiles;
        }

        /// <summary>
        /// A collections of profiles indexed per #transfers.
        /// </summary>
        public class StopProfileCollection : List<StopProfile>
        {
            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public StopProfileCollection()
            {

            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public StopProfileCollection(StopProfile profile)
            {
                this.UpdateStatus(0, profile);
            }

            /// <summary>
            /// Creates a new profile collection.
            /// </summary>
            public StopProfileCollection(int transfers, StopProfile profile)
            {
                this.UpdateStatus(transfers, profile);
            }

            /// <summary>
            /// Updates a profile for the given number of transfers.
            /// </summary>
            public bool UpdateStatus(int transfers, StopProfile profile)
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
                        this.Add(StopProfile.Empty);
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
                            (this[i].PreviousConnectionId == Constants.NoConnectionId &&
                             this[i].Seconds == Constants.NoSeconds))
                        {
                            if (i == this.Count - 1)
                            { // remove last if it would be set to empty.
                                this.RemoveAt(i);
                            }
                            else
                            { // ... or empty out if not the last entry.
                                this[i] = StopProfile.Empty;
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
            public uint Seconds
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
        public uint StopId { get; set; }

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
    public struct StopProfile
    {
        /// <summary>
        /// Gets or sets the previous connection id.
        /// </summary>
        public uint PreviousConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the second.
        /// </summary>
        public uint Seconds { get; set; }

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
        public static StopProfile Empty = new StopProfile()
        {
            PreviousConnectionId = Constants.NoConnectionId,
            Seconds = Constants.NoSeconds
        };
    }
}