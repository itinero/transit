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

using NUnit.Framework;
using Itinero.Transit.Algorithms.OneToOne;
using Itinero.Transit.Data;
using Itinero.Attributes;
using Itinero;

namespace OsmSharp.Transit.Test.Algorithms.OneToOne
{
    /// <summary>
    /// Contains for the profile search route builder.
    /// </summary>
    [TestFixture]
    public class ProfileSearchRouteBuilderTests
    {
        /// <summary>
        /// Tests a successful one-hop with a one-connection db.
        /// 
        /// Departure (0)@00:50:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @01:00          @01:40
        /// 
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 3600, 3600 + 40 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 00, 50, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 50 * 60);
            algorithm.SetTargetStop(1, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "3000"));
            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "3600"));
            Assert.IsTrue(meta.Attributes.Contains("time", "600"));
            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, "6000"));
            Assert.IsTrue(meta.Attributes.Contains("time", "3000"));

            Assert.AreEqual(3000, route.TotalTime);
        }

        /// <summary>
        /// Tests a successful two-hop with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)-->---0--->--(2)
        /// @08:00          @08:10          @08:20
        /// 
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Attribute("name", "stop3")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 11 * 60, 8 * 3600 + 20 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(2, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(2, route.Shape[2].Latitude);
            Assert.AreEqual(2, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, 
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);
            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 30 * 60);
            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey, 
                ((08 * 3600) + (20 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 50 * 60);

            // build route.
            routeBuilder = new ProfileSearchRouteBuilder(algorithm, true);
            routeBuilder.Run();
            route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(4, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);
            Assert.AreEqual(2, route.Shape[3].Latitude);
            Assert.AreEqual(2, route.Shape[3].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(4, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[3];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(3, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);

            meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(30 * 60, meta.Time);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(40 * 60, meta.Time);

            meta = route.ShapeMeta[3];
            Assert.AreEqual(3, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (20 * 60)).ToInvariantString()));
            Assert.AreEqual(50 * 60, meta.Time);
        }

        /// <summary>
        /// Tests a successful three-hop with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)-->---0--->--(2)-->---0--->--(3)
        /// @08:00          @08:10          @08:20          @08:30
        /// 
        /// </summary>
        [Test]
        public void TestThreeHopsSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Attribute("name", "stop3")));
            db.AddStop(3, 3, db.StopAttributes.Add(new Attribute("name", "stop4")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 11 * 60, 8 * 3600 + 20 * 60);
            db.AddConnection(2, 3, 0, 8 * 3600 + 21 * 60, 8 * 3600 + 30 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(3, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(3, route.Shape[2].Latitude);
            Assert.AreEqual(3, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop4"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(3, stop.Coordinate.Latitude);
            Assert.AreEqual(3, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);
            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 30 * 60);
            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 60 * 60);

            // build route.
            routeBuilder = new ProfileSearchRouteBuilder(algorithm, true);
            routeBuilder.Run();
            route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(5, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);
            Assert.AreEqual(2, route.Shape[3].Latitude);
            Assert.AreEqual(2, route.Shape[3].Longitude);
            Assert.AreEqual(3, route.Shape[4].Latitude);
            Assert.AreEqual(3, route.Shape[4].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(5, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[3];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(3, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);
            stop = route.Stops[4];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop4"));
            Assert.AreEqual(4, stop.Shape);
            Assert.AreEqual(3, stop.Coordinate.Latitude);
            Assert.AreEqual(3, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(5, route.ShapeMeta.Length);

            meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 30 * 60);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 40 * 60);

            meta = route.ShapeMeta[3];
            Assert.AreEqual(3, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (20 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 50 * 60);

            meta = route.ShapeMeta[4];
            Assert.AreEqual(4, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 60 * 60);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// <summary>
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10      
        /// 
        ///                   (1)-->---1--->--(2)
        ///                 @08:15          @08:25      
        /// </summary>
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Attribute("name", "stop3")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip2")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(2, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;
            
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(5, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);
            Assert.AreEqual(1, route.Shape[3].Latitude);
            Assert.AreEqual(1, route.Shape[3].Longitude);
            Assert.AreEqual(2, route.Shape[4].Latitude);
            Assert.AreEqual(2, route.Shape[4].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(5, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[3];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(3, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[4];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(4, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(5, route.ShapeMeta.Length);

            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 30 * 60);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 40 * 60);

            meta = route.ShapeMeta[3];
            Assert.AreEqual(3, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (15 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 45 * 60);

            meta = route.ShapeMeta[4];
            Assert.AreEqual(4, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (25 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 55 * 60);
        }

        /// <summary>
        /// Tests a successful two-hop, one transfer with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)
        /// @08:00          @08:10      
        ///                     \ (-> transfer: 100 sec)
        ///                     (2)-->---1--->--(3)
        ///                   @08:15          @08:25  
        /// </summary>
        [Test]
        public void TestTwoHopsOneTransferCloseStopsSuccessful()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Attribute("name", "stop3")));
            db.AddStop(3, 3, db.StopAttributes.Add(new Attribute("name", "stop4")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip2")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(2, 3, 1, 8 * 3600 + 15 * 60, 8 * 3600 + 25 * 60);

            db.SortConnections(DefaultSorting.DepartureTime, null);

            // build dummy transfers db.
            var transfersDb = new TransfersDb(1024);
            transfersDb.AddTransfer(1, 2, 100);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime, transfersDb,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetTargetStop(3, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(6, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);
            Assert.AreEqual(2, route.Shape[3].Latitude);
            Assert.AreEqual(2, route.Shape[3].Longitude);
            Assert.AreEqual(2, route.Shape[4].Latitude);
            Assert.AreEqual(2, route.Shape[4].Longitude);
            Assert.AreEqual(3, route.Shape[5].Latitude);
            Assert.AreEqual(3, route.Shape[5].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(6, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[3];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(3, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);
            stop = route.Stops[4];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(4, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);
            stop = route.Stops[5];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop4"));
            Assert.AreEqual(5, stop.Shape);
            Assert.AreEqual(3, stop.Coordinate.Latitude);
            Assert.AreEqual(3, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(6, route.ShapeMeta.Length);

            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 30 * 60);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 40 * 60);

            meta = route.ShapeMeta[3];
            Assert.AreEqual(3, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60) + 100).ToInvariantString()));
            Assert.AreEqual(meta.Time, 40 * 60 + 100);

            meta = route.ShapeMeta[4];
            Assert.AreEqual(4, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (15 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 45 * 60);

            meta = route.ShapeMeta[5];
            Assert.AreEqual(5, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (25 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 55 * 60);
        }

        /// <summary>
        /// Tests a successful one-hop with an intial short walk.
        /// 
        /// Departure (x)@07:30:00
        /// 
        ///   (x)-->--15min-->--(0)-->---0--->--(1)
        /// @07:30            @08:00          @08:10    
        ///       
        /// </summary>
        [Test]
        public void TestOneHopWithWalkingBefore()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 45 * 60); // a 15 min walk.
            algorithm.SetTargetStop(1, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);

            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (45 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 15 * 60);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 25 * 60);
        }

        /// <summary>
        /// Tests a successful one-hop with a short walk on arrival.
        /// 
        /// Departure (0)@07:30:00
        /// 
        ///   (0)-->---0--->--(1)-->---15mins--->---(x)
        /// @08:00          @08:10    
        ///       
        /// </summary>
        [Test]
        public void TestOneHopWithWalkingAfter()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60); // a 15 min walk.
            algorithm.SetTargetStop(1, 15 * 60);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, false);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(0, route.Shape[1].Longitude);
            Assert.AreEqual(1, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop1"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(0, stop.Coordinate.Latitude);
            Assert.AreEqual(0, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);

            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (30 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (00 * 60)).ToInvariantString()));
            Assert.AreEqual(30 * 60, meta.Time);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (10 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 40 * 60);
        }

        /// <summary>
        /// Tests a successful two-hop with a two-connection db.
        /// 
        /// Departure (0)@07:30:00
        ///           (1)@07:25:00
        /// 
        ///   (0)-->---0--->--(1)-->---0--->--(2)
        /// @08:00          @08:10          @08:20
        /// 
        /// Expected result: A route should be generate from (1) -> (2) not from (0) -> (1) -> (2). Waiting is preferred.
        /// </summary>
        [Test]
        public void TestTwoHopsSuccessfulMultipleStarts()
        {
            // build dummy db.
            var db = new TransitDb();
            db.AddStop(0, 0, db.StopAttributes.Add(new Attribute("name", "stop1")));
            db.AddStop(1, 1, db.StopAttributes.Add(new Attribute("name", "stop2")));
            db.AddStop(2, 2, db.StopAttributes.Add(new Attribute("name", "stop3")));
            db.AddTrip(0, 0, db.TripAttributes.Add(new Attribute("name", "trip1")));
            db.AddConnection(0, 1, 0, 8 * 3600, 8 * 3600 + 10 * 60);
            db.AddConnection(1, 2, 0, 8 * 3600 + 11 * 60, 8 * 3600 + 20 * 60);
            db.SortConnections(DefaultSorting.DepartureTime, null);

            // run algorithm.
            var departureTime = new System.DateTime(2017, 05, 10, 07, 30, 00);
            var algorithm = new ProfileSearch(db, departureTime,
                (profileId, day) => true);
            algorithm.SetSourceStop(0, 07 * 3600 + 30 * 60);
            algorithm.SetSourceStop(1, 07 * 3600 + 25 * 60);
            algorithm.SetTargetStop(2, 0);
            algorithm.Run();

            // build route.
            var routeBuilder = new ProfileSearchRouteBuilder(algorithm, true);
            routeBuilder.Run();
            var route = routeBuilder.Route;

            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(1, route.Shape[0].Latitude);
            Assert.AreEqual(1, route.Shape[0].Longitude);
            Assert.AreEqual(1, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);
            Assert.AreEqual(2, route.Shape[2].Latitude);
            Assert.AreEqual(2, route.Shape[2].Longitude);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(3, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(0, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop2"));
            Assert.AreEqual(1, stop.Shape);
            Assert.AreEqual(1, stop.Coordinate.Latitude);
            Assert.AreEqual(1, stop.Coordinate.Longitude);
            stop = route.Stops[2];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(1, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("name", "stop3"));
            Assert.AreEqual(2, stop.Shape);
            Assert.AreEqual(2, stop.Coordinate.Latitude);
            Assert.AreEqual(2, stop.Coordinate.Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);

            var meta = route.ShapeMeta[0];
            Assert.AreEqual(0, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((07 * 3600) + (25 * 60)).ToInvariantString()));
            Assert.AreEqual(0, meta.Time);

            meta = route.ShapeMeta[1];
            Assert.AreEqual(1, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (11 * 60)).ToInvariantString()));
            Assert.AreEqual(46 * 60, meta.Time);

            meta = route.ShapeMeta[2];
            Assert.AreEqual(2, meta.Shape);
            Assert.IsNotNull(meta.Attributes);
            Assert.IsTrue(meta.Attributes.Contains(Itinero.Transit.Constants.TimeOfDayKey,
                ((08 * 3600) + (20 * 60)).ToInvariantString()));
            Assert.AreEqual(meta.Time, 55 * 60);
        }
    }
}