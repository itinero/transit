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

using Itinero.Exceptions;
using Itinero.Profiles;
using Itinero.Transit.Algorithms.OneToOne;
using Itinero.Transit.Data;
using Itinero.Transit.Algorithms.Search;
using System;
using System.Collections.Generic;
using Itinero.LocalGeo;

namespace Itinero.Transit
{
    /// <summary>
    /// A transit router.
    /// </summary>
    public class TransitRouter : ITransitRouter
    {
        private readonly TransitDb _transitDb;
        private readonly Profile _transferProfile;

        /// <summary>
        /// Creates a new transit router.
        /// </summary>
        public TransitRouter(TransitDb transitDb, Profile transferProfile)
        {
            _transitDb = transitDb;
            _transferProfile = transferProfile;
        }

        /// <summary>
        /// Tries to calculate an earliest arrival route from stop1 to stop2.
        /// </summary>
        public Result<Route> TryEarliestArrival(DateTime departureTime, uint stop1, uint stop2, Func<uint, bool> useAgency)
        {
            var tripEnumerator = _transitDb.GetTripsEnumerator();
            var transfersDb =  _transitDb.GetTransfersDb(_transferProfile);
            var profileSearch = new ProfileSearch(_transitDb, departureTime, transfersDb, (t, day) =>
            {
                if (tripEnumerator.MoveTo(t))
                {
                    if(useAgency(tripEnumerator.AgencyId))
                    {
                        return true;
                    }
                }
                return false;
            });
            profileSearch.SetSourceStop(stop1, (uint)(departureTime - departureTime.Date).TotalSeconds);
            profileSearch.SetTargetStop(stop2, 0);
            profileSearch.Run();
            if (!profileSearch.HasSucceeded)
            {
                return new Result<Route>(profileSearch.ErrorMessage, (message) =>
                {
                    return new RouteNotFoundException(message);
                });
            }

            // generate route.
            var routeBuilder = new ProfileSearchRouteBuilder(profileSearch);
            routeBuilder.Run();
            if (!routeBuilder.HasSucceeded)
            {
                return new Result<Route>(routeBuilder.ErrorMessage, (message) =>
                {
                    return new RouteBuildFailedException(message);
                });
            }
            return new Result<Route>(routeBuilder.Route);
        }

        /// <summary>
        /// Tries to search for stops with the given name.
        /// </summary>
        public Result<HashSet<uint>> TrySearchStop(string name)
        {
            var stops = new HashSet<uint>();
            name = name.ToLowerInvariant();

            var stopsEnumerator = _transitDb.GetStopsEnumerator();
            while(stopsEnumerator.MoveNext())
            {
                var stopTags = _transitDb.StopAttributes.Get(stopsEnumerator.MetaId);
                var stopName = string.Empty;
                if(stopTags != null && stopTags.TryGetValue("name", out stopName))
                {
                    if(stopName.ToLowerInvariant().Contains(name))
                    {
                        stops.Add(stopsEnumerator.Id);
                    }
                }
            }
            return new Result<HashSet<uint>>(stops);
        }

        /// <summary>
        /// Tries to search for the closest stop.
        /// </summary>
        public Result<uint> TrySearchClosestStop(float latitude, float longitude)
        {            
            // calculate moffset in degrees.
            var offsettedLocation = (new Coordinate(latitude, longitude)).OffsetWithDistances(
                Constants.SearchOffsetInMeter);
            var maxOffset = (float)System.Math.Max(
                System.Math.Abs(latitude - offsettedLocation.Latitude),
                System.Math.Abs(longitude - offsettedLocation.Longitude));

            var stopsDbEnumerator = _transitDb.GetStopsEnumerator();
            var stop = stopsDbEnumerator.SearchClosest(latitude, longitude, maxOffset);
            if(stop == Constants.NoStopId)
            {
                return new Result<uint>("No stop found close enough to the given location.");
            }
            return new Result<uint>(stop);
        }
    }
}