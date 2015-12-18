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
using OsmSharp.Routing.Transit.Data;
using OsmSharp.Routing.Algorithms;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Algorithms.OneToOne
{
    /// <summary>
    /// A class responsable for building a profile search route.
    /// </summary>
    public class ProfileSearchRouteBuilder : AlgorithmBase
    {
        private readonly ProfileSearch _search;
        private readonly bool _intermediateStops;

        /// <summary>
        /// Creates a new profile search route builder.
        /// </summary>
        /// <param name="search">The search algorithm.</param>
        /// <param name="intermediateStops">True when the intermediate stops need to be added.</param>
        public ProfileSearchRouteBuilder(ProfileSearch search, bool intermediateStops = true)
        {
            _search = search;
            _intermediateStops = intermediateStops;
        }

        private Route _route;
        private List<uint> _stops;
        private uint _duration;

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        protected override void DoRun()
        {
            if (!_search.HasRun) { throw new InvalidOperationException("Cannot build a route before the search was executed."); }
            if (!_search.HasSucceeded) { throw new InvalidOperationException("Cannot build a route when the search did not succeed."); }

            var stops = new List<Tuple<uint, StopProfile>>();
            var trips = new List<uint?>();

            var connectionEnumerator = _search.Db.GetConnectionsEnumerator(Data.DefaultSorting.DepartureTime);
            var stopEnumerator = _search.Db.GetStopsEnumerator();
            var tripEnumerator = _search.Db.GetTripsEnumerator();

            // get best target stop.
            var targetProfiles = _search.ArrivalProfiles;
            var targetProfileIdx = _search.GetBest(targetProfiles, 10 * 60);
            var targetStop = _search.ArrivalStops[targetProfileIdx];

            // build route along that target.
            var profiles = _search.GetStopProfiles(targetStop);
            var profileIdx = profiles.GetLeastTransfers();
            stops.Add(new Tuple<uint, StopProfile>(targetStop, profiles[profileIdx]));
            while (!profiles[profileIdx].IsFirst)
            {
                var previousStopId = uint.MaxValue;
                if (profiles[profileIdx].IsConnection)
                { // this profile represent an arrival via a connection, add the trip and move down to where
                    // the trip was boarded.
                    connectionEnumerator.MoveTo(profiles[profileIdx].PreviousConnectionId);
                    trips.Add(connectionEnumerator.TripId);

                    // get trip status.
                    var tripStatus = _search.GetTripStatus(connectionEnumerator.TripId);
                    previousStopId = tripStatus.StopId;

                    // get next profiles.
                    profiles = _search.GetStopProfiles(previousStopId);
                    profileIdx = profiles.GetLeastTransfers();

                    // when the next also has a connection this is a transfer.
                    if (profiles[profileIdx].IsConnection)
                    { // move to connection and check it out.
                        stops.Add(new Tuple<uint, StopProfile>(previousStopId, new StopProfile()
                        {
                            PreviousStopId = previousStopId,
                            Seconds = tripStatus.DepartureTime
                        }));
                        trips.Add(null);
                    }

                    // check for a difference in departure time and arrival time of the profile.
                    // if there is a difference insert a waiting period.
                    if (!profiles[profileIdx].IsConnection &&
                         profiles[profileIdx].Seconds != connectionEnumerator.DepartureTime)
                    { // this is a waiting period.
                        stops.Add(new Tuple<uint, StopProfile>(previousStopId, new StopProfile()
                            {
                                PreviousStopId = previousStopId,
                                Seconds = tripStatus.DepartureTime
                            }));
                        trips.Add(null);
                    }

                    // add stop.
                    stops.Add(new Tuple<uint, StopProfile>(previousStopId, profiles[profileIdx]));
                }
                else if (profiles[profileIdx].IsTransfer)
                { // this profile respresent an arrival via a transfers from a given stop.
                    trips.Add(null);
                    previousStopId = profiles[profileIdx].PreviousStopId;

                    // get next profiles.
                    profiles = _search.GetStopProfiles(previousStopId);
                    profileIdx = profiles.GetLeastTransfers();

                    // add stop.
                    stops.Add(new Tuple<uint, StopProfile>(previousStopId, profiles[profileIdx]));
                }
                else
                { // no previous connection or stop or first, what is this?
                    throw new Exception("A profile was found as part of the path that is not a transfer, connection or first.");
                }
            }

            // reverse stops/connections.
            stops.Reverse();
            trips.Reverse();

            if (_intermediateStops)
            { // expand trips.
                for (var i = 0; i< trips.Count;i++)
                {
                    if(trips[i].HasValue)
                    { // there is a trip, expand it.
                        var lastStop = stops[i + 1].Item1;
                        var firstStop = stops[i].Item1;
                        if (!stops[i + 1].Item2.IsConnection)
                        {
                            throw new Exception("Last stop of a trip is not a connection, it should be.");
                        }
                        var connection = stops[i + 1].Item2.PreviousConnectionId;
                        connectionEnumerator.MoveTo(connection);
                        var firstI = i;
                        while (true)
                        { // add departure stop of connection if it doesn't equal the first stop.
                            if (firstStop == connectionEnumerator.DepartureStop)
                            {
                                break;
                            }
                            if (!connectionEnumerator.MoveToPreviousConnection())
                            {
                                throw new Exception("There has to be a previous stop, have not reached the first stop for this trip yet.");
                            }
                            stops.Insert(firstI + 1, new Tuple<uint, StopProfile>(connectionEnumerator.ArrivalStop, new StopProfile()
                                {
                                    PreviousConnectionId = connectionEnumerator.Id,
                                    Seconds = connectionEnumerator.ArrivalTime
                                }));
                            trips.Insert(i, trips[i].Value);
                            i++;
                        }
                    }
                }
            }

            // set the duration.
            _duration = stops[stops.Count - 1].Item2.Seconds -
                stops[0].Item2.Seconds;

            // keep stops.
            _stops = new List<uint>();
            for (var i = 0; i < stops.Count; i++)
            {
                _stops.Add(stops[i].Item1);
            }

            // convert the stop and connection sequences into an actual route.
            _route = new Route();
            _route.Segments = new List<RouteSegment>();

            // get the first stop.
            if (!stopEnumerator.MoveTo(stops[0].Item1))
            {
                throw new Exception(string.Format("Stop {0} not found.", stops[0].Item1));
            }
            var routeTags = new List<RouteTags>();
            this.AddToRouteTags(routeTags, _search.Db.StopAttributes.Get(stopEnumerator.MetaId), "stop_");
            routeTags.Add(new RouteTags()
            {
                Key = Constants.TimeOfDayKey,
                Value = stops[0].Item2.Seconds.ToInvariantString()
            });
            var departureTime = stops[0].Item2.Seconds;
            _route.Segments.Add(new RouteSegment()
            {
                Distance = -1, // distance is not important in transit routing.
                Latitude = (float)stopEnumerator.Latitude,
                Longitude = (float)stopEnumerator.Longitude,
                Time = 0,
                Tags = routeTags.ToArray()
            });
            for (int idx = 1; idx < stops.Count; idx++)
            {
                // get the next ...->trip->stop->... pair.
                var trip = trips[idx - 1];
                if (!stopEnumerator.MoveTo(stops[idx].Item1))
                {
                    throw new Exception(string.Format("Stop {0} not found.", stops[0].Item1));
                }

                // build tags.
                routeTags.Clear();
                this.AddToRouteTags(routeTags, _search.Db.StopAttributes.Get(stopEnumerator.MetaId), "stop_");
                routeTags.Add(new RouteTags()
                {
                    Key = Constants.TimeOfDayKey,
                    Value = stops[idx].Item2.Seconds.ToInvariantString()
                });

                // get route information.
                if (trip == null)
                {
                    if (idx == 1)
                    { // first trip null is waiting period.
                        _route.Segments.Add(new RouteSegment()
                        {
                            Distance = -1,
                            Latitude = (float)stopEnumerator.Latitude,
                            Longitude = (float)stopEnumerator.Longitude,
                            Time = stops[idx].Item2.Seconds - departureTime,
                            Tags = routeTags.ToArray(),
                            Profile = Constants.WaitProfile
                        });
                    }
                    else if (_route.Segments[_route.Segments.Count - 1].Profile == Constants.TransferProfile)
                    { // a waiting period.
                        _route.Segments.Add(new RouteSegment()
                        {
                            Distance = -1,
                            Latitude = (float)stopEnumerator.Latitude,
                            Longitude = (float)stopEnumerator.Longitude,
                            Time = stops[idx].Item2.Seconds - departureTime,
                            Tags = routeTags.ToArray(),
                            Profile = Constants.WaitProfile
                        });
                    }
                    else
                    { // a regular transfer.
                        _route.Segments.Add(new RouteSegment()
                        {
                            Distance = -1,
                            Latitude = (float)stopEnumerator.Latitude,
                            Longitude = (float)stopEnumerator.Longitude,
                            Time = stops[idx].Item2.Seconds - departureTime,
                            Tags = routeTags.ToArray(),
                            Profile = Constants.TransferProfile
                        });
                    }
                }
                else
                {
                    if (!tripEnumerator.MoveTo(trip.Value))
                    {
                        throw new Exception(string.Format("Trip {0} not found.", connectionEnumerator.TripId));
                    }

                    this.AddToRouteTags(routeTags, _search.Db.TripAttributes.Get(tripEnumerator.MetaId), "trip_");
                    this.AddToRouteTags(routeTags, _search.Db.AgencyAttributes.Get(tripEnumerator.AgencyId), "agency_");

                    _route.Segments.Add(new RouteSegment()
                    {
                        Distance = -1,
                        Latitude = (float)stopEnumerator.Latitude,
                        Longitude = (float)stopEnumerator.Longitude,
                        Time = stops[idx].Item2.Seconds - departureTime,
                        Tags = routeTags.ToArray(),
                        Profile = Constants.VehicleProfile
                    });
                }
            }

            if (_route.Segments.Count > 0)
            {
                _route.TotalDistance = _route.Segments[_route.Segments.Count - 1].Distance;
                _route.TotalTime = _route.Segments[_route.Segments.Count - 1].Time;
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Returns the route.
        /// </summary>
        public Route Route
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _route;
            }
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public uint Duration
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _duration;
            }
        }

        /// <summary>
        /// Returns the stops.
        /// </summary>
        public List<uint> Stops
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _stops;
            }
        }

        /// <summary>
        /// Converts a tags collection to route tags.
        /// </summary>
        private List<RouteTags> AddToRouteTags(List<RouteTags> routeTags, TagsCollectionBase tags, string prefix)
        {
            if (tags == null)
            {
                return routeTags;
            }
            foreach (var tag in tags)
            {
                routeTags.Add(new RouteTags()
                    {
                        Key = prefix + tag.Key,
                        Value = tag.Value
                    });
            }
            return routeTags;
        }
    }
}