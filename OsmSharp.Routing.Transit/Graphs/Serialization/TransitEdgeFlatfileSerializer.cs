using OsmSharp.Collections.Tags.Index;
using OsmSharp.IO;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Graphs.Serialization
{
    /// <summary>
    /// Serializes/deserializes edges.
    /// </summary>
    public class TransitEdgeFlatfileSerializer : FlatfileSerializer<TransitEdge>
    {
        /// <summary>
        /// Creates the graph to deserialize into.
        /// </summary>
        /// <param name="tagsCollectionIndex"></param>
        /// <returns></returns>
        protected override DynamicGraphRouterDataSource<TransitEdge> CreateGraph(ITagsCollectionIndex tagsCollectionIndex)
        {
            return new DynamicGraphRouterDataSource<TransitEdge>(tagsCollectionIndex);
        }

        /// <summary>
        /// Serializes all edges.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        protected override void SerializeEdges(LimitedStream stream, DynamicGraphRouterDataSource<TransitEdge> graph)
        {
            var typeModel = RuntimeTypeModel.Create();
            typeModel.Add(typeof(SerializableEdge), true);
            typeModel.Add(typeof(GeoCoordinateSimple), true);

            int blockSize = 1000;
            var arcsQueue = new List<SerializableEdge>(blockSize);

            uint vertex = 0;
            while (vertex < graph.VertexCount)
            { // keep looping and serialize all vertices.
                var arcs = graph.GetArcs(vertex);
                if (arcs != null)
                { // serialize the arcs.
                    for (int idx = 0; idx < arcs.Length; idx++)
                    {
                        var serializableEdge = new SerializableEdge()
                        {
                            FromId = vertex,
                            ToId = arcs[idx].Key,
                            Coordinates = arcs[idx].Value.Coordinates,
                        };

                        if(arcs[idx].Value.ForwardSchedule != null)
                        {
                            var serializableSchedule = new SerializableSchedule[arcs[idx].Value.ForwardSchedule.Entries.Count];
                            for (int schedIdx = 0; schedIdx < arcs[idx].Value.ForwardSchedule.Entries.Count; schedIdx++)
                            {
                                serializableSchedule[schedIdx] = new SerializableSchedule()
                                {
                                    ArrivalTime = arcs[idx].Value.ForwardSchedule.Entries[schedIdx].ArrivalTime,
                                    DepartureTime = arcs[idx].Value.ForwardSchedule.Entries[schedIdx].DepartureTime,
                                    Trip = arcs[idx].Value.ForwardSchedule.Entries[schedIdx].Trip
                                };
                            }
                            serializableEdge.ForwardSchedule = serializableSchedule;
                        }

                        if(arcs[idx].Value.BackwardSchedule != null)
                        {
                            var serializableSchedule = new SerializableSchedule[arcs[idx].Value.BackwardSchedule.Entries.Count];
                            for (int schedIdx = 0; schedIdx < arcs[idx].Value.BackwardSchedule.Entries.Count; schedIdx++)
                            {
                                serializableSchedule[schedIdx] = new SerializableSchedule()
                                {
                                    ArrivalTime = arcs[idx].Value.BackwardSchedule.Entries[schedIdx].ArrivalTime,
                                    DepartureTime = arcs[idx].Value.BackwardSchedule.Entries[schedIdx].DepartureTime,
                                    Trip = arcs[idx].Value.BackwardSchedule.Entries[schedIdx].Trip
                                };
                            }
                            serializableEdge.BackwardSchedule = serializableSchedule;
                        }
                        arcsQueue.Add(serializableEdge);

                        if (arcsQueue.Count == blockSize)
                        { // execute serialization.
                            typeModel.SerializeWithSize(stream, arcsQueue.ToArray());
                            arcsQueue.Clear();
                        }
                    }

                    // serialize.
                    vertex++;
                }
            }

            if (arcsQueue.Count > 0)
            { // execute serialization.
                typeModel.SerializeWithSize(stream, arcsQueue.ToArray());
                arcsQueue.Clear();
            }
        }

        /// <summary>
        /// Deserializes all edges.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <param name="graph"></param>
        protected override void DeserializeEdges(LimitedStream stream, long size, DynamicGraphRouterDataSource<TransitEdge> graph)
        {
            var typeModel = RuntimeTypeModel.Create();
            typeModel.Add(typeof(SerializableEdge), true);
            typeModel.Add(typeof(GeoCoordinateSimple), true);
            typeModel.Add(typeof(SerializableSchedule), true);

            long position = stream.Position;
            while (stream.Position < position + size)
            { // keep looping until the appropriate number of bytes have been read.
                var serializableEdges = typeModel.DeserializeWithSize(stream, null, typeof(SerializableEdge[])) as SerializableEdge[];
                for (int idx = 0; idx < serializableEdges.Length; idx++)
                {
                    var serializedEdge = serializableEdges[idx];
                    var forwardSchedule = new TransitEdgeSchedule();
                    if(serializedEdge.ForwardSchedule != null)
                    {
                        foreach(var scheduleEntry in serializedEdge.ForwardSchedule)
                        {
                            forwardSchedule.Add(scheduleEntry.Trip, (int)scheduleEntry.DepartureTime, (int)scheduleEntry.ArrivalTime);
                        }
                    }

                    var backwardSchedule = new TransitEdgeSchedule();
                    if (serializedEdge.BackwardSchedule != null)
                    {
                        foreach (var scheduleEntry in serializedEdge.BackwardSchedule)
                        {
                            backwardSchedule.Add(scheduleEntry.Trip, (int)scheduleEntry.DepartureTime, (int)scheduleEntry.ArrivalTime);
                        }
                    }

                    graph.AddArc(serializableEdges[idx].FromId, serializableEdges[idx].ToId,
                        new TransitEdge()
                        {
                            Coordinates = serializableEdges[idx].Coordinates,
                            
                        }, null);
                }
            }
        }

        /// <summary>
        /// Returns the version string.
        /// </summary>
        public override string VersionString
        {
            get { return "TransitEdgeFlatfile.v1.0"; }
        }

        /// <summary>
        /// A serializable edge.
        /// </summary>
        [ProtoContract]
        private class SerializableEdge
        {
            /// <summary>
            /// Gets or sets the from id.
            /// </summary>
            [ProtoMember(1)]
            public uint FromId { get; set; }

            /// <summary>
            /// Gets or sets the to id.
            /// </summary>
            [ProtoMember(2)]
            public uint ToId { get; set; }

            /// <summary>
            /// Gets or sets the coordinates.
            /// </summary>
            [ProtoMember(3)]
            public GeoCoordinateSimple[] Coordinates { get; set; }

            /// <summary>
            /// Gets or sets the forward schedule.
            /// </summary>
            [ProtoMember(4)]
            public SerializableSchedule[] ForwardSchedule { get; set; }

            /// <summary>
            /// Gets or sets the backward schedule.
            /// </summary>
            [ProtoMember(5)]
            public SerializableSchedule[] BackwardSchedule { get; set; }
        }

        /// <summary>
        /// A serializable schedule.
        /// </summary>
        [ProtoContract]
        private class SerializableSchedule
        {
            /// <summary>
            /// Gets or sets the trip.
            /// </summary>
            [ProtoMember(1)]
            public uint Trip { get; set; }

            /// <summary>
            /// Gets or sets the departure time in seconds since the beginning of the day.
            /// </summary>
            [ProtoMember(2)]
            public uint DepartureTime { get; set; }

            /// <summary>
            /// Gets or sets the arrival time in seconds since the end of the day.
            /// </summary>
            [ProtoMember(3)]
            public uint ArrivalTime { get; set; }
        }
    }
}
