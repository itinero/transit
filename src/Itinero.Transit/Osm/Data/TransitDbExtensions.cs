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

using Itinero.Attributes;
using Itinero.Transit.Data;

namespace Itinero.Transit.Osm.Data
{
    /// <summary>
    /// Contains extension methods for the transit db.
    /// </summary>
    public static class TransitDbExtensions
    {
        /// <summary>
        /// Holds the default edge profile for transfers.
        /// </summary>
        public static IAttributeCollection DefaultEdgeProfile =
            new AttributeCollection(new Attribute("highway", "residential"));

        /// <summary>
        /// Adds a transfers db for pedestrians.
        /// </summary>
        public static void AddTransfersDbForPedestrians(this TransitDb db, float maxTimeInSeconds)
        {
            db.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), maxTimeInSeconds);
        }

        /// <summary>
        /// Adds a transfers db.
        /// </summary>
        public static void AddTransfersDb(this TransitDb db, Profiles.Profile profile,
            float maxTimeInSeconds)
        {
            db.AddTransfersDb(profile, TransitDbExtensions.DefaultEdgeProfile, maxTimeInSeconds);
        }
    }
}