﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            this.Transfers = 0;
        }

        /// <summary>
        /// Creates a new modal weight.
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="transfers"></param>
        public ModalWeight(float weight, uint transfers)
        {
            this.Time = weight;
            this.Transfers = transfers;
        }

        /// <summary>
        /// Holds the time up until now.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Holds the transfer count up until now.
        /// </summary>
        public uint Transfers { get; set; }

        /// <summary>
        /// Compares this modal weight to another.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var other = (obj as ModalWeight);
            if(other != null)
            {
                var time = this.Time.CompareTo(other.Time);
                if(time == 0)
                {
                    return this.Transfers.CompareTo(other.Transfers);
                }
                return time;
            }
            throw new ArgumentOutOfRangeException("ModalWeight cannot be compared to object of other type.");
        }
    }
}
