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

namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal.Machines
{
    /// <summary>
    /// A micro planned machine consuming anything that is not transit and giving only one instruction for an entire segment of non-transit having the same vehicle.
    /// </summary>
    public class AnythingButTransitMachine : MicroPlannerMachine
    {
        /// <summary>
        /// Creates a new turn machine.
        /// </summary>
        /// <param name="planner">The planner.</param>
        public AnythingButTransitMachine(ModalMicroPlanner planner)
            : base(planner, 2000)
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
            //      0: NoTransit
            //      1: NoTransit
            //      2: NextNotTransit
            //

            // generate states.
            var states = FiniteStateMachineState<MicroPlannerMessage>.Generate(3);

            // state 2 is final.
            states[2].Final = true;

            // 0: NoTransit

            // !IsNextTransit (Point)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(
                    (machine, test) =>
                    {
                        return !IsNextTransit(machine, test);
                    }),
                    new FiniteStateMachineTransition<MicroPlannerMessage>.TransitionFinishedDelegate(this.OnNonTransitDetected));
            // !IsTransit (Arc)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessageArc),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(
                    (machine, test) =>
                    {
                        return !IsTransit(machine, test);
                    }),
                    new FiniteStateMachineTransition<MicroPlannerMessage>.TransitionFinishedDelegate(this.OnNonTransitDetected));

            // 1: NoTransit

            // !IsNextTransit (Point)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(
                    (machine, test) =>
                    {
                        return !IsNextNull(machine, test) && !IsNextTransit(machine, test) && SameNextVehicleDetected(machine, test);
                    }));
            // !IsTransit (Arc)
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessageArc),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(
                    (machine, test) =>
                    {
                        return !IsNextNull(machine, test) && !IsTransit(machine, test);
                    }));
            // IsNextTransit (Point) || IsNextNull
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 2, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(
                    (machine, test) =>
                    {
                        return IsNextTransit(machine, test) || IsNextNull(machine, test) || !SameNextVehicleDetected(machine, test);
                    }));

            // 2: NextIsTransit

            // final state.

            // return the start automata with intial state.
            return states[0];
        }

        /// <summary>
        /// Tests the given point for a trip on the next arc.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The arc.</param>
        /// <returns>True if there is a trip.</returns>
        private static bool IsTransit(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessageArc)
            {
                return AnythingButTransitMachine.IsTransit(machine, (test as MicroPlannerMessageArc).Arc);
            }
            return false;
        }

        /// <summary>
        /// Tests the given arc for any transit information.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="arc">The arc.</param>
        /// <returns></returns>
        private static bool IsTransit(FiniteStateMachine<MicroPlannerMessage> machine, AggregatedArc arc)
        {
            if (arc != null &&
                arc.Tags != null)
            {
                if (arc.Tags.ContainsKey("transit.stop.id") ||
                    arc.Tags.ContainsKeyValue("type", "intermodal"))
                { // this arc has a transit tag.
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests the next arc of the given point for any transit information.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The point.</param>
        /// <returns></returns>
        private bool IsNextTransit(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            { // and arc.
                var nextArc = (test as MicroPlannerMessagePoint).Point.Next;
                return AnythingButTransitMachine.IsTransit(this, nextArc);
            }
            return false;
        }

        /// <summary>
        /// Tests the next arc for it's existence.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The point.</param>
        /// <returns></returns>
        private bool IsNextNull(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            { // and arc.
                var nextArc = (test as MicroPlannerMessagePoint).Point.Next;
                return nextArc == null;
            }
            return false;
        }

        /// <summary>
        /// Holds the current vehicle.
        /// </summary>
        private string _vehicle;

        /// <summary>
        /// Called after a non-transit link is detected.
        /// </summary>
        /// <param name="test"></param>
        private void OnNonTransitDetected(object test)
        {
            if (test is MicroPlannerMessageArc)
            { // and arc.
                _vehicle = (test as MicroPlannerMessageArc).Arc.Vehicle;
            }
            else if (test is MicroPlannerMessagePoint)
            { // and arc.
                var arc = (test as MicroPlannerMessagePoint).Point.Next;
                if (arc != null)
                {
                    _vehicle = arc.Vehicle;
                }
            }
        }

        /// <summary>
        /// Tests the given arc for a vehicle.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="arc">The arc.</param>
        /// <returns>True if the vehicle is equal to the current vehicle.</returns>
        private bool SameVehicleDetectedOnArc(FiniteStateMachine<MicroPlannerMessage> machine, AggregatedArc arc)
        {
            if (arc != null)
            { // this arc is a transit tag.
                if (arc.Vehicle == _vehicle ||
                    string.IsNullOrWhiteSpace(_vehicle))
                { // vehicle are equal.
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests the next arc of the given point for the same vehicle as compared to the current state.
        /// </summary>
        /// <param name="machine">The machine.</param>
        /// <param name="test">The point.</param>
        /// <returns>True if there is an identical trip.</returns>
        private bool SameNextVehicleDetected(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            { // and arc.
                var nextArc = (test as MicroPlannerMessagePoint).Point.Next;
                return this.SameVehicleDetectedOnArc(this, nextArc);
            }
            return false;
        }

        /// <summary>
        /// Called when this machine is successfull.
        /// </summary>
        public override void Succes()
        {
            var route = this.FinalMessages[0].Route;

            // get first and last point.
            AggregatedPoint firstPoint = null;
            if (this.FinalMessages[0] is MicroPlannerMessagePoint)
            { // machine started on a point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessagePoint).Point;
            }
            else
            { // get the previous point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessageArc).Arc.Previous;
            }
            var lastPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessagePoint).Point;

            // get the segment.
            var duration = route.Segments[lastPoint.SegmentIdx].Time -
                route.Segments[firstPoint.SegmentIdx].Time;
            var distance = route.Segments[lastPoint.SegmentIdx].Distance -
                route.Segments[firstPoint.SegmentIdx].Distance;
            var vehicle = string.Empty;
            if (route.Segments[lastPoint.SegmentIdx].Vehicle != null)
            { // there is a vehicle set.
                vehicle = route.Segments[lastPoint.SegmentIdx].Vehicle;
            }

            var metaData = this.GetConstantTagsForCurrentMessages().ToStringObjectDictionary();
            metaData["osmsharp.instruction.type"] = "anything_but_transit";
            metaData["osmsharp.instruction.duration"] = duration;
            metaData["osmsharp.instruction.time"] = route.Segments[lastPoint.SegmentIdx].Time;
            metaData["osmsharp.instruction.distance"] = distance;
            metaData["osmsharp.instruction.vehicle"] = vehicle;
            metaData["osmsharp.instruction.total_distance"] = route.Segments[lastPoint.SegmentIdx].Distance;
            this.Planner.SentencePlanner.GenerateInstruction(metaData, firstPoint.SegmentIdx, lastPoint.SegmentIdx, this.GetBoxForCurrentMessages(), lastPoint.Points);
        }
    }
}
