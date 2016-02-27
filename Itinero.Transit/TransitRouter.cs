﻿// OsmSharp - OpenStreetMap (OSM) SDK
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