namespace OsmSharp.Routing.Transit
{
    /// <summary>
    /// Represents the a pair of schedules for one transit edge.
    /// </summary>
    public class TransitEdgeSchedulePair
    {
        /// <summary>
        /// Creates a new transit edge schedule pair.
        /// </summary>
        public TransitEdgeSchedulePair()
        {
            this.Forward = new TransitEdgeSchedule();
            this.Backward = new TransitEdgeSchedule();
        }

        /// <summary>
        /// Holds the forward schedule.
        /// </summary>
        public TransitEdgeSchedule Forward { get; set; }

        /// <summary>
        /// Holds the backward schedule.
        /// </summary>
        public TransitEdgeSchedule Backward { get; set; }
    }
}