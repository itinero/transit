using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Transit.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.RouteCalculators
{
    public class DykstraVisitList
    {
        /// <summary>
        /// Holds the set of visited vertices.
        /// </summary>
        private HashSet<VertexTimeAndTrip> _visited;

        /// <summary>
        /// Holds the set of restricted vertices.
        /// </summary>
        private HashSet<VertexTimeAndTrip> _restricted;

        /// <summary>
        /// Holds the restricted visits.
        /// </summary>
        private Dictionary<VertexTimeAndTrip, HashSet<VertexTimeAndTrip>> _restrictedVisits;

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        public DykstraVisitList()
        {
            _visited = new HashSet<VertexTimeAndTrip>();
            _restricted = new HashSet<VertexTimeAndTrip>();
            _restrictedVisits = new Dictionary<VertexTimeAndTrip, HashSet<VertexTimeAndTrip>>();
        }

        /// <summary>
        /// Returns true if the given vertex has been visited already.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool HasBeenVisited(PathSegment<VertexTimeAndTrip> vertex)
        {
            if (vertex.From != null)
            { // there is a previous vertex, check it.
                return this.HasBeenVisited(vertex.VertexId, vertex.From.VertexId);
            }
            return _visited.Contains(vertex.VertexId);
        }

        /// <summary>
        /// Returns true if the given vertex has been visited already.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="fromVertex"></param>
        /// <returns></returns>
        public bool HasBeenVisited(VertexTimeAndTrip vertex, VertexTimeAndTrip fromVertex)
        {
            if (!_restricted.Contains(vertex))
            { // not restricted.
                return _visited.Contains(vertex);
            }
            else
            { // check restricted.
                HashSet<VertexTimeAndTrip> froms;
                if (_restrictedVisits.TryGetValue(vertex, out froms))
                {
                    return froms.Contains(fromVertex);
                }
                return false;
            }
        }


        /// <summary>
        /// Sets the vertex as visited coming from the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public void SetVisited(PathSegment<VertexTimeAndTrip> vertex)
        {
            if (vertex.From != null)
            { // there is a previous vertex, check it.
                this.SetVisited(vertex.VertexId, vertex.From.VertexId);
            }
            else
            {
                _visited.Add(vertex.VertexId);
            }
        }

        /// <summary>
        /// Sets the vertex as visited coming from the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="fromVertex"></param>
        public void SetVisited(VertexTimeAndTrip vertex, VertexTimeAndTrip fromVertex)
        {
            if (!_restricted.Contains(vertex))
            { // not restricted.
                _visited.Add(vertex);
            }
            else
            { // check restricted.
                HashSet<VertexTimeAndTrip> froms;
                if (!_restrictedVisits.TryGetValue(vertex, out froms))
                {
                    froms = new HashSet<VertexTimeAndTrip>();
                    _restrictedVisits.Add(vertex, froms);
                }
                froms.Add(fromVertex);
            }
        }

        /// <summary>
        /// Sets the given vertex as restricted.
        /// </summary>
        /// <param name="vertex"></param>
        public void SetRestricted(VertexTimeAndTrip vertex)
        {
            _restricted.Add(vertex);
            _visited.Remove(vertex);
        }
    }
}
