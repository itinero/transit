using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graph;
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
    public class MultiModalGraphRouterDataSource : DynamicGraphRouterDataSource<LiveEdge>
    {
        /// <summary>
        /// Holds the schedules.
        /// </summary>
        private List<TransitEdgeSchedulePair> _schedules;

        /// <summary>
        /// Creates a new multi modal graph router.
        /// </summary>
        /// <param name="index"></param>
        public MultiModalGraphRouterDataSource(ITagsCollectionIndexReadonly index)
            : base(index)
        {
            _schedules = new List<TransitEdgeSchedulePair>();
        }

        /// <summary>
        /// Creates a new multi modal graph router.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="index"></param>
        public MultiModalGraphRouterDataSource(IDynamicGraph<LiveEdge> graph, ITagsCollectionIndexReadonly index)
            : base(graph, index)
        {
            _schedules = new List<TransitEdgeSchedulePair>();
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
    }
}