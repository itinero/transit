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

using System;

namespace OsmSharp.Routing.Transit.MultiModal.RouteCalculators
{
    /// <summary>
    /// Represents a modal weight.
    /// </summary>
    public class ModalWeight : IComparable
    {
        /// <summary>
        /// Creates a new modal weight.
        /// </summary>
        /// <param name="weight"></param>
        public ModalWeight(float weight)
        {
            this.Time = weight;
            this.TimeWithoutWaiting = weight;
            this.Transfers = 0;
        }

        ///// <summary>
        ///// Creates a new modal weight.
        ///// </summary>
        ///// <param name="weight"></param>
        ///// <param name="timeWithoutWaiting"></param>
        //public ModalWeight(float weight, float timeWithoutWaiting)
        //{
        //    this.Time = weight;
        //    this.TimeWithoutWaiting = timeWithoutWaiting;
        //    this.Transfers = 0;
        //}

        /// <summary>
        /// Creates a new modal weight.
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="timeWithoutWaiting"></param>
        /// <param name="transfers"></param>
        public ModalWeight(float weight, float timeWithoutWaiting, uint transfers)
        {
            this.Time = weight;
            this.Transfers = transfers;
            this.TimeWithoutWaiting = timeWithoutWaiting;
        }

        /// <summary>
        /// Holds the time up until now.
        /// </summary>
        public float Time { get; private set; }

        /// <summary>
        /// Holds the time up until now but without waiting.
        /// </summary>
        public float TimeWithoutWaiting { get; private set; }

        /// <summary>
        /// Holds the transfer count up until now.
        /// </summary>
        public uint Transfers { get; private set; }

        /// <summary>
        /// Compares this modal weight to another.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var other = (obj as ModalWeight);
            if (this.Transfers != other.Transfers &&
                System.Math.Abs(this.Time - other.Time) < 5 * 60)
            { // transfers differ, when time is small only compare transfers.
                return this.Transfers.CompareTo(other.Transfers);
            }
            if(this.Time == other.Time)
            { // the time is exactly the same, just check the waiting time.
                return this.TimeWithoutWaiting.CompareTo(other.TimeWithoutWaiting);
            }
            return this.Time.CompareTo(other.Time);
        }
    }
}