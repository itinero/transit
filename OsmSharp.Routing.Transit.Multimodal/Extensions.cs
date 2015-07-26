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

using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal
{
    /// <summary>
    /// Contains extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sets all elements to the given value.
        /// </summary>
        public static void SetAll<T>(this IList<T> list, T value)
        {
            for(int i = 0; i < list.Count; i++)
            {
                list[i] = value;
            }
        }

        /// <summary>
        /// Adds elements to the given list with the given value.
        /// </summary>
        public static void AddAll<T>(this IList<T> list, T value, int count)
        {
            while(count > 0)
            {
                count--;
                list.Add(value);
            }
        }
    }
}