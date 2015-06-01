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
            var tagsId = graph.TagsIndex.Add(new TagsCollection(new Tag() { Key = "highway", Value = "residential" }));
            var vertex1 = graph.AddVertex(0.000000f, 0f);
            var vertex2 = graph.AddVertex(0.004484f, 0f);
            var vertex3 = graph.AddVertex(0.010514f, 0f);
            var vertex4 = graph.AddVertex(0.014998f, 0f);
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Distance = 500,
                Forward = true,
                Tags = tagsId
            });

            // execute routing algorithm.
            var algorithm = new OneToManyDykstra(graph, new OsmRoutingInterpreter(), 
                Vehicle.Car, new Routing.Graph.Routing.PathSegmentVisitList(1), 1000, false);
            algorithm.Run();

            // build path.
            var routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, 2);
            var path = routebuilder.BuildPath();
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.VertexId);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(1, path.From.VertexId);
            Assert.IsNull(path.From.From);

            // build route.
            routebuilder = new OneToManyDykstraRouteBuilder(graph, algorithm, 2);
            var route = routebuilder.Build();
            //Assert.IsNotNull(path);
            //Assert.AreEqual(2, path.VertexId);
            //Assert.IsNotNull(path.From);
            //Assert.AreEqual(1, path.From.VertexId);
            //Assert.IsNull(path.From.From);
        }

        //public void TestTwoHops()
        //{

        //    // build a tiny test graph.
        //    var graph = new RouterDataSource<Edge>(new Graph<Edge>(), new TagsIndex());
        //    var tagsId = graph.TagsIndex.Add(new TagsCollection(new Tag() { Key = "highway", Value = "residential" }));
        //    var vertex1 = graph.AddVertex(0.000000f, 0f);
        //    var vertex2 = graph.AddVertex(0.004484f, 0f);
        //    var vertex3 = graph.AddVertex(0.010514f, 0f);
        //    var vertex4 = graph.AddVertex(0.014998f, 0f);
        //    graph.AddEdge(vertex1, vertex2, new Edge()
        //    {
        //        Distance = 500,
        //        Forward = true,
        //        Tags = tagsId
        //    });
        //    graph.AddEdge(vertex3, vertex4, new Edge()
        //    {
        //        Distance = 500,
        //        Forward = true,
        //        Tags = tagsId
        //    });
        //}
    }
}