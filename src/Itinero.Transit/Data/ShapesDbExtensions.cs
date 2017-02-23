// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.LocalGeo;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// Contains extension and helper methods for the shapes db.
    /// </summary>
    public static class ShapesDbExtensions
    {
        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public static void Add(this ShapesDb shapesDb, uint stop1, uint stop2, params Coordinate[] coordinates)
        {
            shapesDb.Add(stop1, stop2, new Graphs.Geometric.Shapes.ShapeEnumerable(coordinates));
        }
    }
}