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

using Itinero.Profiles;
using Itinero.Transit.Data;
using System;

namespace Itinero.Transit
{
    /// <summary>
    /// Abstract representation of a multimodal router.
    /// </summary>
    public abstract class MultimodalRouterBase
    {
        /// <summary>
        /// Gets the core router.
        /// </summary>
        public abstract Router Router
        {
            get;
        }

        /// <summary>
        /// Gets the multimodal db.
        /// </summary>
        public abstract MultimodalDb Db
        {
            get;
        }

        /// <summary>
        /// Tries to calculate an earliest arrival route from stop1 to stop2.
        /// </summary>
        public abstract Result<Route> TryEarliestArrival(DateTime departureTime,
            RouterPoint sourcePoint, Profile sourceProfile, RouterPoint targetPoint, Profile targetProfile, 
                EarliestArrivalSettings settings);
    }

    /// <summary>
    /// Contains earliest arrival settings.
    /// </summary>
    public class EarliestArrivalSettings
    {
        /// <summary>
        /// Gets or sets the maximum seconds for the source.
        /// </summary>
        public int MaxSecondsSource { get; set; }

        /// <summary>
        /// Gets or sets the maximum seconds for the target.
        /// </summary>
        public int MaxSecondsTarget { get; set; }

        /// <summary>
        /// Gets or sets the use agency function.
        /// </summary>
        public Func<uint, bool> UseAgency { get; set; }

        /// <summary>
        /// Gets the default earliest arrival settings.
        /// </summary>
        public static EarliestArrivalSettings Default
        {
            get
            {
                return new EarliestArrivalSettings()
                {
                    MaxSecondsSource = 1800,
                    MaxSecondsTarget = 1800,
                    UseAgency = null
                };
            }
        }
    }
}