//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp.Math.Automata;
//using OsmSharp.Math.StateMachines;
//using OsmSharp.Routing.Instructions.ArcAggregation.Output;
//using OsmSharp.Routing.Instructions.MicroPlanning;
//using System;

//namespace OsmSharp.Routing.Transit.Multimodal.Instructions.Modal.Machines
//{
//    /// <summary>
//    /// Consumes all messages that represent a transfer.
//    /// </summary>
//    public class TransferMachine : MicroPlannerMachine
//    {
//        /// <summary>
//        /// Creates a new turn machine.
//        /// </summary>
//        /// <param name="planner">The planner.</param>
//        public TransferMachine(ModalMicroPlanner planner)
//            : base(planner, 2001)
//        {

//        }

//        /// <summary>
//        /// Builds the initial state.
//        /// </summary>
//        /// <returns></returns>
//        protected override FiniteStateMachineState<MicroPlannerMessage> BuildStates()
//        {
//            // STATES:
//            //
//            //      0: Transfer
//            //      1: TransferEnded
//            //

//            // generate states.
//            var states = FiniteStateMachineState<MicroPlannerMessage>.Generate(3);

//            // state 2 is final.
//            states[2].Final = true;
//            // 0: NoTrip

//            // Any (Point)
//            // IsTransfer (Arc)
//            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessagePoint),
//                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(IsNextATransfer));
//            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessageArc),
//                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(IsTransfer));

//            // 1: OnTrip
//            // Any (Arc)
//            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessageArc));
//            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessagePoint),
//                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(IsNextATransfer));
//            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 2, typeof(MicroPlannerMessagePoint),
//                (machine, test) =>
//                {
//                    return !IsNextATransfer(machine, test);
//                });

//            // return the start automata with intial state.
//            return states[0];
//        }

//        /// <summary>
//        /// Tests the given arc for a transfer.
//        /// </summary>
//        /// <param name="machine">The machine.</param>
//        /// <param name="test">The arc.</param>
//        /// <returns>True if there is a transfer.</returns>
//        private static bool IsTransfer(FiniteStateMachine<MicroPlannerMessage> machine, object test)
//        {
//            if (test == null) { return false; }

//            if (test is MicroPlannerMessageArc)
//            { // test for transfer.
//                return TransferMachine.IsTransfer(machine, (test as MicroPlannerMessageArc).Arc);
//            }
//            throw new InvalidOperationException("IsTransfer can only test arcs.");
//        }

//        /// <summary>
//        /// Tests the given arc for a transfer.
//        /// </summary>
//        /// <param name="machine">The machine.</param>
//        /// <param name="arc">The arc.</param>
//        /// <returns>True if there is a transfer.</returns>
//        private static bool IsTransfer(FiniteStateMachine<MicroPlannerMessage> machine, AggregatedArc arc)
//        {
//            if (arc != null &&
//                arc.Tags != null)
//            { // there are tags.
//                return arc.Vehicle == OsmSharp.Routing.Transit.Constants.WaitVehicle ||
//                    arc.Vehicle == OsmSharp.Routing.Transit.Constants.TransferVehicle;
//            }
//            return false;
//        }

//        /// <summary>
//        /// Tests the point for having an arc that represents a tranfer.
//        /// </summary>
//        /// <param name="machine">The machine.</param>
//        /// <param name="test">The point.</param>
//        /// <returns>True if there is a transfer.</returns>
//        private static bool IsNextATransfer(FiniteStateMachine<MicroPlannerMessage> machine, object test)
//        {
//            if (test == null) { return false; }

//            if (test is MicroPlannerMessagePoint)
//            { // test point.next
//                return TransferMachine.IsTransfer(machine, (test as MicroPlannerMessagePoint).Point.Next);
//            }
//            throw new InvalidOperationException("IsNextATransfer can only test points.");
//        }

//        /// <summary>
//        /// Called when this machine is successfull.
//        /// </summary>
//        public override void Succes()
//        {
//            var route = this.FinalMessages[0].Route;

//            // get first and last point.
//            AggregatedPoint firstPoint = null;
//            if (this.FinalMessages[0] is MicroPlannerMessagePoint)
//            { // machine started on a point.
//                firstPoint = (this.FinalMessages[0] as MicroPlannerMessagePoint).Point;
//            }
//            else
//            { // get the previous point.
//                firstPoint = (this.FinalMessages[0] as MicroPlannerMessageArc).Arc.Previous;
//            }
//            var lastPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessagePoint).Point;

//            // get the segment.
//            var duration = route.Segments[lastPoint.SegmentIdx].Time -
//                route.Segments[firstPoint.SegmentIdx].Time;
//            var distance = route.Segments[lastPoint.SegmentIdx].Distance -
//                route.Segments[firstPoint.SegmentIdx].Distance;

//            var metaData = this.GetConstantTagsForCurrentMessages().ToStringObjectDictionary();
//            metaData["osmsharp.instruction.transit"] = "yes";
//            metaData["osmsharp.instruction.type"] = "transfer";
//            metaData["osmsharp.instruction.duration"] = duration;
//            metaData["osmsharp.instruction.time"] = route.Segments[lastPoint.SegmentIdx].Time;
//            metaData["osmsharp.instruction.distance"] = distance;
//            metaData["osmsharp.instruction.total_distance"] = route.Segments[lastPoint.SegmentIdx].Distance;
//            this.Planner.SentencePlanner.GenerateInstruction(metaData, firstPoint.SegmentIdx, lastPoint.SegmentIdx, this.GetBoxForCurrentMessages(), firstPoint.Points);
//        }
//    }
//}