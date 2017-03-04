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

using Itinero.Algorithms;
using Itinero.Transit.Data;
using System;
using System.Collections.Generic;

namespace Itinero.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// An algorithm that calculates a one-to-one path between two stops, with a given departure time, that has the best arrival time.
    /// </summary>
    public class ProfileSearch : AlgorithmBase
    {
        private readonly TransitDb _db;
        private readonly TransfersDb _transfersDb;
        private readonly Dictionary<uint, uint> _sources;
        private readonly Dictionary<uint, uint> _targets;
        private readonly DateTime _departureTime;
        private readonly int _maximumSearchTime = Constants.OneDayInSeconds;
        private readonly int _minimumTransferTime = 3 * 60;
        private readonly Func<uint, DateTime, bool> _isTripPossible;
        private readonly uint _defaultTransferPentaly = 3 * 60;

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, DateTime departureTime,
            Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = null;
            _sources = new Dictionary<uint, uint>();
            _targets = new Dictionary<uint, uint>();
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
        }

        /// <summary>
        /// Creates a new instance of the profile search algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, DateTime departureTime,
            TransfersDb transfersDb, Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = transfersDb;
            _sources = new Dictionary<uint, uint>();
            _targets = new Dictionary<uint, uint>();
            _departureTime = departureTime;

            _isTripPossible = isTripPossible;
        }

        /// <summary>
        /// Creates a new instance of earliest arrival algorithm.
        /// </summary>
        public ProfileSearch(TransitDb db, DateTime departureTime,
            TransfersDb transfersDb, int minimumTransferTime, int maxmumSearchTime, Func<uint, DateTime, bool> isTripPossible)
        {
            _db = db;
            _transfersDb = transfersDb;
            _sources = new Dictionary<uint, uint>();
            _targets = new Dictionary<uint, uint>();
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
        /// Sets a source stop.
        /// </summary>
        public void SetSourceStop(uint stopId, uint seconds)
        {
            if(this.HasRun) { throw new InvalidOperationException("Cannot set source stops after search has run."); }

            _sources[stopId] = seconds;
        }

        /// <summary>
        /// Gets the sources.
        /// </summary>
        public IReadOnlyDictionary<uint, uint> Sources
        {
            get
            {
                return _sources;
            }
        }
        
        /// <summary>
        /// Sets a target stop.
        /// </summary>
        public void SetTargetStop(uint stopId, uint seconds)
        {
            if (this.HasRun) { throw new InvalidOperationException("Cannot set target stops after search has run."); }

            _targets[stopId] = seconds;
        }

        /// <summary>
        /// Gets the targets.
        /// </summary>
        public IReadOnlyDictionary<uint, uint> TargetStop
        {
            get
            {
                return _targets;
            }
        }

        /// <summary>
        /// Gets the departure time.
        /// </summary>
        public uint DepartureTime
        {
            get
            {
                return (uint)(_departureTime - _departureTime.Date).TotalSeconds;
            }
        }

        /// <summary>
        /// Holds all the statuses of all stops that have been touched.
        /// </summary>
        private Dictionary<uint, StopProfileCollection> _profiles;
        
        /// <summary>
        /// Holds the status of all trips.
        /// </summary>
        private Dictionary<uint, TripStatus> _tripStatuses;

        /// <summary>
        /// Holds the best target stops per transfer count.
        /// </summary>
        private List<uint> _targetStops;

        /// <summary>
        /// Holds the target profiles per transfer count.
        /// </summary>
        private StopProfileCollection _targetProfiles;

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

            if (_sources.Count == 0)
            { // no sources.
                return;
            }
            if (_targets.Count == 0)
            { // no targets.
                return;
            }

            // initialize data structures.
            var tripPossibilities = new Dictionary<int, bool>();
            var tripPerRoute = new Dictionary<int, int>(100);
            _tripStatuses = new Dictionary<uint, TripStatus>();
            _targetProfiles = new StopProfileCollection();
            _targetStops = new List<uint>();

            // initialize stops status.
            _profiles = new Dictionary<uint, StopProfileCollection>();
            foreach(var source in _sources)
            {
                _profiles.Add(source.Key, new StopProfileCollection(new StopProfile()
                {
                    Seconds = source.Value
                }));
            }

            // keep a list of possible target stops.
            var targetProfilesTime = double.MaxValue;

            // get enumerators.
            var enumerator = _db.GetConnectionsEnumerator(DefaultSorting.DepartureTime);
            TransfersDb.TransfersEnumerator transferEnumerator = null;
            if (_transfersDb != null)
            {
                transferEnumerator = _transfersDb.GetTransferEnumerator();

                foreach (var source in _sources)
                {
                    if (transferEnumerator.MoveTo(source.Key))
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
                                PreviousStopId = source.Key,
                                Seconds = startTime + transferEnumerator.Seconds
                            });
                        }
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
                    var tripPossible = false;
                    var tripStatus = new TripStatus();
                    if (_tripStatuses.TryGetValue(enumerator.TripId, out tripStatus))
                    { // trip was already reached.
                        if(tripStatus.Transfers == int.MaxValue)
                        { // trip is impossible.
                            continue;
                        }
                        else if (tripStatus.Transfers == -1)
                        { // trip is possible but has not been reached yet.
                            tripPossible = true;
                            tripReached = false;
                        }
                        else
                        { // trip reached and possible.
                            tripReached = true;
                            tripPossible = true;
                        }
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

                        // check if this trip was reached with less transfers at this stop or a better one.
                        for (var i = 0; i < tripStatus.Transfers && i < departureProfiles.Count; i++)
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
                            var existingStatus = _tripStatuses[enumerator.TripId];
                            if (existingStatus.Transfers > i + 2)
                            { // # transfers smaller, always replace.
                                _tripStatuses[enumerator.TripId] = new TripStatus()
                                {
                                    Transfers = i + 2,
                                    StopId = enumerator.DepartureStop,
                                    DepartureTime = enumerator.DepartureTime
                                };
                            }
                            else if(existingStatus.Transfers == 2)
                            { // # transfers equal and point to the first stop, compare source times.
                                var existingStopTime = _profiles[existingStatus.StopId];
                                var potentialStopTime = _profiles[enumerator.DepartureStop];
                                if(existingStopTime[0].Seconds > potentialStopTime[0].Seconds)
                                { // the potential is an improvement.
                                    _tripStatuses[enumerator.TripId] = new TripStatus()
                                    {
                                        Transfers = i + 2,
                                        StopId = enumerator.DepartureStop,
                                        DepartureTime = enumerator.DepartureTime
                                    };
                                }
                            }

                            break;
                        }
                    }
                    else
                    { // if trip was not found have a look and see if we can transfer to this trip at this connection.
                        // first check if trip is possible if needed.
                        if(!tripPossible && 
                           !_isTripPossible(enumerator.TripId, _departureTime))
                        { // trip is not possible. 
                            _tripStatuses[enumerator.TripId] = new TripStatus()
                            {
                                StopId = Constants.NoStopId,
                                Transfers = int.MaxValue,
                                DepartureTime = uint.MaxValue
                            };
                            continue;
                        }
                        else
                        { // no yet reached, but possible.
                            _tripStatuses[enumerator.TripId] = new TripStatus()
                            {
                                StopId = Constants.NoStopId,
                                Transfers = -1,
                                DepartureTime = 0
                            };
                        }

                        // check if this trip can be transferred to.
                        var tripTransfers = int.MaxValue;
                        for (var i = departureProfiles.Count - 1; i >= 0; i--)
                        {
                            var sourceProfile = departureProfiles[i];
                            if (sourceProfile.IsEmpty)
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
                            if (sourceProfile.IsTransfer)
                            { // only increase by one, the previous profile was a transfer.
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
                                Transfers = tripTransfers,
                                DepartureTime = enumerator.DepartureTime
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
                                        PreviousStopId = enumerator.ArrivalStop,
                                        Seconds = enumerator.ArrivalTime + transferEnumerator.Seconds
                                    });
                                }
                            }
                        }

                        // check target.
                        uint targetSeconds;
                        if (_targets.TryGetValue(enumerator.ArrivalStop, out targetSeconds))
                        {
                            // build new target statuses per transfer count at this target stop.
                            var targetprofiles = _profiles[enumerator.ArrivalStop];
                            for(var i = 0; i < targetprofiles.Count; i++)
                            {
                                var targetProfile = targetprofiles[i];
                                if (!targetProfile.IsEmpty &&
                                    !targetProfile.IsFirst)
                                {
                                    var newTargetProfile = new StopProfile();
                                    newTargetProfile.Seconds = targetProfile.Seconds + targetSeconds;
                                    if(targetProfile.IsConnection)
                                    {
                                        newTargetProfile.PreviousConnectionId = targetProfile.PreviousConnectionId;
                                    }
                                    else if(targetProfile.IsTransfer)
                                    {
                                        newTargetProfile.PreviousStopId = targetProfile.PreviousStopId;
                                    }

                                    // try to update.
                                    if (_targetProfiles.UpdateStatus(i, newTargetProfile, t =>
                                        {
                                            if(_targetStops.Count - 1 == t)
                                            {
                                                _targetStops.RemoveAt(t);
                                            }
                                            else
                                            {
                                                _targetStops[t] = Constants.NoStopId;
                                            }
                                        }))
                                    {
                                        while (i >= _targetStops.Count)
                                        {
                                            _targetStops.Add(Constants.NoStopId);
                                        }
                                        _targetStops[i] = enumerator.ArrivalStop;
                                        this.HasSucceeded = true;
                                    }
                                }
                            }

                            // set the arrival time as the one with the least transfers (and thus the biggest travel time) and the time to target.
                            if(arrivalProfiles.Seconds + targetSeconds < targetProfilesTime)
                            { // ok, this is the next best.
                                targetProfilesTime = arrivalProfiles.Seconds + targetSeconds;
                            }
                        }
                    }
                }
            } while (enumerator.MoveNext());
        }

        /// <summary>
        /// Gets the arrival profiles.
        /// </summary>
        public StopProfileCollection ArrivalProfiles
        {
            get
            {
                return _targetProfiles;
            }
        }

        /// <summary>
        /// Gets the arrival stops.
        /// </summary>
        public List<uint> ArrivalStops
        {
            get
            {
                return _targetStops;
            }
        }

        /// <summary>
        /// Gets the calculated arrival time for the given transfer count.
        /// </summary>
        /// <returns></returns>
        public DateTime ArrivalTime(int transfers)
        {
            this.CheckHasRunAndHasSucceeded();

            return _departureTime.Date.AddSeconds(_targetProfiles[transfers].Seconds);
        }

        /// <summary>
        /// Gets the duration is seconds.
        /// </summary>
        /// <returns></returns>
        public long Duration(int transfers)
        {
            this.CheckHasRunAndHasSucceeded();

            return _targetProfiles[transfers].Seconds - (int)(_departureTime - _departureTime.Date).TotalSeconds;
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
        /// Gets the trip status for the given trip.
        /// </summary>
        public TripStatus GetTripStatus(uint tripId)
        {
            this.CheckHasRunAndHasSucceeded();

            TripStatus tripStatus;
            if (!_tripStatuses.TryGetValue(tripId, out tripStatus))
            { // status not found.
                throw new Exception(string.Format("Trip with id {0} not found.", tripId));
            }
            return tripStatus;
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
                return this.UpdateStatus(transfers, profile, null);
            }

            /// <summary>
            /// Updates a profile for the given number of transfers.
            /// </summary>
            public bool UpdateStatus(int transfers, StopProfile profile, Action<int> remove)
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
                        if ((this[i].Seconds >= profile.Seconds) ||
                             this[i].IsEmpty)
                        {
                            if (i == this.Count - 1)
                            { // remove last if it would be set to empty.
                                this.RemoveAt(i);
                            }
                            else
                            { // ... or empty out if not the last entry.
                                this[i] = StopProfile.Empty;
                            }
                            if (remove != null)
                            {
                                remove(i);
                            }
                        }
                    }

                    if (this[transfers].Seconds > profile.Seconds)
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
    /// Keep a dummy-status when trip is impossible.
    /// </summary>
    public struct TripStatus
    {
        /// <summary>
        /// Gets or sets the stop id.
        /// </summary>
        public uint StopId { get; set; }

        /// <summary>
        /// Gets or sets the departure time.
        /// </summary>
        public uint DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the number of transfers.
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
        private int _previousId;

        /// <summary>
        /// Gets or sets the stop id.
        /// </summary>
        public uint PreviousStopId
        { 
            get
            {
                if (_previousId >= 0)
                {
                    return Constants.NoStopId;
                }
                return ((uint)-_previousId) - 1;
            }
            set
            {
                _previousId = (int)-(value + 1);
            }
        }

        /// <summary>
        /// Gets or sets the connection id.
        /// </summary>
        public uint PreviousConnectionId
        {
            get
            {
                if (_previousId <= 0)
                {
                    return Constants.NoConnectionId;
                }
                return ((uint)_previousId) - 1;
            }
            set
            {
                _previousId = (int)(value + 1);
            }
        }

        /// <summary>
        /// Returns true if this profile is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _previousId == 0 &&
                    this.Seconds == Constants.NoSeconds;
            }
        }

        /// <summary>
        /// Returns true if this profile has a previous connection.
        /// </summary>
        public bool IsConnection
        {
            get
            {
                return _previousId > 0;
            }
        }

        /// <summary>
        /// Returns true if this profile has a previous stop.
        /// </summary>
        public bool IsTransfer
        {
            get
            {
                return _previousId < 0;
            }
        }

        /// <summary>
        /// Returns true if this profile has no previous stop or connection.
        /// </summary>
        public bool IsFirst
        {
            get
            {
                return _previousId == 0 &&
                    this.Seconds != Constants.NoSeconds;
            }
        }

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
            if (this.IsConnection)
            {
                return string.Format("C{0}@{1}s", this.PreviousConnectionId, this.Seconds);
            }
            else if (this.IsTransfer)
            {
                return string.Format("S{0}@{1}s", this.PreviousStopId, this.Seconds);
            }
            else if(this.Seconds != Constants.NoSeconds)
            {
                return string.Format("{0}s", this.Seconds);
            }
            return "empty";
        }

        /// <summary>
        /// Gets the default empty profile.
        /// </summary>
        public static StopProfile Empty = new StopProfile()
        {
            _previousId = 0,
            Seconds = Constants.NoSeconds
        };
    }
}