using GTFS;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Routers;
using OsmSharp.Routing.Transit.MultiModal.GTFS;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.MultiModal.Routers
{
    /// <summary>
    /// A type router that can handle liveedges that represent multimodal edges.
    /// </summary>
    public class TypedRouterMultiModal : TypedRouter<LiveEdge>
    {
        /// <summary>
        /// Holds the graph.
        /// </summary>
        private MultiModalGraphRouterDataSource _graph;

        /// <summary>
        /// Holds the stops.
        /// </summary>
        private Dictionary<string, uint> _stops;

        /// <summary>
        /// Holds the trips.
        /// </summary>
        private Dictionary<string, uint> _tripIds;

        /// <summary>
        /// Holds the router.
        /// </summary>
        private ReferenceCalculator _basicRouter;

        /// <summary>
        /// Creates a new type router using edges of type MultiModalEdge.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="router"></param>
        public TypedRouterMultiModal(MultiModalGraphRouterDataSource graph, IRoutingInterpreter interpreter, ReferenceCalculator router)
            :base(graph, interpreter, router)
        {
            _graph = graph;
            _basicRouter = router;
            _stops = new Dictionary<string, uint>();
            _tripIds = new Dictionary<string, uint>();
        }

        /// <summary>
        /// Adds a GTFS feed to this router.
        /// </summary>
        /// <param name="feed"></param>
        public void AddGTFSFeed(GTFSFeed feed)
        {
            GTFSGraphReader.AddToGraph(_graph, feed, _stops, _tripIds, _graph.Schedules);
        }

        /// <summary>
        /// Calculates a route between two given location that is possible multimodal.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="toFirstStop"></param>
        /// <param name="interModal"></param>
        /// <param name="fromLastStop"></param>
        /// <returns></returns>
        public Route CalculateTransit(DateTime departureTime, Vehicle toFirstStop, Vehicle interModal, Vehicle fromLastStop, RouterPoint from, RouterPoint to, Dictionary<string, object> parameters)
        {
            // calculate source/target.
            var source = this.RouteResolvedGraph(toFirstStop, from, false);
            var target = this.RouteResolvedGraph(fromLastStop, to, true);
            
            // make sure all parameters are set.
            var routingParameters = this.BuildRoutingParameters(parameters, departureTime);

            // calculate path.
            var path = _basicRouter.Calculate(_graph, this.Interpreter, interModal,
                source, target, double.MaxValue, routingParameters);

            return this.ConstructRoute(departureTime, new List<Vehicle>(new Vehicle[] { toFirstStop, interModal, fromLastStop }), path, from, to);
        }

        /// <summary>
        /// Calculates a transit route between two given stops leaving on or after the given departure time.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public Route CalculateTransit(DateTime departureTime, string from, string to)
        {
            return this.CalculateTransit(departureTime, from, to, null);
        }

        /// <summary>
        /// Calculates a transit route between two given stops leaving on or after the given departure time.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Route CalculateTransit(DateTime departureTime, string from, string to, Dictionary<string, object> parameters)
        {
            if (!_stops.ContainsKey(from)) { throw new ArgumentOutOfRangeException(string.Format("Stop {0} not found!", from)); }
            if (!_stops.ContainsKey(to)) { throw new ArgumentOutOfRangeException(string.Format("Stop {0} not found!", to)); }

            // build path segment visit list.
            var sourceVisitList = new PathSegmentVisitList();
            sourceVisitList.UpdateVertex(new PathSegment<long>(_stops[from]));
            var targetVisitList = new PathSegmentVisitList();
            targetVisitList.UpdateVertex(new PathSegment<long>(_stops[to]));

            // make sure all parameters are set.
            var routingParameters = this.BuildRoutingParameters(parameters, departureTime);

            // calculate path.
            var path = _basicRouter.Calculate(_graph, this.Interpreter, null,
                sourceVisitList, targetVisitList, double.MaxValue, routingParameters);

            // construct router points.
            float latitude, longitude;
            _graph.GetVertex(_stops[from], out latitude, out longitude);
            var source = new RouterPoint(_stops[from], new GeoCoordinate(latitude, longitude));
            _graph.GetVertex(_stops[to], out latitude, out longitude);
            var target = new RouterPoint(_stops[to], new GeoCoordinate(latitude, longitude));

            return this.ConstructRoute(departureTime, new List<Vehicle>(new Vehicle[] { Vehicle.Pedestrian }), path, source, target);
        }

        /// <summary>
        /// Adds all default pararameters.
        /// </summary>
        /// <param name="parameters"></param>
        private Dictionary<string, object> BuildRoutingParameters(Dictionary<string, object> parameters, DateTime departureTime)
        {
            if (parameters == null)
            { // make sure the parameters are at least empty.
                parameters = new Dictionary<string, object>();
            }
            var routingParameters = new Dictionary<string, object>(parameters);

            routingParameters[ReferenceCalculator.START_TIME_KEY] = departureTime;
            routingParameters[ReferenceCalculator.SCHEDULES_KEY] = _graph.Schedules;
            if (!routingParameters.ContainsKey(ReferenceCalculator.IS_TRIP_POSSIBLE_KEY))
            {
                Func<uint, DateTime, bool> isTripPossible = (x, y) => { return true; }; // TODO: make this actually check schedules!
                routingParameters[ReferenceCalculator.IS_TRIP_POSSIBLE_KEY] = isTripPossible;
            }
            if (!routingParameters.ContainsKey(ReferenceCalculator.MODAL_TRANSFER_TIME_KEY))
            {
                routingParameters[ReferenceCalculator.MODAL_TRANSFER_TIME_KEY] = (float)(5 * 60); // default 5 mins of tranfer time.
            }
            return routingParameters;
        }

        /// <summary>
        /// Returns true if the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override bool SupportsVehicle(Vehicle vehicle)
        {
            // TODO: ask interpreter.
            return true;
        }

        #region Route Construction

        /// <summary>
        /// Converts a linked route to a Route with metadata.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private Route ConstructRoute(DateTime departureTime, List<Vehicle> vehicles, PathSegment<long> route, RouterPoint source, RouterPoint target)
        {
            if (route != null)
            {
                long[] vertices = route.ToArray();

                // construct the actual graph route.
                return this.Generate(departureTime, vehicles, source, target, vertices);
            }
            return null; // calculation failed!
        }

        /// <summary>
        /// Generates an osm sharp route from a graph route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="fromResolved"></param>
        /// <param name="toResolved"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected virtual Route Generate(DateTime departureTime,
            List<Vehicle> vehicles, 
            RouterPoint fromResolved,
            RouterPoint toResolved,
            long[] vertices)
        {
            // create the route.
            Route route = null;

            if (vertices != null)
            {
                route = new Route();

                // set the vehicle.
                route.Vehicle = null;

                RoutePointEntry[] entries;
                if (vertices.Length > 0)
                {
                    entries = this.GenerateEntries(departureTime, vehicles, vertices);
                }
                else
                {
                    entries = new RoutePointEntry[0];
                }

                // create the from routing point.
                var from = new RoutePoint();
                //from.Name = from_point.Name;
                from.Latitude = (float)fromResolved.Location.Latitude;
                from.Longitude = (float)fromResolved.Location.Longitude;
                if (entries.Length > 0)
                {
                    entries[0].Points = new RoutePoint[1];
                    entries[0].Points[0] = from;
                    entries[0].Points[0].Tags = RouteTagsExtensions.ConvertFrom(fromResolved.Tags);
                }

                // create the to routing point.
                var to = new RoutePoint();
                //to.Name = to_point.Name;
                to.Latitude = (float)toResolved.Location.Latitude;
                to.Longitude = (float)toResolved.Location.Longitude;
                if (entries.Length > 0)
                {
                    //to.Tags = ConvertTo(to_point.Tags);
                    entries[entries.Length - 1].Points = new RoutePoint[1];
                    entries[entries.Length - 1].Points[0] = to;
                    entries[entries.Length - 1].Points[0].Tags = RouteTagsExtensions.ConvertFrom(toResolved.Tags);
                }

                // set the routing points.
                route.Entries = entries;

                //// calculate metrics.
                //var calculator = new TimeCalculator(_interpreter);
                //Dictionary<string, double> metrics = calculator.Calculate(route);
                //route.TotalDistance = metrics[TimeCalculator.DISTANCE_KEY];
                //route.TotalTime = metrics[TimeCalculator.TIME_KEY];
            }

            return route;
        }

        /// <summary>
        /// Generates a list of entries.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected virtual RoutePointEntry[] GenerateEntries(DateTime departureTime,
            List<Vehicle> vehicles, long[] vertices)
        {
            // create an entries list.
            var entries = new List<RoutePointEntry>();

            // create the first entry.
            var coordinate = this.GetCoordinate(vehicles[0], vertices[0]);
            var first = new RoutePointEntry();
            first.Latitude = (float)coordinate.Latitude;
            first.Longitude = (float)coordinate.Longitude;
            first.Type = RoutePointEntryType.Start;
            first.WayFromName = null;
            first.WayFromNames = null;

            entries.Add(first);

            // create all the other entries except the last one.
            var nodePrevious = vertices[0];
            for (int idx = 1; idx < vertices.Length - 1; idx++)
            {
                // get all the data needed to calculate the next route entry.
                var nodeCurrent = vertices[idx];
                var nodePreviousCoordinate = coordinate;
                var nodeNextCoordinate = this.GetCoordinate(vehicles[0], vertices[idx + 1]);
                long nodeNext = vertices[idx + 1];
                if (nodePrevious != nodeCurrent)
                {
                    var edge = this.GetEdgeData(vehicles[0], nodePrevious, nodeCurrent);

                    //// FIRST CALCULATE ALL THE ENTRY METRICS!

                    //// STEP1: Get the names.
                    //var currentTags = _dataGraph.TagsIndex.Get(edge.Tags);
                    //var name = _interpreter.EdgeInterpreter.GetName(currentTags);
                    //var names = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(currentTags);

                    // add intermediate entries.
                    if (edge.Coordinates != null)
                    { // loop over coordinates.
                        for (int coordinateIdx = 0; coordinateIdx < edge.Coordinates.Length; coordinateIdx++)
                        {
                            var entry = new RoutePointEntry();
                            entry.Latitude = edge.Coordinates[coordinateIdx].Latitude;
                            entry.Longitude = edge.Coordinates[coordinateIdx].Longitude;
                            entry.Type = RoutePointEntryType.Along;
                            //entry.Tags = currentTags.ConvertFrom();
                            //entry.WayFromName = name;
                            //entry.WayFromNames = names.ConvertFrom();

                            entries.Add(entry);
                        }
                    }

                    //// STEP2: Get the side streets
                    //var sideStreets = new List<RoutePointEntrySideStreet>();
                    //var neighbours = this.GetNeighboursUndirectedWithEdges(vehicle, nodeCurrent, nodePrevious, nodeNext);
                    //var consideredNeighbours = new HashSet<GeoCoordinate>();
                    //if (neighbours.Count > 0)
                    //{ // construct neighbours list.
                    //    foreach (var neighbour in neighbours)
                    //    {
                    //        var neighbourKeyCoordinate = this.GetCoordinate(vehicle, neighbour.Key);
                    //        if (neighbour.Value.Coordinates != null &&
                    //            neighbour.Value.Coordinates.Length > 0)
                    //        { // get the first of the coordinates array.
                    //            neighbourKeyCoordinate = new GeoCoordinate(
                    //                neighbour.Value.Coordinates[0].Latitude,
                    //                neighbour.Value.Coordinates[0].Longitude);
                    //        }
                    //        if (!consideredNeighbours.Contains(neighbourKeyCoordinate))
                    //        { // neighbour has not been considered yet.
                    //            consideredNeighbours.Add(neighbourKeyCoordinate);

                    //            var neighbourCoordinate = this.GetCoordinate(vehicle, neighbour.Key);
                    //            var tags = _dataGraph.TagsIndex.Get(neighbour.Value.Tags);

                    //            // build the side street info.
                    //            var sideStreet = new RoutePointEntrySideStreet();
                    //            sideStreet.Latitude = (float)neighbourCoordinate.Latitude;
                    //            sideStreet.Longitude = (float)neighbourCoordinate.Longitude;
                    //            sideStreet.Tags = tags.ConvertFrom();
                    //            sideStreet.WayName = _interpreter.EdgeInterpreter.GetName(tags);
                    //            sideStreet.WayNames = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(tags).ConvertFrom();

                    //            sideStreets.Add(sideStreet);
                    //        }
                    //    }
                    //}
                }

                // create the route entry.
                var nextCoordinate = this.GetCoordinate(vehicles[0], nodeCurrent);

                var routeEntry = new RoutePointEntry();
                routeEntry.Latitude = (float)nextCoordinate.Latitude;
                routeEntry.Longitude = (float)nextCoordinate.Longitude;
                //routeEntry.SideStreets = sideStreets.ToArray();
                //routeEntry.Tags = currentTags.ConvertFrom();
                routeEntry.Type = RoutePointEntryType.Along;
                //routeEntry.WayFromName = name;
                //routeEntry.WayFromNames = names.ConvertFrom();
                entries.Add(routeEntry);

                // set the previous node.
                nodePrevious = nodeCurrent;
            }

            // create the last entry.
            if (vertices.Length > 1)
            {
                int last_idx = vertices.Length - 1;
                var edge = this.GetEdgeData(vehicles[0], vertices[last_idx - 1], vertices[last_idx]);
                //TagsCollectionBase tags = _dataGraph.TagsIndex.Get(edge.Tags);

                //// get names.
                //var name = _interpreter.EdgeInterpreter.GetName(tags);
                //var names = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(tags).ConvertFrom();

                // add intermediate entries.
                if (edge.Coordinates != null)
                { // loop over coordinates.
                    for (int idx = 0; idx < edge.Coordinates.Length; idx++)
                    {
                        var entry = new RoutePointEntry();
                        entry.Latitude = edge.Coordinates[idx].Latitude;
                        entry.Longitude = edge.Coordinates[idx].Longitude;
                        entry.Type = RoutePointEntryType.Along;
                        //entry.Tags = tags.ConvertFrom();
                        //entry.WayFromName = name;
                        //entry.WayFromNames = names;

                        entries.Add(entry);
                    }
                }

                // add last entry.
                coordinate = this.GetCoordinate(vehicles[0], vertices[last_idx]);
                var last = new RoutePointEntry();
                last.Latitude = (float)coordinate.Latitude;
                last.Longitude = (float)coordinate.Longitude;
                last.Type = RoutePointEntryType.Stop;
                //last.Tags = tags.ConvertFrom();
                //last.WayFromName = name;
                //last.WayFromNames = names;

                entries.Add(last);
            }

            // return the result.
            return entries.ToArray();
        }


        #endregion
    }
}