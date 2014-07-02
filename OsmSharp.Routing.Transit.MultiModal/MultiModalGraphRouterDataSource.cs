using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Osm.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal
{
    /// <summary>
    /// Represents a multimodal graph router data source.
    /// </summary>
    public class MultiModalGraphRouterDataSource
    {
        /// <summary>
        /// Holds the schedules.
        /// </summary>
        private List<TransitEdgeSchedulePair> _schedules;

        /// <summary>
        /// Holds the graph datasource.
        /// </summary>
        private DynamicGraphRouterDataSource<LiveEdge> _graph;

        /// <summary>
        /// Creates a new multi modal graph router.
        /// </summary>
        /// <param name="graph"></param>
        public MultiModalGraphRouterDataSource(DynamicGraphRouterDataSource<LiveEdge> graph)
        {
            _schedules = new List<TransitEdgeSchedulePair>();
            _graph = graph;
        }

        /// <summary>
        /// Returns the schedule.
        /// </summary>
        public List<TransitEdgeSchedulePair> Schedules
        {
            get
            {
                return _schedules;
            }
        }

        /// <summary>
        /// Returns the graph.
        /// </summary>
        public DynamicGraphRouterDataSource<LiveEdge> Graph
        {
            get
            {
                return _graph;
            }
        }
    }
}