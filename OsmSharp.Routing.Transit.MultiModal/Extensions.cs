using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Transit.Graphs;
using OsmSharp.Routing.Transit.RouteCalculators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal
{
    /// <summary>
    /// Contains extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a simple path into a timed path.
        /// </summary>
        /// <param name="simplePath"></param>
        /// <returns></returns>
        public static PathSegment<VertexTimeAndTrip> ConvertFrom(this PathSegment<long> simplePath)
        {
            if (simplePath == null)
            { // null is converted into null.
                return null;
            }

            if (simplePath.From != null)
            { // recursive call.
                return new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(simplePath.VertexId), simplePath.Weight,
                    simplePath.From.ConvertFrom());
            }
            return new PathSegment<VertexTimeAndTrip>(new VertexTimeAndTrip(simplePath.VertexId));
        }

        /// <summary>
        /// Converts a timed path into a simple path.
        /// </summary>
        /// <param name="timedPath"></param>
        /// <returns></returns>
        public static PathSegment<long> ConvertTo(this PathSegment<VertexTimeAndTrip> timedPath)
        {
            if (timedPath == null)
            { // null is converted into null.
                return null;
            }

            if (timedPath.From != null)
            { // recursive call.
                return new PathSegment<long>(timedPath.VertexId.Vertex, timedPath.Weight,
                    timedPath.From.ConvertTo());
            }
            return new PathSegment<long>(timedPath.VertexId.Vertex);
        }

        /// <summary>
        /// Converts a timed path into a simple path.
        /// </summary>
        /// <param name="timedPath"></param>
        /// <returns></returns>
        public static PathSegment<long>[] ConvertTo(this PathSegment<VertexTimeAndTrip>[] timedPath)
        {
            if (timedPath == null)
            { // null is converted into null.
                return null;
            }

            var converted = new PathSegment<long>[timedPath.Length];
            for (int idx = 0; idx < timedPath.Length; idx++)
            {
                converted[idx] = timedPath[idx].ConvertTo();
            }
            return converted;
        }

        ///// <summary>
        ///// Returns the one and only arc between the two given vertices.
        ///// </summary>
        ///// <typeparam name="TEdgeData"></typeparam>
        ///// <param name="graph"></param>
        ///// <param name="from"></param>
        ///// <param name="to"></param>
        ///// <returns></returns>
        //public static TEdgeData GetArc<TEdgeData>(this IDynamicGraphReadOnly<TEdgeData> graph, uint from, uint to)
        //    where TEdgeData : IDynamicGraphEdgeData
        //{
        //    var arcs = graph.GetArcs(from);
        //    foreach (var arc in arcs)
        //    {
        //        if (arc.Key == to)
        //        {
        //            return arc.Value;
        //        }
        //    }
        //    throw new ArgumentOutOfRangeException("No arc found between the two given vertices.");
        //}

        /// <summary>
        /// Returns the vertex closest to the given coordinates.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static uint GetVertexAt<TEdgeData>(this DynamicGraphRouterDataSource<TEdgeData> graph, GeoCoordinate coordinate)
            where TEdgeData : IDynamicGraphEdgeData
        {
            double distance = double.MaxValue;
            uint vertex = 0;
            float latitude, longitude;
            for(uint idx = 1; idx <= graph.VertexCount; idx++)
            {
                if (graph.GetVertex(idx, out latitude, out longitude))
                {
                    double currentDistance = coordinate.Distance(new GeoCoordinate(latitude, longitude));
                    if(currentDistance < distance)
                    {
                        vertex = idx;
                        distance = currentDistance;
                    }
                }
            }
            return vertex;
        }

        /// <summary>
        /// Holds the schedule id limit.
        /// </summary>
        public const uint SCHEDULE_ID_MAX = uint.MaxValue / 2;

        /// <summary>
        /// Encodes the given schedule id.
        /// </summary>
        /// <param name="edge"></param>
        public static uint EncodeScheduleId(int id)
        {
            return (uint)(SCHEDULE_ID_MAX - id);
        }

        /// <summary>
        /// Returns the schedule id (if any) for the given edge.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static uint? GetScheduleId(this LiveEdge edge)
        {
            uint tagsId = edge.Tags;
            if (tagsId > (SCHEDULE_ID_MAX / 2))
            { // there is a schedule for this.
                return SCHEDULE_ID_MAX - tagsId;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the given edge is a road.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static bool IsRoad(this LiveEdge edge)
        {
            if(edge.Tags == 0)
            { // no road, no transite.
                return false;
            }
            return !edge.GetScheduleId().HasValue;
        }

        /// <summary>
        /// Returns true if the given edge is a transit edge.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public static bool IsTransit(this LiveEdge edge)
        {
            if (edge.Tags == 0)
            { // no road, no transite.
                return false;
            }
            return edge.GetScheduleId().HasValue;
        }

        /// <summary>
        /// Returns the schedules for this given edge from the given schedules collection.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="schedules"></param>
        /// <returns></returns>
        public static TransitEdgeSchedulePair GetSchedule(this LiveEdge edge, List<TransitEdgeSchedulePair> schedules)
        {
            uint? scheduleId = edge.GetScheduleId();
            if (scheduleId.HasValue)
            { // the scheduleId is there.
                return schedules[(int)scheduleId.Value];
            }
            return null;
        }

        /// <summary>
        /// Returns the backward schedule for the given edge from the given schedules collection.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="schedules"></param>
        /// <returns></returns>
        public static TransitEdgeSchedule GetBackwardSchedule(this LiveEdge edge, List<TransitEdgeSchedulePair> schedules)
        {
            var schedulePair = edge.GetSchedule(schedules);
            if(schedulePair != null)
            {
                return schedulePair.Backward;
            }
            return null;
        }

        /// <summary>
        /// Returns the forward schedule for the given edge from the given schedules collection.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="schedules"></param>
        /// <returns></returns>
        public static TransitEdgeSchedule GetForwardSchedule(this LiveEdge edge, List<TransitEdgeSchedulePair> schedules)
        {
            var schedulePair = edge.GetSchedule(schedules);
            if (schedulePair != null)
            {
                return schedulePair.Forward;
            }
            return null;
        }
    }
}