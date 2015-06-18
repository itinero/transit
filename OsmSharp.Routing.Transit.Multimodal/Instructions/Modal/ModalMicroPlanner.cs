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

using OsmSharp.Routing.Instructions.MicroPlanning;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Transit.Multimodal.Instructions.Modal.Machines;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal
{
    /// <summary>
    /// A micro planner including extra machine for transit routing.
    /// </summary>
    public class ModalMicroPlanner : OsmSharp.Routing.Instructions.MicroPlanning.MicroPlanner
    {
        /// <summary>
        /// Creates a new micro planner.
        /// </summary>
        /// <param name="languageGenerator">The language generator.</param>
        /// <param name="interpreter">The interpreter.</param>
        public ModalMicroPlanner(ModalLanguageGenerator languageGenerator, IRoutingInterpreter interpreter)
            : base(languageGenerator, interpreter)
        {

        }

        /// <summary>
        /// Initializes the list of machines.
        /// </summary>
        /// <param name="machines">The list of machines.</param>
        protected override void InitializeMachines(List<MicroPlannerMachine> machines)
        {
            machines.Add(new AnythingButTransitMachine(this));
            machines.Add(new TransitTripMachine(this));
            machines.Add(new TransferMachine(this));
        }
    }
}