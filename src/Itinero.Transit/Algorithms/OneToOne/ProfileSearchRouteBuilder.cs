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

using Itinero.Transit.Data;
using Itinero.Algorithms;
using System;
using System.Collections.Generic;
using System.Threading;
using Itinero.LocalGeo;
using Itinero.Attributes;

namespace Itinero.Transit.Algorithms.OneToOne
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
        protected override void DoRun(CancellationToken cancellationToken)
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
                for (var i = 0; i < trips.Count; i++)
                {
                    if (trips[i].HasValue)
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
            // _route = new Route();
            var routeShape = new List<Coordinate>();
            var routeShapeMetas = new List<Route.Meta>();
            var routeStops = new List<Route.Stop>();

            // get the first stop.
            if (!stopEnumerator.MoveTo(stops[0].Item1))
            {
                throw new Exception(string.Format("Stop {0} not found.", stops[0].Item1));
            }
            routeShape.Add(new Coordinate(stopEnumerator.Latitude, stopEnumerator.Longitude));
            var attributes = new AttributeCollection();
            attributes.AddOrReplace(Constants.TimeOfDayKey, stops[0].Item2.Seconds.ToInvariantString());
            routeShapeMetas.Add(new Route.Meta()
            {
                Attributes = attributes,
                Shape = 0
            });
            var stopAttributes = _search.Db.StopAttributes.Get(stopEnumerator.MetaId);
            routeStops.Add(new Route.Stop()
            {
                Shape = 0,
                Attributes = stopAttributes,
                Coordinate = new Coordinate(stopEnumerator.Latitude, stopEnumerator.Longitude)
            });

            var departureTime = stops[0].Item2.Seconds;
            for (int idx = 1; idx < stops.Count; idx++)
            {
                // get the next ...->trip->stop->... pair.
                var trip = trips[idx - 1];
                if (!stopEnumerator.MoveTo(stops[idx].Item1))
                {
                    throw new Exception(string.Format("Stop {0} not found.", stops[0].Item1));
                }

                // add shapepoints between stops if present.
                var shapePoints = _search.Db.ShapesDb.Get(stops[idx - 1].Item1, stops[idx].Item1);
                if (shapePoints != null)
                {
                    foreach(var shapePoint in shapePoints)
                    {
                        routeShape.Add(new Coordinate(shapePoint.Latitude, shapePoint.Longitude));
                    }
                }

                // add stop shapepoint.
                routeShape.Add(new Coordinate(stopEnumerator.Latitude, stopEnumerator.Longitude));

                // add stop.
                stopAttributes = _search.Db.StopAttributes.Get(stopEnumerator.MetaId);
                routeStops.Add(new Route.Stop()
                {
                    Shape = routeShape.Count - 1,
                    Attributes = stopAttributes,
                    Coordinate = new Coordinate(stopEnumerator.Latitude, stopEnumerator.Longitude)
                });

                // add timing info.
                attributes = new AttributeCollection();
                attributes.AddOrReplace(Constants.TimeOfDayKey, stops[idx].Item2.Seconds.ToInvariantString());

                // get route information.
                if (trip == null)
                {
                    if (idx == 1)
                    { // first trip null is waiting period.
                        var meta = new Route.Meta()
                        {
                            Shape = routeShape.Count - 1,
                            Attributes = attributes
                        };
                        meta.Time = stops[idx].Item2.Seconds - departureTime;
                        meta.Profile = Constants.WaitProfile;
                        routeShapeMetas.Add(meta);
                    }
                    else if (routeShapeMetas[routeShapeMetas.Count - 1].Profile == Constants.TransferProfile)
                    { // a waiting period.
                        var meta = new Route.Meta()
                        {
                            Shape = routeShape.Count - 1,
                            Attributes = attributes
                        };
                        meta.Time = stops[idx].Item2.Seconds - departureTime;
                        meta.Profile = Constants.WaitProfile;
                        routeShapeMetas.Add(meta);
                    }
                    else
                    { // a regular transfer.
                        var meta = new Route.Meta()
                        {
                            Shape = routeShape.Count - 1,
                            Attributes = attributes
                        };
                        meta.Time = stops[idx].Item2.Seconds - departureTime;
                        meta.Profile = Constants.TransferProfile;
                        routeShapeMetas.Add(meta);
                    }
                }
                else
                {
                    if (!tripEnumerator.MoveTo(trip.Value))
                    {
                        throw new Exception(string.Format("Trip {0} not found.", connectionEnumerator.TripId));
                    }

                    attributes.AddOrReplaceWithPrefix("trip_", _search.Db.TripAttributes.Get(tripEnumerator.MetaId));
                    attributes.AddOrReplaceWithPrefix("agency_", _search.Db.AgencyAttributes.Get(tripEnumerator.AgencyId));

                    var meta = new Route.Meta()
                    {
                        Shape = routeShape.Count - 1,
                        Attributes = attributes
                    };
                    meta.Time = stops[idx].Item2.Seconds - departureTime;
                    meta.Profile = Constants.VehicleProfile;
                    routeShapeMetas.Add(meta);
                }
            }

            // build actual route.
            _route = new Route();
            _route.Shape = routeShape.ToArray();
            _route.ShapeMeta = routeShapeMetas.ToArray();
            _route.Stops = routeStops.ToArray();
            if (_route.ShapeMeta.Length > 0)
            {
                _route.TotalDistance = _route.ShapeMeta[_route.ShapeMeta.Length - 1].Distance;
                _route.TotalTime = _route.ShapeMeta[_route.ShapeMeta.Length - 1].Time;
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
    }
}