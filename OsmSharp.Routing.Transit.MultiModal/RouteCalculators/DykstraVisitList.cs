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
        private HashSet<long> _restricted;

        /// <summary>
        /// Holds the restricted visits.
        /// </summary>
        private Dictionary<long, HashSet<long>> _restrictedVisits;

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        public DykstraVisitList()
        {
            _visited = new HashSet<VertexTimeAndTrip>();
            _restricted = new HashSet<long>();
            _restrictedVisits = new Dictionary<long, HashSet<long>>();
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
            if (!_restricted.Contains(vertex.Vertex))
            { // not restricted.
                return _visited.Contains(vertex);
            }
            else
            { // check restricted.
                HashSet<long> froms;
                if (_restrictedVisits.TryGetValue(vertex.Vertex, out froms))
                {
                    return froms.Contains(fromVertex.Vertex);
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
            if (!_restricted.Contains(vertex.Vertex))
            { // not restricted.
                _visited.Add(vertex);
            }
            else
            { // check restricted.
                HashSet<long> froms;
                if (!_restrictedVisits.TryGetValue(vertex.Vertex, out froms))
                {
                    froms = new HashSet<long>();
                    _restrictedVisits.Add(vertex.Vertex, froms);
                }
                froms.Add(fromVertex.Vertex);
            }
        }

        /// <summary>
        /// Sets the given vertex as restricted.
        /// </summary>
        /// <param name="vertex"></param>
        public void SetRestricted(long vertex)
        {
            _restricted.Add(vertex);
        }
    }
}
