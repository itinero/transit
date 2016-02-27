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
using System;

namespace OsmSharp.Transit.Test
{
    /// <summary>
    /// Contains tests for the routing algorithm base.
    /// </summary>
    [TestFixture]
    public class RoutingAlgorithmBaseTests
    {
        /// <summary>
        /// Tests HasRun.
        /// </summary>
        [Test]
        public void TestHasRun()
        {
            var algorithm = new RoutingAlgorithmMock();

            Assert.IsFalse(algorithm.HasRun);
            algorithm.Run();
            Assert.IsTrue(algorithm.HasRun);
        }
        
        /// <summary>
        /// Tests HasSucceeded.
        /// </summary>
        [Test]
        public void TestHasSucceeded()
        {
            var algorithm = new RoutingAlgorithmMock();
      
            Assert.IsFalse(algorithm.HasSucceeded);
            algorithm.Run();
            Assert.IsFalse(algorithm.HasSucceeded);

            algorithm = new RoutingAlgorithmMock();
            algorithm.RunDelegate = () => { return true; };
            Assert.IsFalse(algorithm.HasSucceeded);
            algorithm.Run();
            Assert.IsTrue(algorithm.HasSucceeded);
        }

        /// <summary>
        /// Tests Check methods.
        /// </summary>
        [Test]
        public void TestChecks()
        {
            var algorithm = new RoutingAlgorithmMock();

            Assert.Catch<Exception>(() => { algorithm.DoCheckHasRun(); });
            Assert.Catch<Exception>(() => { algorithm.DoCheckHasRunAndHasSucceeded(); });

            algorithm.Run();

            algorithm.DoCheckHasRun();
            Assert.Catch<Exception>(() => { algorithm.DoCheckHasRunAndHasSucceeded(); });

            algorithm = new RoutingAlgorithmMock();
            algorithm.RunDelegate = () => { return true; };
            algorithm.Run();

            algorithm.DoCheckHasRun();
            algorithm.DoCheckHasRunAndHasSucceeded();
        }

        /// <summary>
        /// Tests double run check.
        /// </summary>
        [Test]
        public void TestDoubleRunCheck()
        {
            var algorithm = new RoutingAlgorithmMock();

            algorithm.Run();
            Assert.Catch<Exception>(() => { algorithm.Run(); });
        }
    }
}