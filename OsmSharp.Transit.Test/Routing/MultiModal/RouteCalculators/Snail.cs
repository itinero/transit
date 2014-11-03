using OsmSharp.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Transit.Test.Routing.MultiModal.RouteCalculators
{
    /// <summary>
    /// An extremely slow vehicle profile.
    /// </summary>
    class Snail : Vehicle
    {

        protected override bool IsVehicleAllowed(Collections.Tags.TagsCollectionBase tags, string highwayType)
        {
            return true;
        }

        public override Units.Speed.KilometerPerHour MaxSpeed()
        {
            return 0.01;
        }

        public override Units.Speed.KilometerPerHour MaxSpeedAllowed(string highwayType)
        {
            return 0.01;
        }

        public override string UniqueName
        {
            get { return "A.Very.Slow.Snail"; }
        }
    }
}
