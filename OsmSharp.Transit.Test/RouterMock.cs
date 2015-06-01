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

using OsmSharp.Routing.Transit;
using System;

namespace OsmSharp.Transit.Test
{
    /// <summary>
    /// Abstract representation of a routing algorithm.
    /// </summary>
    class RouterMock : RouterBase
    {
        /// <summary>
        /// Gets or sets the run delegate.
        /// </summary>
        public Func<bool> RunDelegate { get; set; }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            if (this.RunDelegate != null &&
                this.RunDelegate())
            { // delegate was called and returned true.
                this.HasSucceeded = true;
            }
        }

        /// <summary>
        /// Executes the check.
        /// </summary>
        public void DoCheckHasRun()
        {
            this.CheckHasRun();
        }

        /// <summary>
        /// Executes the check.
        /// </summary>
        public void DoCheckHasRunAndHasSucceeded()
        {
            this.CheckHasRunAndHasSucceeded();
        }
    }
}