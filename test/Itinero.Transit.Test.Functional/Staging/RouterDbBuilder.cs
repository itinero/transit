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

using Itinero.IO.Osm;
using System;
using System.IO;

namespace Itinero.Transit.Test.Functional.Staging
{
    /// <summary>
    /// Builds a routerdb.
    /// </summary>
    public static class RouterDbBuilder
    {
        public static string BelgiumRouterDbLocation = "belgium.routerdb";
        
        /// <summary>
        /// Builds a routerdb.
        /// </summary>
        /// <returns></returns>
        public static RouterDb BuildBelgium()
        {
            RouterDb routerDb = null;
            if (!File.Exists(Download.BelgiumLocal))
            {
                throw new Exception("No location belgium OSM file found!");
            }

            if (File.Exists(RouterDbBuilder.BelgiumRouterDbLocation))
            {
                try
                {
                    using (var stream = File.OpenRead(RouterDbBuilder.BelgiumRouterDbLocation))
                    {
                        routerDb = RouterDb.Deserialize(stream);
                    }
                }
                catch
                {
                    Itinero.Logging.Logger.Log("RouterDbBuilder", Itinero.Logging.TraceEventType.Warning, "Invalid existing RouterDb file, could not load file.");
                    routerDb = null;
                }
            }

            if (routerDb == null)
            {
                Itinero.Logging.Logger.Log("RouterDbBuilder", Itinero.Logging.TraceEventType.Information, "No existing RouterDb file found, creating now.");
                using (var stream = File.OpenRead(Download.BelgiumLocal))
                {
                    routerDb = new RouterDb();
                    routerDb.LoadOsmData(stream, Itinero.Osm.Vehicles.Vehicle.Car, 
                        Itinero.Osm.Vehicles.Vehicle.Bicycle, Itinero.Osm.Vehicles.Vehicle.Pedestrian);
                }

                using (var stream = File.Open(RouterDbBuilder.BelgiumRouterDbLocation, FileMode.Create))
                {
                    routerDb.Serialize(stream);
                }
                Itinero.Logging.Logger.Log("RouterDbBuilder", Itinero.Logging.TraceEventType.Information, "RouterDb file created.");
            }
            return routerDb;
        }
    }
}
