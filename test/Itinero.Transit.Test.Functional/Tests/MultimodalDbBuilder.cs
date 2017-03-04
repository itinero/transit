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
using Itinero.Transit.Osm.Data;
using System;

namespace Itinero.Transit.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests to build a multimodal db.
    /// </summary>
    public static class MultimodalDbBuilder
    {
        /// <summary>
        /// Tests building a multimodal db.
        /// </summary>
        public static MultimodalDb Run(RouterDb routerDb, TransitDb transitDb)
        {
            return MultimodalDbBuilder.GetTestBuildMultimopdalDb(routerDb, transitDb).TestPerf(
                "Testing build a multimodal db.");
        }

        /// <summary>
        /// Tests building a multimodaldb.
        /// </summary>
        public static Func<MultimodalDb> GetTestBuildMultimopdalDb(RouterDb routerDb, TransitDb transitDb)
        {
            return () =>
            {
                var multimodalDb = new MultimodalDb(routerDb, transitDb);
                multimodalDb.TransitDb.AddTransfersDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 100);
                multimodalDb.AddStopLinksDb(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), maxDistance: 100);
                return multimodalDb;
            };
        }
    }
}
