//using OsmSharp.Math.Geo.Simple;
//using OsmSharp.Routing.Graph;
//using OsmSharp.Routing.Osm.Graphs;
//using OsmSharp.Routing.Transit;

//namespace OsmSharp.Routing.Transit.MultiModal
//{  
//    /// <summary>
//    /// A simple edge containing the orignal OSM-tags or that represents a transit-edge.
//    /// </summary>
//    public struct MultiModalEdge : IDynamicGraphEdgeData
//    {
//        /// <summary>
//        /// Contains a value that represents tagsId and forward flag [forwardFlag (true when zero)][tagsIdx].
//        /// </summary>
//        private uint _value;

//        /// <summary>
//        /// Gets/sets the value.
//        /// </summary>
//        internal uint Value
//        {
//            get
//            {
//                return _value;
//            }
//            set
//            {
//                _value = value;
//            }
//        }

//        /// <summary>
//        /// Flag indicating if this is a forward or backward edge relative to the tag descriptions.
//        /// </summary>
//        public bool Forward
//        {
//            get
//            { // true when first bit is 0.
//                return _value % 2 == 0;
//            }
//            set
//            {
//                if (_value % 2 == 0)
//                { // true already.
//                    if (!value) { _value = _value + 1; }
//                }
//                else
//                { // false already.
//                    if (value) { _value = _value - 1; }
//                }
//            }
//        }

//        /// <summary>
//        /// The properties of this edge.
//        /// </summary>
//        public uint Tags
//        {
//            get
//            {
//                return _value / 2;
//            }
//            set
//            {
//                if (_value % 2 == 0)
//                { // true already.
//                    _value = value * 2;
//                }
//                else
//                { // false already.
//                    _value = (value * 2) + 1;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the list of intermediate coordinates.
//        /// </summary>
//        public GeoCoordinateSimple[] Coordinates { get; set; }

//        /// <summary>
//        /// Gets/or sets the total distance of this edge.
//        /// </summary>
//        public float Distance { get; set; }

//        /// <summary>
//        /// Returns true if this edge represents a neighbour-relation.
//        /// </summary>
//        public bool RepresentsNeighbourRelations
//        {
//            get { return true; }
//        }

//        /// <summary>
//        /// Returns true if the other edge represents the same information than this edge.
//        /// </summary>
//        /// <param name="other"></param>
//        /// <returns></returns>
//        public bool Equals(IDynamicGraphEdgeData other)
//        {
//            if (other is MultiModalEdge)
//            { // ok, type is the same.
//                var otherLive = (MultiModalEdge)other;
//                if (otherLive._value != this._value)
//                { // basic info different.
//                    return false;
//                }

//                // only the coordinates can be different now.
//                if (this.Coordinates != null)
//                { // both have to contain the same coordinates.
//                    if (otherLive.Coordinates == null ||
//                        this.Coordinates.Length != otherLive.Coordinates.Length)
//                    { // impossible, different number of coordinates.
//                        return false;
//                    }

//                    for (int idx = 0; idx < otherLive.Coordinates.Length; idx++)
//                    {
//                        if (this.Coordinates[idx].Longitude != otherLive.Coordinates[idx].Longitude ||
//                            this.Coordinates[idx].Latitude != otherLive.Coordinates[idx].Latitude)
//                        { // oeps, coordinates are different!
//                            return false;
//                        }
//                    }
//                    return true;
//                }
//                else
//                { // both are null.
//                    return otherLive.Coordinates == null;
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Returns true if the other edge represents the same geographical information than this edge.
//        /// </summary>
//        /// <param name="other"></param>
//        /// <returns></returns>
//        public bool EqualsGeometrically(IDynamicGraphEdgeData other)
//        {
//            if (other is MultiModalEdge)
//            { // ok, type is the same.
//                var otherLive = (MultiModalEdge)other;

//                // only the coordinates can be different now.
//                if (this.Coordinates != null)
//                { // both have to contain the same coordinates.
//                    if (this.Coordinates.Length != otherLive.Coordinates.Length)
//                    { // impossible, different number of coordinates.
//                        return false;
//                    }

//                    for (int idx = 0; idx < otherLive.Coordinates.Length; idx++)
//                    {
//                        if (this.Coordinates[idx].Longitude != otherLive.Coordinates[idx].Longitude ||
//                            this.Coordinates[idx].Latitude != otherLive.Coordinates[idx].Latitude)
//                        { // oeps, coordinates are different!
//                            return false;
//                        }
//                    }
//                    return true;
//                }
//                else
//                { // both are null.
//                    return otherLive.Coordinates == null;
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Returns true if this edge is a road.
//        /// </summary>
//        public bool IsRoad
//        {
//            get
//            {
//                return this.Tags != 0;
//            }
//        }

//        /// <summary>
//        /// Returns true if this edge is a transit edge.
//        /// </summary>
//        public bool IsTransit
//        {
//            get
//            {
//                return this.ForwardSchedule != null ||
//                    this.BackwardSchedule != null;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the forward transit edge schedule.
//        /// </summary>
//        public TransitEdgeSchedule ForwardSchedule { get; set; }

//        /// <summary>
//        /// Gets or sets the backward transit edge schedule.
//        /// </summary>
//        public TransitEdgeSchedule BackwardSchedule { get; set; }
//    }
//}