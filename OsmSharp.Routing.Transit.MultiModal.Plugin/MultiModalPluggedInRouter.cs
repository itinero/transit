using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Service.Routing.Core;

namespace OsmSharp.Routing.Transit.MultiModal.Plugin
{
    /// <summary>
    /// A multimodal routing implementation.
    /// </summary>
    internal class MultiModalPluggedInRouter : IPluggedInRouter
    {
        /// <summary>
        /// Holds the router.
        /// </summary>
        private MultiModalRouter _router;

        /// <summary>
        /// Creates a pluggedin router.
        /// </summary>
        /// <param name="router"></param>
        public MultiModalPluggedInRouter(MultiModalRouter router)
        {
            _router = router;
        }

        /// <summary>
        /// Resolves a new location.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="location"></param>
        /// <param name="matcher"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, GeoCoordinate location, IEdgeMatcher matcher, TagsCollectionBase tags)
        {
            return _router.Resolve(vehicle, location, matcher, tags);
        }

        /// <summary>
        /// Checks connectivity.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="next"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool CheckConnectivity(Vehicle vehicle, RouterPoint next, float weight)
        {
            return _router.CheckConnectivity(vehicle, next, weight);
        }

        /// <summary>
        /// Calculates a route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public Route Calculate(Vehicle vehicle, RouterPoint previous, RouterPoint next)
        {
            return _router.Calculate(vehicle, previous, next);
        }

        /// <summary>
        /// Calculates a many to many matrix.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="routerPoint1"></param>
        /// <param name="routerPoint2"></param>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(Vehicle vehicle, RouterPoint[] routerPoint1, RouterPoint[] routerPoint2)
        {
            return _router.CalculateManyToManyWeight(vehicle, routerPoint1, routerPoint2);
        }

        /// <summary>
        /// Calculates a route to the closest point in the targets.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="first"></param>
        /// <param name="routerPoint"></param>
        /// <returns></returns>
        public Route CalculateToClosest(Vehicle vehicle, RouterPoint first, RouterPoint[] routerPoint)
        {
            return _router.CalculateToClosest(vehicle, first, routerPoint);
        }
    }
}