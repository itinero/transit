// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using OsmSharp.Routing.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// An edge representing a transit link between two stops.
    /// </summary>
    public class TransitEdge : IDynamicGraphEdgeData
    {
        /// <summary>
        /// Creates a new transit edge.
        /// </summary>
        public TransitEdge()
        {
            this.ForwardSchedule = new TransitEdgeSchedule();
            this.BackwardSchedule = new TransitEdgeSchedule();
        }

        public OsmSharp.Math.Geo.Simple.GeoCoordinateSimple[] Coordinates
        {
            get;
            set;
        }

        public bool EqualsGeometrically(IDynamicGraphEdgeData other)
        {
            throw new NotImplementedException();
        }

        public bool Forward
        {
            get;
            set;
        }

        public bool RepresentsNeighbourRelations
        {
            get { return true; }
        }

        public uint Tags
        {
            get;
            set;
        }

        public bool Equals(IDynamicGraphEdgeData other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the forward transit edge schedule.
        /// </summary>
        public TransitEdgeSchedule ForwardSchedule { get; private set; }

        /// <summary>
        /// Gets or sets the backward transit edge schedule.
        /// </summary>
        public TransitEdgeSchedule BackwardSchedule { get; private set; }
    }
}