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

        /// <summary>
        /// Creates a new profile search route builder.
        /// </summary>
        /// <param name="search">The search algorithm.</param>
        public ProfileSearchRouteBuilder(ProfileSearch search)
        {
            _search = search;
        }

        private Route _route;

        /// <summary>
        /// Executes the route build step.
        /// </summary>
        /// <returns></returns>
        protected override void DoRun()
        {
            if (!_search.HasRun) { throw new InvalidOperationException("Cannot build a route before the search was executed."); }
            if (!_search.HasSucceeded) { throw new InvalidOperationException("Cannot build a route when the search did not succeed."); }

            var stops = new List<Tuple<uint, StopProfile>>();
            var connections = new List<uint?>();

            var connectionEnumerator = _search.Db.GetConnectionsEnumerator(Data.DefaultSorting.DepartureTime);
            var stopEnumerator = _search.Db.GetStopsEnumerator();
            var tripEnumerator = _search.Db.GetTripsEnumerator();

            var profiles = _search.GetStopProfiles(_search.TargetStop);
            var profileIdx = profiles.GetLeastTransfers();
            stops.Add(new Tuple<uint, StopProfile>(_search.TargetStop, profiles[profileIdx]));
            while (!profiles[profileIdx].IsFirst)
            {
                var previousStopId = uint.MaxValue;
                if (profiles[profileIdx].IsConnection)
                { // this profile represent an arrival via a connection, add connection.
                    connections.Add(profiles[profileIdx].PreviousConnectionId);
                    connectionEnumerator.MoveTo(profiles[profileIdx].PreviousConnectionId);
                    previousStopId = connectionEnumerator.DepartureStop;
                    var previousTripId = connectionEnumerator.TripId;
                    var previousDepartureTime = connectionEnumerator.DepartureTime;

                    // get next profiles.
                    profiles = _search.GetStopProfiles(previousStopId);
                    profileIdx = profiles.GetLeastTransfers();

                    // when the next also has a connection, check for a transfer.
                    // if there is a transfer insert it.
                    if(profiles[profileIdx].IsConnection)
                    { // move to connection and check it out.
                        connectionEnumerator.MoveTo(profiles[profileIdx].PreviousConnectionId);
                        if (connectionEnumerator.TripId != previousTripId)
                        { // there is a transfer.
                            stops.Add(new Tuple<uint, StopProfile>(previousStopId, new StopProfile()
                            {
                                PreviousStopId = previousStopId,
                                Seconds = previousDepartureTime
                            }));
                            connections.Add(null);
                        }
                    }

                    // check for a difference in departure time and arrival time of the profile.
                    // if there is a difference insert a waiting period.
                    if (!profiles[profileIdx].IsConnection &&
                         profiles[profileIdx].Seconds != connectionEnumerator.DepartureTime)
                    { // this is a waiting period.
                        stops.Add(new Tuple<uint,StopProfile>(previousStopId, new StopProfile()
                            {
                                PreviousStopId = previousStopId,
                                Seconds = connectionEnumerator.DepartureTime
                            }));
                        connections.Add(null);
                    }

                    // add stop.
                    stops.Add(new Tuple<uint, StopProfile>(previousStopId, profiles[profileIdx]));
                }
                else if (profiles[profileIdx].IsTransfer)
                { // this profile respresent an arrival via a transfers from a given stop.
                    connections.Add(null);
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
            connections.Reverse();

            // convert the stop and connection sequences into an actual route.
            _route = new Route();
            _route.Segments = new List<RouteSegment>();

            // get the first stop.
            if(!stopEnumerator.MoveTo(stops[0].Item1))
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
                // get the next ...->connection->stop->... pair.
                var connection = connections[idx - 1];
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
                if (connection == null)
                {
                    if (idx == 1)
                    { // first connection null is waiting period.
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
                    if (!connectionEnumerator.MoveTo(connection.Value))
                    {
                        throw new Exception(string.Format("Connection {0} not found.", connection.Value));
                    }
                    if (!tripEnumerator.MoveTo(connectionEnumerator.TripId))
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

            if(_route.Segments.Count > 0)
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
        /// Converts a tags collection to route tags.
        /// </summary>
        private List<RouteTags> AddToRouteTags(List<RouteTags> routeTags, TagsCollectionBase tags, string prefix)
        {
            if(tags == null)
            {
                return routeTags;
            }
            foreach(var tag in tags)
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