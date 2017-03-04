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

using Itinero.Algorithms.Routes;
using Itinero.Profiles;
using Itinero.Transit.Algorithms;
using Itinero.Transit.Algorithms.OneToOne;
using Itinero.Transit.Data;
using System;

namespace Itinero.Transit
{
    /// <summary>
    /// An implementation of a multi modal router.
    /// </summary>
    public sealed class MultimodalRouter : MultimodalRouterBase
    {
        private readonly MultimodalDb _db;
        private readonly Profile _transferProfile;
        private readonly Router _router;

        /// <summary>
        /// Creates a multimodal router.
        /// </summary>
        public MultimodalRouter(MultimodalDb db, Profile transferProfile)
        {
            _db = db;
            _transferProfile = transferProfile;
            _router = new Router(db.RouterDb);

            this.PreferTransit = true;
            this.PreferTransitThreshold = 60 * 5;
        }

        /// <summary>
        /// Get or sets the prefer transit flag.
        /// </summary>
        public bool PreferTransit { get; set; }

        /// <summary>
        /// Gets router.
        /// </summary>
        public override Router Router
        {
            get
            {
                return _router;
            }
        }

        /// <summary>
        /// Gets the multimodal db.
        /// </summary>
        public override MultimodalDb Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Gets or sets the prefer transit threshold in seconds.
        /// </summary>
        /// <remarks>Only for routes with duration > threshold, use prefer transit flag.</remarks>
        public int PreferTransitThreshold { get; set; }

        /// <summary>
        /// Tries to calculate an earliest arrival route from stop1 to stop2.
        /// </summary>
        public override Result<Route> TryEarliestArrival(DateTime departureTime,
            RouterPoint sourcePoint, Profile sourceProfile, RouterPoint targetPoint, Profile targetProfile, 
                EarliestArrivalSettings settings)
        {
            // get the get factor function.
            var sourceGetFactor = _router.GetDefaultGetFactor(sourceProfile);
            var targetGetFactor = _router.GetDefaultGetFactor(targetProfile);

            // create the profile search.
            var tripEnumerator = _db.TransitDb.GetTripsEnumerator();
            var transfersDb =  _db.TransitDb.GetTransfersDb(_transferProfile);
            var profileSearch = new ProfileSearch(_db.TransitDb, departureTime, transfersDb, _db.TransitDb.GetIsTripPossibleFunc());

            // search for sources.
            var departureTimeSeconds = (uint)(departureTime - departureTime.Date).TotalSeconds;
            var sourceSearch = new ClosestStopsSearch(_db, sourceProfile, sourceGetFactor, sourcePoint,
                settings.MaxSecondsSource, false);
            sourceSearch.StopFound = (s, t) =>
                {
                    profileSearch.SetSourceStop(s, departureTimeSeconds + (uint)t);
                    return false;
                };

            // search for targets.
            var targetSearch = new ClosestStopsSearch(_db, targetProfile, targetGetFactor, targetPoint,
                settings.MaxSecondsTarget, true);
            targetSearch.StopFound = (s, t) =>
                {
                    profileSearch.SetTargetStop(s, (uint)t);
                    return false;
                };

            // create bidirectional helper if possible.
            SearchHelper helper = null;
            BidirectionalSearchHelper bidirectionalHelper = null;
            if (sourceProfile.Name == targetProfile.Name)
            { // profiles are the same.
                bidirectionalHelper = new BidirectionalSearchHelper(
                    sourceSearch.Search, targetSearch.Search);
                targetSearch.WasEdgeFound = bidirectionalHelper.TargetWasFound;
            }
            else
            { // profiles are different but the source can still reach the destination.
                helper = new SearchHelper(_router.Db, sourceSearch.Search, sourceProfile, targetPoint);
                sourceSearch.WasEdgeFound = helper.SourceWasFound;
            }

            // run source search.
            sourceSearch.Run();
            if (!sourceSearch.HasRun ||
                !sourceSearch.HasSucceeded)
            {
                return new Result<Route>("Searching for source stops failed.");
            }

            // run target search.
            targetSearch.Run();
            if (!targetSearch.HasRun ||
                !targetSearch.HasSucceeded)
            {
                return new Result<Route>("Searching for target stops failed.");
            }

            // run actual profile search.
            profileSearch.Run();
            if (!profileSearch.HasRun ||
                !profileSearch.HasSucceeded)
            {
                return new Result<Route>("No route found.");
            }

            // build routes.
            var profileSearchRouteBuilder = new ProfileSearchRouteBuilder(profileSearch);
            profileSearchRouteBuilder.Run();
            if (!profileSearchRouteBuilder.HasRun ||
                !profileSearchRouteBuilder.HasSucceeded)
            {
                return new Result<Route>(string.Format("Route could not be built: {0}.", profileSearchRouteBuilder.ErrorMessage));
            }

            var sourceWeight = sourceSearch.GetWeight(profileSearchRouteBuilder.Stops[0]);
            var targetWeight = targetSearch.GetWeight(profileSearchRouteBuilder.Stops[profileSearchRouteBuilder.Stops.Count - 1]);
            var transitWeight = sourceWeight + profileSearchRouteBuilder.Duration + targetWeight;
            
            if (bidirectionalHelper != null &&
                bidirectionalHelper.HasSucceeded)
            { // there is a direct route to the target.
                if(transitWeight > bidirectionalHelper.BestWeight)
                {
                    if (!this.PreferTransit || bidirectionalHelper.BestWeight < this.PreferTransitThreshold)
                    { // transit it not preferred or belof threshold.
                        var path = bidirectionalHelper.GetPath();
                        return new Result<Route>(
                            CompleteRouteBuilder.Build(_router.Db, sourceProfile, sourcePoint, targetPoint, path));
                    }
                }
            }
            else if(helper != null &&
                helper.HasSucceeded)
            { // there is a direct route to the target.
                if (transitWeight > helper.BestWeight)
                {
                    if (!this.PreferTransit || bidirectionalHelper.BestWeight < this.PreferTransitThreshold)
                    { // transit it not preferred or belof threshold.
                        var path = helper.GetPath();
                        return new Result<Route>(
                            CompleteRouteBuilder.Build(_router.Db, sourceProfile, sourcePoint, targetPoint, path));
                    }
                }
            }           

            // build source/target routes.
            var sourceRoute = sourceSearch.GetRoute(profileSearchRouteBuilder.Stops[0]);
            var targetRoute = targetSearch.GetRoute(profileSearchRouteBuilder.Stops[profileSearchRouteBuilder.Stops.Count - 1]);

            // concatenate it all.
            var route = sourceRoute.Concatenate(profileSearchRouteBuilder.Route);
            route = route.Concatenate(targetRoute);

            return new Result<Route>(route);
        }
    }
}