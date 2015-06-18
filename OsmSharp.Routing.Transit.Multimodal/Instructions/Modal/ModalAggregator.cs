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

using OsmSharp.Routing.Instructions.ArcAggregation;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Interpreter;

namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal
{
    /// <summary>
    /// A modal arc aggregator.
    /// </summary>
    public class ModalAggregator : ArcAggregator
    {
        /// <summary>
        /// Creates a new multi modal arc aggregator.
        /// </summary>
        /// <param name="interpreter"></param>
        public ModalAggregator(IRoutingInterpreter interpreter)
            : base(interpreter)
        {

        }

        /// <summary>
        /// Returns true if the change between the two given arcs is significant.
        /// </summary>
        /// <returns></returns>
        protected override bool IsSignificant(AggregatedArc previousArc, AggregatedArc nextArc)
        {
            return !nextArc.Vehicle.Equals(previousArc.Vehicle) || 
                nextArc.Vehicle.StartsWith("Transit") ||
                previousArc.Vehicle.StartsWith("Transit");
        }
    }
}