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

using OsmSharp.Math.Automata;
using OsmSharp.Math.StateMachines;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Instructions.MicroPlanning;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal.Machines
{
    /// <summary>
    /// Consumes all messages that relate to the same transit trip.
    /// </summary>
    public class TransitTripMachine : MicroPlannerMachine
    {
        /// <summary>
        /// Creates a new turn machine.
        /// </summary>
        /// <param name="planner">The planner.</param>
        public TransitTripMachine(ModalMicroPlanner planner)
            : base(planner, 2002)
        {

        }

        /// <summary>
        /// Builds the initial state.
        /// </summary>
        /// <returns></returns>
        protected override FiniteStateMachineState<MicroPlannerMessage> BuildStates()
        {
            // STATES:
            //
            //      0: NoTrip
            //      1: OnTrip
            //      2: TripEnded
            //

            // generate states.
            var states = FiniteStateMachineState<MicroPlannerMessage>.Generate(3);

            // state 2 is final.
            states[2].Final = true;
            // 0: NoTrip

            // NextTripDetected (Point)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(NextTripDetected),
                new FiniteStateMachineTransition<MicroPlannerMessage>.TransitionFinishedDelegate(this.OnTripDetected));
            // TripDetected (Arc)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessageArc),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TripDetected),
                new FiniteStateMachineTransition<MicroPlannerMessage>.TransitionFinishedDelegate(this.OnTripDetected));

            // 1: OnTrip

            // Any (Arc)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessageArc));
            // SameNextTripDetected (Point)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(SameNextTripDetected));
            // !SameNextTripDetected (Point)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 2, typeof(MicroPlannerMessagePoint),
                (machine, test) =>
                {
                    return !SameNextTripDetected(machine, test);
                });

            // return the start automata with intial state.
            return states[0];
        }

        /// <summary>
        /// Tests the given point for a trip on the next arc.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The arc.</param>
        /// <returns>True if there is a trip.</returns>
        private static bool NextTripDetected(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            {
                return TransitTripMachine.TripDetected(machine, (test as MicroPlannerMessagePoint).Point.Next);
            }
            return false;
        }

        /// <summary>
        /// Tests the given arc for a trip.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The arc.</param>
        /// <returns>True if there is a trip.</returns>
        private static bool TripDetected(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessageArc)
            {
                return TransitTripMachine.TripDetected(machine, (test as MicroPlannerMessageArc).Arc);
            }
            return false;
        }

        /// <summary>
        /// Tests the given arc for a trip.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="arc">The arc.</param>
        /// <returns>True if there is a trip.</returns>
        private static bool TripDetected(FiniteStateMachine<MicroPlannerMessage> machine, AggregatedArc arc)
        {
            if (arc != null &&
                arc.Tags != null)
            { // this arc is a transit tag.
                string trip_id;
                if (arc.Tags.TryGetValue("transit.trip.id", out trip_id))
                { // trips are equal.
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Holds the trip id that was detected (if any).
        /// </summary>
        private string _tripId = null;

        /// <summary>
        /// Called after a trip is detected.
        /// </summary>
        /// <param name="test"></param>
        private void OnTripDetected(object test)
        {
            if (test is MicroPlannerMessageArc)
            { // and arc.
                var tags = (test as MicroPlannerMessageArc).Arc.Tags;
                if (tags != null)
                { // this arc is a transit tag.
                    tags.TryGetValue("transit.trip.id", out _tripId);
                }
            }
            else if (test is MicroPlannerMessagePoint)
            { // and arc.
                var tags = (test as MicroPlannerMessagePoint).Point.Next.Tags;
                if (tags != null)
                { // this arc is a transit tag.
                    tags.TryGetValue("transit.trip.id", out _tripId);
                }
            }
        }

        /// <summary>
        /// Tests the next arc of the given point for the same trip as compared to the current state.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The point.</param>
        /// <returns>True if there is an identical trip.</returns>
        private bool SameNextTripDetected(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            { // and arc.
                var nextArc = (test as MicroPlannerMessagePoint).Point.Next;
                if(string.IsNullOrWhiteSpace(_tripId))
                {
                    throw new Exception("Cannot check for same trip without a previously detected trip.");
                }
                if (nextArc != null &&
                    nextArc.Tags != null)
                { // this arc is a transit tag.
                    string trip_id;
                    if (nextArc.Tags.TryGetValue("transit.trip.id", out trip_id))
                    { // trips are equal.
                        return trip_id != null && trip_id.Equals(_tripId);
                    }
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Called when this machine is successfull.
        /// </summary>
        public override void Succes()
        {
            var route = this.FinalMessages[0].Route;

            // keep a list of all poi's.
            var pois = new List<PointPoi>();

            // get first and last point.
            AggregatedPoint firstPoint = null;
            if (this.FinalMessages[0] is MicroPlannerMessagePoint)
            { // machine started on a point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessagePoint).Point;
            }
            else
            { // get the previous point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessageArc).Arc.Previous;
                if (firstPoint.Points != null) { pois.AddRange(firstPoint.Points); }
            }
            var lastPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessagePoint).Point;

            // get the data about the start of the found transit trip.
            var startTime = route.Segments[firstPoint.SegmentIdx].Time;
            var startDistance = route.Segments[firstPoint.SegmentIdx].Distance;

            // calculate total data.
            var totalDuration = route.Segments[lastPoint.SegmentIdx].Time - startTime;
            var totalDistance = route.Segments[lastPoint.SegmentIdx].Distance - startDistance;

            // collect poi's.
            foreach (var message in this.FinalMessages)
            {
                if (message is MicroPlannerMessagePoint)
                {
                    var pointPois = (message as MicroPlannerMessagePoint).Point.Points;
                    if (pointPois != null)
                    {
                        pois.AddRange(pointPois);
                    }
                }
            }
            var vehicle = string.Empty;
            if (route.Segments[lastPoint.SegmentIdx].Vehicle != null)
            { // there is a vehicle set.
                vehicle = route.Segments[lastPoint.SegmentIdx].Vehicle;
            }

            // generate one instruction for the entire trip.
            var metaData = this.GetConstantTagsForCurrentMessages().ToStringObjectDictionary();
            metaData["osmsharp.instruction.transit"] = "yes";
            metaData["osmsharp.instruction.type"] = "transit";
            metaData["osmsharp.instruction.duration"] = totalDuration;
            metaData["osmsharp.instruction.distance"] = totalDistance;
            metaData["osmsharp.instruction.time"] = route.Segments[lastPoint.SegmentIdx].Time;
            metaData["osmsharp.instruction.total_distance"] = route.Segments[lastPoint.SegmentIdx].Distance;
            metaData["osmsharp.instruction.vehicle"] = vehicle;
            this.Planner.SentencePlanner.GenerateInstruction(metaData, firstPoint.SegmentIdx, lastPoint.SegmentIdx, this.GetBoxForCurrentMessages(), pois);

        }
    }
}