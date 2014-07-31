using OsmSharp.Routing.ArcAggregation.Output;
using OsmSharp.Routing.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.Routers
{
    /// <summary>
    /// A multimodal arc aggregator with extra facilities for non-road edges.
    /// </summary>
    public class MultiModalArcAggregator : ArcAggregation.ArcAggregator
    {
        /// <summary>
        /// Creates a new multi modal arc aggregator.
        /// </summary>
        /// <param name="interpreter"></param>
        public MultiModalArcAggregator(IRoutingInterpreter interpreter)
            : base(interpreter)
        {

        }

        /// <summary>
        /// Returns true if the change between the two given arcs is significant.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="previousArc"></param>
        /// <param name="nextArc"></param>
        /// <returns></returns>
        protected override bool IsSignificant(Vehicle vehicle, AggregatedArc previousArc, AggregatedArc nextArc)
        {
            // check for transit edges and always consider edges without information important.
            if (previousArc.Tags != null && nextArc.Tags == null)
            {
                return true;
            }
            if (previousArc.Tags == null && nextArc.Tags != null)
            {
                return true;
            }
            string previousType, nextType;
            bool nextTypeFound = nextArc.Tags.TryGetValue("type", out nextType) && 
                (nextType == "intermodal" || nextType == "transit");
            bool previousTypeFound = previousArc.Tags.TryGetValue("type", out previousType) &&
                (previousType == "intermodal" || previousType == "transit");
            if (previousTypeFound && nextTypeFound)
            { // both modal types.
                // always return true, all intermediate stops still need to be in the resulting aggregation.
                return true;
            }
            else if(previousTypeFound && !nextTypeFound)
            { // intermodal difference.
                return true;
            }
            else if (!previousTypeFound && nextTypeFound)
            { // intermodal difference.
                return true;
            }
            
            return base.IsSignificant(vehicle, previousArc, nextArc);
        }
    }
}
