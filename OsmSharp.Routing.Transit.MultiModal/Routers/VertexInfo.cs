using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.Routers
{
    public class VertexInfo
    {
        public long VertexId { get; set; }

        public long FromVertexId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Weight { get; set; }
    }
}
