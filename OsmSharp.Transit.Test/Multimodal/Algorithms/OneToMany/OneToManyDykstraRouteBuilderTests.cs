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

using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Transit.Multimodal.Algorithms.OneToMany;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Transit.Test.Multimodal.Algorithms.OneToMany
{
    /// <summary>
    /// Contains tests for the dykstra route builder.
    /// </summary>
    [TestFixture]
    public class OneToManyDykstraRouteBuilderTests
    {
        /// <summary>
        /// Tests a one hop route.
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(),
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(vertex1), 1000, false);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(1, path.From.VertexId);
            Assert.IsNull(path.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var route = routebuilder.Build();
            Assert.IsNotNull(route);
            Assert.AreEqual(2, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(500, route.Segments[1].Distance);
            Assert.AreEqual(500 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
        }

        /// <summary>
        /// Tests a two hop route.
        /// </summary>
        [Test]
        public void TestTwoHops()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            var vertex3 = graph.AddVertex(0.010514f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(),
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(vertex1), 1000, false);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex3);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(vertex3, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(vertex2, path.From.VertexId);
            Assert.IsNotNull(path.From.From);
            Assert.AreEqual(vertex1, path.From.From.VertexId);
            Assert.IsNull(path.From.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex3);
            var route = routebuilder.Build();
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(500, route.Segments[1].Distance);
            Assert.AreEqual(500 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
            Assert.AreEqual(1000, route.Segments[2].Distance);
            Assert.AreEqual(1000 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[2].Time);
            Assert.IsNotNull(route.Segments[2].Tags);
            Assert.AreEqual(1, route.Segments[2].Tags.Length);
            Assert.AreEqual("highway", route.Segments[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[2].Tags[0].Value);
        }

        /// <summary>
        /// Tests a two hop route.
        /// </summary>
        [Test]
        public void TestTwoHopsBackward()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            var vertex3 = graph.AddVertex(0.010514f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(),
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(vertex1), 1000, true);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex3);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(vertex1, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(vertex2, path.From.VertexId);
            Assert.IsNotNull(path.From.From);
            Assert.AreEqual(vertex3, path.From.From.VertexId);
            Assert.IsNull(path.From.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex3);
            var route = routebuilder.Build();
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(500, route.Segments[1].Distance);
            Assert.AreEqual(500 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
            Assert.AreEqual(1000, route.Segments[2].Distance);
            Assert.AreEqual(1000 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[2].Time);
            Assert.IsNotNull(route.Segments[2].Tags);
            Assert.AreEqual(1, route.Segments[2].Tags.Length);
            Assert.AreEqual("highway", route.Segments[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[2].Tags[0].Value);
        }

        /// <summary>
        /// Tests a one hop with shape route.
        /// </summary>
        [Test]
        public void TestOneHopWithShape()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            }, new Collections.Coordinates.Collections.CoordinateArrayCollection<GeoCoordinate>(
                new GeoCoordinate[] { new GeoCoordinate(0.002242f, 0f) }));

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(),
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(vertex1), 1000, false);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(1, path.From.VertexId);
            Assert.IsNull(path.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var route = routebuilder.Build();
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(250, route.Segments[1].Distance, 1);
            Assert.AreEqual(250 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time, 1);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
            Assert.AreEqual(500, route.Segments[2].Distance);
            Assert.AreEqual(500 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[2].Time);
            Assert.IsNotNull(route.Segments[2].Tags);
            Assert.AreEqual(1, route.Segments[2].Tags.Length);
            Assert.AreEqual("highway", route.Segments[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[2].Tags[0].Value);
        }

        /// <summary>
        /// Tests a one hop with shape route.
        /// </summary>
        [Test]
        public void TestOneHopWithShapeBackward()
        {
            // build a tiny test graph.
            var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
            var tags = new TagsCollection(new Tag() { Key = "highway", Value = "residential" });
            var tagsId = graph.TagsIndex.Add(tags);
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            }, new Collections.Coordinates.Collections.CoordinateArrayCollection<GeoCoordinate>(
                new GeoCoordinate[] { new GeoCoordinate(0.002242f, 0f) }));

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(),
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(vertex1), 1000, true);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(2, path.From.VertexId);
            Assert.IsNull(path.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, vertex2);
            var route = routebuilder.Build();
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(0, route.Segments[0].Distance);
            Assert.AreEqual(0, route.Segments[0].Time);
            Assert.AreEqual(250, route.Segments[1].Distance, 1);
            Assert.AreEqual(250 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[1].Time, 1);
            Assert.IsNotNull(route.Segments[1].Tags);
            Assert.AreEqual(1, route.Segments[1].Tags.Length);
            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);
            Assert.AreEqual(500, route.Segments[2].Distance);
            Assert.AreEqual(500 / Vehicle.Car.ProbableSpeed(tags).Value * 3.6, route.Segments[2].Time);
            Assert.IsNotNull(route.Segments[2].Tags);
            Assert.AreEqual(1, route.Segments[2].Tags.Length);
            Assert.AreEqual("highway", route.Segments[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[2].Tags[0].Value);
        }
    }
}