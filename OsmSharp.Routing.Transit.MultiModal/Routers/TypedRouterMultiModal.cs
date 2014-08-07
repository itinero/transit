using GeoAPI.Geometries;
using GTFS;
using GTFS.Entities;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.ArcAggregation;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Router;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Routers;
using OsmSharp.Routing.Transit.Graphs;
using OsmSharp.Routing.Transit.MultiModal.GTFS;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private MultiModalGraphRouterDataSource _source;

        /// <summary>
        /// Holds the stops.
        /// </summary>
        private Dictionary<string, uint> _stopIds;

        /// <summary>
        /// Holds the stop-index.
        /// </summary>
        private Dictionary<string, Stop> _stops;

        /// <summary>
        /// Holds the stop per vertex.
        /// </summary>
        private Dictionary<uint, string> _reverseStops;

        /// <summary>
        /// Holds the trips.
        /// </summary>
        private Dictionary<string, uint> _tripIds;

        /// <summary>
        /// Holds the router.
        /// </summary>
        private ReferenceCalculator _basicRouter;

        /// <summary>
        /// Holds the interpreter.
        /// </summary>
        private IRoutingInterpreter _interpreter;

        /// <summary>
        /// Creates a new type router using edges of type MultiModalEdge.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interpreter"></param>
        /// <param name="router"></param>
        public TypedRouterMultiModal(MultiModalGraphRouterDataSource source, IRoutingInterpreter interpreter, ReferenceCalculator router)
            : base(source.Graph, interpreter, router)
        {
            _source = source;
            _basicRouter = router;
            _interpreter = interpreter;
            _stopIds = new Dictionary<string, uint>();
            _reverseStops = new Dictionary<uint, string>();
            _tripIds = new Dictionary<string, uint>();
            _stops = new Dictionary<string, Stop>();
        }

        #region GFTS

        /// <summary>
        /// Holds all GTFS feeds.
        /// </summary>
        private List<GTFSFeed> _feeds = new List<GTFSFeed>();

        /// <summary>
        /// Adds a new GTFS feed to this router.
        /// </summary>
        /// <param name="feed"></param>
        public void AddGTFSFeed(GTFSFeed feed)
        {
            _feeds.Add(feed);

            GTFSGraphReader.AddToGraph(_source.Graph, feed, _stopIds, _tripIds, _source.Schedules);

            this.BuildReverseStops();
            this.BuildStopIndex();
        }

        /// <summary>
        /// Builds the reverse stop index.
        /// </summary>
        private void BuildReverseStops()
        {
            _reverseStops.Clear();
            foreach(var stop in _stopIds)
            {
                _reverseStops[stop.Value] = stop.Key;
            }
        }

        /// <summary>
        /// Builds the stop index.
        /// </summary>
        private void BuildStopIndex()
        {
            _stops.Clear();
            foreach (var feed in _feeds)
            {
                foreach (var stop in feed.Stops)
                {
                    _stops[stop.Id] = stop;
                }
            }
        }

        #endregion

        #region Queries

        /// <summary>
        /// Returns the agency with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Agency GetAgency(string id)
        {
            foreach (var feed in _feeds)
            {
                var agency = feed.GetAgency(id);
                if (agency != null)
                {
                    return agency;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all agencies.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies()
        {
            if (_feeds.Count > 0)
            {
                IEnumerable<Agency> agencies = _feeds[0].Agencies;
                for (int idx = 1; idx < _feeds.Count; idx++)
                {
                    agencies = agencies.Concat(_feeds[1].Agencies);
                }
                return agencies;
            }
            return new List<Agency>();
        }

        /// <summary>
        /// Returns the agencies that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Agency> GetAgencies(string query)
        {
            return this.GetAgencies().Where(x => { return x.Name != null && x.Name.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) != -1; });
        }

        /// <summary>
        /// Returns all the stops.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops()
        {
            if (_feeds.Count > 0)
            {
                IEnumerable<Stop> stops = _feeds[0].Stops;
                for (int idx = 1; idx < _feeds.Count; idx++)
                {
                    stops = stops.Concat(_feeds[1].Stops);
                }
                return stops;
            }
            return new List<Stop>();
        }

        /// <summary>
        /// Returns the stops.
        /// </summary>
        /// <returns></returns>
        public Stop GetStop(string stopId)
        {
            Stop stop;
            if(_stops.TryGetValue(stopId, out stop))
            {
                return stop;
            }
            return null;
        }

        /// <summary>
        /// Returns the agency for the given stop.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        public Agency GetAgencyForStop(string stopId)
        {
            return null;
            //IEnumerable<Agency> agencies = new List<Agency>();
            //foreach(var feed in _feeds)
            //{
            //    agencies = agencies.Concat(from a in feed.Agencies
            //               join r in feed.Routes on a.Id equals r.AgencyId
            //               join t in feed.Trips on r.Id equals t.RouteId
            //               join st in feed.StopTimes on t.Id equals st.TripId
            //               join s in feed.Stops on st.StopId equals s.Id
            //               where s.Id == stopId
            //               select a);
            //}
            //return agencies.FirstOrDefault();
        }

        /// <summary>
        /// Returns all the stops that contain the words in the given query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStops(string query)
        {
            return this.GetStops().Where(x => { return x.Name != null && x.Name.IndexOf(query, StringComparison.InvariantCultureIgnoreCase) != -1; });
        }

        /// <summary>
        /// Returns all stops for the given agency.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId)
        {
            return new List<Stop>();
        }

        /// <summary>
        /// Returns all stops for the given agency that contain the words in the given query.
        /// </summary>
        /// <param name="agencyId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<Stop> GetStopsForAgency(string agencyId, string query)
        {
            return new List<Stop>();
        }

        /// <summary>
        /// Returns a trip for the given trip id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Trip GetTrip(string id)
        {
            foreach (var feed in _feeds)
            {
                var item = feed.GetTrip(id);
                if (item != null)
                {
                    return item;
                }
            }
            return null;
        }

        #endregion

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
            var path = _basicRouter.CalculateAndTime(_source.Graph, this.Interpreter, interModal,
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
            if (!_stopIds.ContainsKey(from)) { throw new ArgumentOutOfRangeException(string.Format("Stop {0} not found!", from)); }
            if (!_stopIds.ContainsKey(to)) { throw new ArgumentOutOfRangeException(string.Format("Stop {0} not found!", to)); }

            // build path segment visit list.
            var sourceVisitList = new PathSegmentVisitList();
            sourceVisitList.UpdateVertex(new PathSegment<long>(_stopIds[from]));
            var targetVisitList = new PathSegmentVisitList();
            targetVisitList.UpdateVertex(new PathSegment<long>(_stopIds[to]));

            // make sure all parameters are set.
            var routingParameters = this.BuildRoutingParameters(parameters, departureTime);

            // calculate path.
            var path = _basicRouter.CalculateAndTime(_source.Graph, this.Interpreter, null,
                sourceVisitList, targetVisitList, double.MaxValue, routingParameters);

            // construct router points.
            float latitude, longitude;
            _source.Graph.GetVertex(_stopIds[from], out latitude, out longitude);
            var source = new RouterPoint(_stopIds[from], new GeoCoordinate(latitude, longitude));
            _source.Graph.GetVertex(_stopIds[to], out latitude, out longitude);
            var target = new RouterPoint(_stopIds[to], new GeoCoordinate(latitude, longitude));

            return this.ConstructRoute(departureTime, new List<Vehicle>(new Vehicle[] { Vehicle.Pedestrian }), path, source, target);
        }

        /// <summary>
        /// Calculates the weight to all vertices that are within a certain range from the source.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="toFirstStop"></param>
        /// <param name="interModal"></param>
        /// <param name="fromLastStop"></param>
        /// <param name="from"></param>
        /// <param name="parameters"></param>
        /// <param name="maxWeight"></param>
        /// <param name="sampleZoom"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<GeoCoordinate, ulong, double>> CalculateAllWithin(DateTime departureTime, Vehicle toFirstStop, Vehicle interModal, Vehicle fromLastStop, RouterPoint from, Dictionary<string, object> parameters, double maxWeight, int sampleZoom)
        {
            // calculate source/target.
            var source = this.RouteResolvedGraph(toFirstStop, from, false);

            // make sure all parameters are set.
            var routingParameters = this.BuildRoutingParameters(parameters, departureTime);

            // calculate path.
            var tiledSamples = new TiledWeights(sampleZoom);
            _basicRouter.CalculateRange(_source.Graph, this.Interpreter, interModal,
                source, maxWeight, true, routingParameters, (vertexTimeAndTrip) =>
                {
                    var coordinate = this.GetCoordinate(toFirstStop, vertexTimeAndTrip.VertexId.Vertex);
                    tiledSamples.AddSample(coordinate.Latitude, coordinate.Longitude, vertexTimeAndTrip.Weight);
                });

            // extract coordinates of all withing.
            var allWithinCoordinates = new List<Tuple<GeoCoordinate, ulong, double>>(tiledSamples.Count);
            foreach(var sampleTuple in tiledSamples)
            {
                var center = sampleTuple.Item1.Box.Center;
                allWithinCoordinates.Add(new Tuple<GeoCoordinate,ulong,double>(new GeoCoordinate(center.Latitude, center.Longitude), sampleTuple.Item1.Id, sampleTuple.Item2));
            }
            return allWithinCoordinates;
        }

        /// <summary>
        /// Adds all default pararameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="departureTime"></param>
        private Dictionary<string, object> BuildRoutingParameters(Dictionary<string, object> parameters, DateTime departureTime)
        {
            if (parameters == null)
            { // make sure the parameters are at least empty.
                parameters = new Dictionary<string, object>();
            }
            var routingParameters = new Dictionary<string, object>(parameters);

            routingParameters[ReferenceCalculator.START_TIME_KEY] = departureTime;
            routingParameters[ReferenceCalculator.SCHEDULES_KEY] = _source.Schedules;
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
        /// <param name="departureTime"></param>
        /// <param name="vehicles"></param>
        /// <param name="route"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private Route ConstructRoute(DateTime departureTime, List<Vehicle> vehicles, PathSegment<VertexTimeAndTrip> route, RouterPoint source, RouterPoint target)
        {
            if (route != null)
            {
                // get all vertices from the path.
                var vertices = route.ToArrayWithWeight();

                // construct the actual graph route.
                return this.Generate(departureTime, vehicles, source, target, vertices);
            }
            return null; // calculation failed!
        }

        /// <summary>
        /// Generates an osm sharp route from a graph route.
        /// </summary>
        /// <param name="departureTime"></param>
        /// <param name="vehicles"></param>
        /// <param name="fromResolved"></param>
        /// <param name="toResolved"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected virtual Route Generate(DateTime departureTime,
            List<Vehicle> vehicles, 
            RouterPoint fromResolved,
            RouterPoint toResolved,
            Tuple<VertexTimeAndTrip, double>[] vertices)
        {
            // create the route.
            Route route = null;

            if (vertices != null)
            {
                route = new Route();

                // set the vehicle.
                route.Vehicle = vehicles[0];

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
        /// <param name="departureTime"></param>
        /// <param name="vehicles"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        protected virtual RoutePointEntry[] GenerateEntries(DateTime departureTime,
            List<Vehicle> vehicles, Tuple<VertexTimeAndTrip, double>[] vertices)
        {
            // create an entries list.
            var entries = new List<RoutePointEntry>();

            // create the first entry.
            var coordinate = this.GetCoordinate(vehicles[0], vertices[0].Item1.Vertex);
            var first = new RoutePointEntry();
            first.Latitude = (float)coordinate.Latitude;
            first.Longitude = (float)coordinate.Longitude;
            first.Type = RoutePointEntryType.Start;
            first.WayFromName = null;
            first.WayFromNames = null;

            entries.Add(first);

            // create all the other entries except the last one.
            var nodePrevious = vertices[0];
            var perviousIsTransit = false;
            for (int idx = 1; idx < vertices.Length - 1; idx++)
            {
                // get all the data needed to calculate the next route entry.
                var nodeCurrent = vertices[idx];
                var nodePreviousCoordinate = coordinate;
                var nodeNextCoordinate = this.GetCoordinate(vehicles[0], vertices[idx + 1].Item1.Vertex);
                var nodeNext = vertices[idx + 1];
                if (nodePrevious.Item1.Vertex != nodeCurrent.Item1.Vertex)
                {
                    var edge = this.GetEdgeData(vehicles[0], nodePrevious.Item1.Vertex, nodeCurrent.Item1.Vertex);

                    if(edge is LiveEdge)
                    { // regular road.
                        var liveEdge = (LiveEdge)edge;
                        if(liveEdge.IsRoad())
                        { // edge is a road.
                            var currentTags = _source.Graph.TagsIndex.Get(edge.Tags);
                            var name = _interpreter.EdgeInterpreter.GetName(currentTags);
                            var names = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(currentTags);

                            // add intermediate entries.
                            if (edge.Coordinates != null)
                            { // loop over coordinates.
                                for (int coordinateIdx = 0; coordinateIdx < edge.Coordinates.Length; coordinateIdx++)
                                {
                                    var entry = new RoutePointEntry();
                                    entry.Latitude = edge.Coordinates[coordinateIdx].Latitude;
                                    entry.Longitude = edge.Coordinates[coordinateIdx].Longitude;
                                    entry.Type = RoutePointEntryType.Along;
                                    entry.Tags = currentTags.ConvertFrom();
                                    entry.WayFromName = name;
                                    entry.WayFromNames = names.ConvertFrom();

                                    entries.Add(entry);
                                }

                                // Get the side streets
                                var sideStreets = new List<RoutePointEntrySideStreet>();
                                var neighbours = this.GetNeighboursUndirectedWithEdges(vehicles[0], nodeCurrent.Item1.Vertex, nodePrevious.Item1.Vertex, nodeNext.Item1.Vertex);
                                var consideredNeighbours = new HashSet<GeoCoordinate>();
                                if (neighbours.Count > 0)
                                { // construct neighbours list.
                                    foreach (var neighbour in neighbours)
                                    {
                                        if (((LiveEdge)neighbour.Value).IsRoad())
                                        { // neighour edge is a road.
                                            var neighbourKeyCoordinate = this.GetCoordinate(vehicles[0], neighbour.Key);
                                            if (neighbour.Value.Coordinates != null &&
                                                neighbour.Value.Coordinates.Length > 0)
                                            { // get the first of the coordinates array.
                                                neighbourKeyCoordinate = new GeoCoordinate(
                                                    neighbour.Value.Coordinates[0].Latitude,
                                                    neighbour.Value.Coordinates[0].Longitude);
                                            }
                                            if (!consideredNeighbours.Contains(neighbourKeyCoordinate))
                                            { // neighbour has not been considered yet.
                                                consideredNeighbours.Add(neighbourKeyCoordinate);

                                                var neighbourCoordinate = this.GetCoordinate(vehicles[0], neighbour.Key);
                                                var tags = _source.Graph.TagsIndex.Get(neighbour.Value.Tags);

                                                // build the side street info.
                                                var sideStreet = new RoutePointEntrySideStreet();
                                                sideStreet.Latitude = (float)neighbourCoordinate.Latitude;
                                                sideStreet.Longitude = (float)neighbourCoordinate.Longitude;
                                                sideStreet.Tags = tags.ConvertFrom();
                                                sideStreet.WayName = _interpreter.EdgeInterpreter.GetName(tags);
                                                sideStreet.WayNames = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(tags).ConvertFrom();

                                                sideStreets.Add(sideStreet);
                                            }
                                        }
                                    }
                                }

                                // create the route entry.
                                var nextCoordinate = this.GetCoordinate(vehicles[0], nodeCurrent.Item1.Vertex);

                                var routeEntry = new RoutePointEntry();
                                routeEntry.Latitude = (float)nextCoordinate.Latitude;
                                routeEntry.Longitude = (float)nextCoordinate.Longitude;
                                routeEntry.SideStreets = sideStreets.ToArray();
                                routeEntry.Tags = currentTags.ConvertFrom();
                                routeEntry.Type = RoutePointEntryType.Along;
                                routeEntry.WayFromName = name;
                                routeEntry.WayFromNames = names.ConvertFrom();
                                entries.Add(routeEntry);
                            }
                            perviousIsTransit = false;
                        }
                        else if(liveEdge.IsTransit())
                        { // edge is a transit edge.
                            // add previous station as poi.
                            string stopId;
                            if (entries.Count > 0 && !perviousIsTransit && _reverseStops.TryGetValue((uint)nodePrevious.Item1.Vertex, out stopId))
                            { // a first entry in a transit series. add previous station.     
                                double stopLatitude, stopLongitude;
                                var stopTags = this.GetStopDetails(stopId, out stopLatitude, out stopLongitude);
                                if(stopTags != null)
                                { // there are stop-details.
                                    var routePoints = new List<RoutePoint>();
                                    if (entries[entries.Count - 1].Points != null)
                                    {
                                        routePoints.AddRange(entries[entries.Count - 1].Points);
                                    }
                                    var agency = this.GetAgencyForStop(stopId);
                                    var stopPoint = new RoutePoint();
                                    stopPoint.Latitude = (float)stopLatitude;
                                    stopPoint.Longitude = (float)stopLongitude;
                                    stopPoint.Tags = stopTags;
                                    routePoints.Add(stopPoint);
                                    entries[entries.Count - 1].Points = routePoints.ToArray();
                                }
                            }

                            // get edge details and create tags.
                            var forwardSchedule = liveEdge.GetForwardSchedule(_source.Schedules);
                            var forwardScheduleEntry = forwardSchedule == null ? 
                                null : forwardSchedule.GetForTrip(nodePrevious.Item1.Trip, 
                                    departureTime.AddSeconds(nodePrevious.Item2));
                            RouteTags[] tags = null;
                            if(forwardScheduleEntry != null)
                            { // there is a schedule entry.
                                tags = new RouteTags[6];
                                tags[0] = new RouteTags();
                                tags[0].Key = "type";
                                tags[0].Value = "transit";
                                tags[1] = new RouteTags();
                                tags[1].Key = "trip_id";
                                tags[1].Value = forwardScheduleEntry.Value.Trip.ToInvariantString();
                                tags[2] = new RouteTags();
                                tags[2].Key = "description";
                                tags[2].Value = forwardScheduleEntry.Value.ToInvariantString();

                                tags[3] = new RouteTags();
                                tags[3].Key = "trip";
                                tags[3].Value = "Unknown";
                                string tripId = _tripIds.First(x => { return x.Value == forwardScheduleEntry.Value.Trip; }).Key;
                                if(!string.IsNullOrWhiteSpace(tripId))
                                {
                                    var trip = this.GetTrip(tripId);
                                    if(trip != null)
                                    {
                                        tags[3].Value = trip.Headsign;
                                    }
                                }
                                tags[4] = new RouteTags();
                                tags[4].Key = "departure_time";
                                tags[4].Value = departureTime.Date.AddSeconds(forwardScheduleEntry.Value.DepartureTime).ToInvariantString();
                                tags[5] = new RouteTags();
                                tags[5].Key = "arrival_time";
                                tags[5].Value = departureTime.Date.AddSeconds(forwardScheduleEntry.Value.ArrivalTime).ToInvariantString();
                            }
                            else
                            { // there is no schedule entry.
                                tags = new RouteTags[1];
                                tags[0] = new RouteTags();
                                tags[0].Key = "type";
                                tags[0].Value = "transit";
                            }

                            // add intermediate entries.
                            if (edge.Coordinates != null)
                            { // loop over coordinates.
                                for (int coordinateIdx = 0; coordinateIdx < edge.Coordinates.Length; coordinateIdx++)
                                {
                                    var entry = new RoutePointEntry();
                                    entry.Latitude = edge.Coordinates[coordinateIdx].Latitude;
                                    entry.Longitude = edge.Coordinates[coordinateIdx].Longitude;
                                    entry.Type = RoutePointEntryType.Along;
                                    entry.Tags = tags;
                                    entries.Add(entry);
                                }
                            }

                            // create the route entry.
                            var nextCoordinate = this.GetCoordinate(vehicles[0], nodeCurrent.Item1.Vertex);

                            var routeEntry = new RoutePointEntry();
                            routeEntry.Latitude = (float)nextCoordinate.Latitude;
                            routeEntry.Longitude = (float)nextCoordinate.Longitude;
                            routeEntry.SideStreets = new RoutePointEntrySideStreet[0];
                            routeEntry.Tags = tags;
                            routeEntry.Type = RoutePointEntryType.Along;
                            routeEntry.WayFromName = string.Empty;
                            routeEntry.WayFromNames = new RouteTags[0];
                            if (_reverseStops.TryGetValue((uint)nodeCurrent.Item1.Vertex, out stopId))
                            { // a first entry in a transit series. add previous station.
                                double stopLatitude, stopLongitude;
                                var stopTags = this.GetStopDetails(stopId, out stopLatitude, out stopLongitude);
                                if (stopTags != null)
                                { // there are stop-details.
                                    var routePoints = new List<RoutePoint>();
                                    if (entries[entries.Count - 1].Points != null)
                                    {
                                        routePoints.AddRange(entries[entries.Count - 1].Points);
                                    }
                                    var agency = this.GetAgencyForStop(stopId);
                                    var stopPoint = new RoutePoint();
                                    stopPoint.Latitude = (float)stopLatitude;
                                    stopPoint.Longitude = (float)stopLongitude;
                                    stopPoint.Tags = stopTags;
                                    routePoints.Add(stopPoint);
                                    entries[entries.Count - 1].Points = routePoints.ToArray();
                                }
                            }

                            entries.Add(routeEntry);
                            perviousIsTransit = true;
                        }
                        else
                        { // edge is something else.
                            // add intermediate entries.
                            if (edge.Coordinates != null)
                            { // loop over coordinates.
                                for (int coordinateIdx = 0; coordinateIdx < edge.Coordinates.Length; coordinateIdx++)
                                {
                                    var entry = new RoutePointEntry();
                                    entry.Latitude = edge.Coordinates[coordinateIdx].Latitude;
                                    entry.Longitude = edge.Coordinates[coordinateIdx].Longitude;
                                    entry.Type = RoutePointEntryType.Along;
                                    entry.Tags = new RouteTags[1];
                                    entry.Tags[0] = new RouteTags();
                                    entry.Tags[0].Key = "type";
                                    entry.Tags[0].Value = "intermodal";

                                    entries.Add(entry);
                                }
                            }

                            // create the route entry.
                            var nextCoordinate = this.GetCoordinate(vehicles[0], nodeCurrent.Item1.Vertex);

                            var routeEntry = new RoutePointEntry();
                            routeEntry.Latitude = (float)nextCoordinate.Latitude;
                            routeEntry.Longitude = (float)nextCoordinate.Longitude;
                            routeEntry.SideStreets = new RoutePointEntrySideStreet[0];
                            routeEntry.Tags = new RouteTags[1];
                            routeEntry.Tags[0] = new RouteTags();
                            routeEntry.Tags[0].Key = "type";
                            routeEntry.Tags[0].Value = "intermodal";
                            routeEntry.Type = RoutePointEntryType.Along;
                            routeEntry.WayFromName = string.Empty;
                            routeEntry.WayFromNames = new RouteTags[0];
                            entries.Add(routeEntry);
                            perviousIsTransit = false;
                        }
                    }
                }

                // set the previous node.
                nodePrevious = nodeCurrent;
            }

            // create the last entry.
            if (vertices.Length > 1)
            {
                int last_idx = vertices.Length - 1;
                var edge = this.GetEdgeData(vehicles[0], vertices[last_idx - 1].Item1.Vertex, vertices[last_idx].Item1.Vertex);
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
                coordinate = this.GetCoordinate(vehicles[0], vertices[last_idx].Item1.Vertex);
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

        /// <summary>
        /// Creates an array of routetags for the given stopId.
        /// </summary>
        /// <param name="stopId"></param>
        /// <returns></returns>
        private RouteTags[] GetStopDetails(string stopId, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;
            var stop = this.GetStop(stopId);
            var stopName = string.Empty;
            var stopDescription = string.Empty;
            var stopAgencyName = string.Empty;
            if (stop != null)
            { // a stop was found.
                stopName = stop.Name;
                stopDescription = stop.Description;
                latitude = stop.Latitude;
                longitude = stop.Longitude;
            }
            var agency = this.GetAgencyForStop(stopId);
            var tags = new RouteTags[3];
            tags[0] = new RouteTags();
            tags[0].Key = "name";
            tags[0].Value = stopName;
            tags[1] = new RouteTags();
            tags[1].Key = "description";
            tags[1].Value = stopDescription;
            tags[2] = new RouteTags();
            tags[2].Key = "agency";
            tags[2].Value = "NOT FOUND";
            if (agency != null)
            {
                tags[2].Value = agency.Name;
            }
            else if(stop.Tag != null && stop.Tag is string)
            {
                tags[2].Value = stop.Tag as string;
            }
            return tags;
        }


        #endregion

        public FeatureCollection GetNetworkFeatures()
        {
            return this.GetNetworkFeatures(new GeoCoordinateBox(new GeoCoordinate(-1000, -1000), new GeoCoordinate(1000, 1000)));
        }

        public FeatureCollection GetNetworkFeatures(GeoCoordinateBox box)
        {
            var features = new FeatureCollection();
            var vertexFeatures = new FeatureCollection();

            // add all stops.
            var handledVertices = new HashSet<uint>();
            float latitude, longitude;
            foreach(var stop in _stopIds)
            {
                var attributes = new AttributesTable();
                attributes.AddAttribute("stop_id", stop.Key);
                attributes.AddAttribute("vertex_id", stop.Value);
                _source.Graph.GetVertex(stop.Value, out latitude, out longitude);
                if(box.Contains(new GeoCoordinate(latitude, longitude)))
                {
                    var point = new Point(new Coordinate(longitude, latitude));
                    vertexFeatures.Add(new Feature(point, attributes));

                    handledVertices.Add(stop.Value);
                }
            }

            // add the rest of the vertices and associated arcs.
            for(uint vertex = 1; vertex <= _source.Graph.VertexCount; vertex++)
            {
                _source.Graph.GetVertex(vertex, out latitude, out longitude);
                if(!box.Contains(new GeoCoordinate(latitude, longitude)))
                {
                    continue;
                }
                if(!handledVertices.Contains(vertex))
                {
                    var attributes = new AttributesTable();
                    attributes.AddAttribute("vertex_id", vertex);

                    var point = new Point(new Coordinate(longitude, latitude));
                    vertexFeatures.Add(new Feature(point, attributes));
                }

                var arcs = _source.Graph.GetArcs(vertex);
                foreach(var arc in arcs)
                {
                    if (arc.Value.Forward)
                    {
                        var coordinates = new List<Coordinate>();
                        _source.Graph.GetVertex(vertex, out latitude, out longitude);
                        coordinates.Add(new Coordinate(longitude, latitude));

                        if (arc.Value.Coordinates != null)
                        {
                            for (int idx = 0; idx < arc.Value.Coordinates.Length; idx++)
                            {
                                coordinates.Add(new Coordinate(arc.Value.Coordinates[idx].Longitude, arc.Value.Coordinates[idx].Latitude));
                            }
                        }
                        _source.Graph.GetVertex(arc.Key, out latitude, out longitude);
                        coordinates.Add(new Coordinate(longitude, latitude));

                        var attributes = new AttributesTable();
                        if (arc.Value.IsRoad())
                        {
                            var tags = _source.Graph.TagsIndex.Get(arc.Value.Tags);
                            if (tags != null)
                            {
                                foreach (var tag in tags)
                                {
                                    attributes.AddAttribute(tag.Key, tag.Value);
                                }
                            }
                        }
                        else if(arc.Value.IsTransit())
                        {
                            uint? scheduleId = arc.Value.GetScheduleId();
                            attributes.AddAttribute("type", "transit");
                            attributes.AddAttribute("schedule_id", scheduleId.ToInvariantString());
                            var forwardSchedule = arc.Value.GetForwardSchedule(_source.Schedules);
                            for(int idx = 0; idx < forwardSchedule.Entries.Count; idx++)
                            {
                                var entry = forwardSchedule.Entries[idx];
                                attributes.AddAttribute("forward_schedule_" + idx.ToString("D4"), entry.ToInvariantString());
                            }
                            var backwardSchedule = arc.Value.GetBackwardSchedule(_source.Schedules);
                            for (int idx = 0; idx < backwardSchedule.Entries.Count; idx++)
                            {
                                var entry = backwardSchedule.Entries[idx];
                                attributes.AddAttribute("backward_schedule_" + idx.ToString("D4"), entry.ToInvariantString());
                            }
                        }
                        else
                        {
                            attributes.AddAttribute("type", "intermodal");
                        }
                        attributes.AddAttribute("from_vertex", vertex);
                        attributes.AddAttribute("to_vertex", arc.Key);
                        var lineString = new LineString(coordinates.ToArray());
                        features.Add(new Feature(lineString, attributes));
                    }
                }
            }

            // add points at the end.
            foreach(var vertexFeature in vertexFeatures.Features)
            {
                features.Add(vertexFeature);
            }

            return features;
        }

        /// <summary>
        /// Converts the given route to a line string.
        /// </summary>
        /// <param name="route">The route to convert.</param>
        /// <param name="aggregated">Aggregate similar edge together.</param>
        /// <returns></returns>
        public FeatureCollection GetFeatures(Route route, bool aggregated)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            var featureCollection = new FeatureCollection();
            var pointsCollection = new FeatureCollection();
            if (!aggregated)
            { // do not aggregate, just add geometry.
                var coordinates = route.GetPoints();
                var ntsCoordinates = coordinates.Select(x => { return new Coordinate(x.Longitude, x.Latitude); });
                var geometryFactory = new GeometryFactory();
                var lineString = geometryFactory.CreateLineString(ntsCoordinates.ToArray());
                var feature = new Feature(lineString, new AttributesTable());
                featureCollection.Add(feature);
            }
            else
            { // aggregate similar edges.
                if (route.Vehicle == null) { throw new InvalidOperationException("Route does not have a vehicle."); }

                var aggregator = new MultiModalArcAggregator(_interpreter);
                var aggregatedRoute = aggregator.Aggregate(route);

                var current = aggregatedRoute;
                while (current != null)
                {
                    var currentArc = current.Next;
                    if (currentArc != null)
                    { // there is an arc.
                        var next = currentArc.Next;
                        if(next != null)
                        { // there is a next point, now there can be a segment extracted from the route.
                            // build geometry.
                            var coordinates = new List<Coordinate>();
                            for (int idx = current.EntryIdx; idx < next.EntryIdx + 1; idx++)
                            {
                                coordinates.Add(new Coordinate(route.Entries[idx].Longitude, route.Entries[idx].Latitude));
                            }

                            // build attributes.
                            var currentArcTags = currentArc.Tags;
                            var attributesTable = new AttributesTable();
                            if (currentArcTags != null)
                            { // there are tags.
                                foreach (var tag in currentArcTags)
                                {
                                    attributesTable.AddAttribute(tag.Key, tag.Value);
                                }
                            }

                            // build feature.
                            var lineString = new LineString(coordinates.ToArray());
                            featureCollection.Add(new Feature(lineString, attributesTable));
                        }

                        if(current.Points != null)
                        {
                            foreach(var point in current.Points)
                            {
                                // build attributes.
                                var currentPointTags = point.Tags;
                                var attributesTable = new AttributesTable();
                                if (currentPointTags != null)
                                { // there are tags.
                                    foreach (var tag in currentPointTags)
                                    {
                                        attributesTable.AddAttribute(tag.Key, tag.Value);
                                    }
                                }

                                // build feature.
                                var pointGeometry = new Point(new Coordinate(point.Location.Longitude, point.Location.Latitude));
                                pointsCollection.Add(new Feature(pointGeometry, attributesTable));
                            }
                        }

                        // get the next point.
                        current = currentArc.Next;
                    }
                    else
                    { // there is no next anymore.
                        current = null;
                    }
                }
            }
            
            // add points
            foreach(var point in pointsCollection.Features)
            {
                featureCollection.Add(point);
            }
            return featureCollection;
        }
    }
}