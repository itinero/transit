// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using Itinero.Attributes;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.Transit.Data
{
    /// <summary>
    /// A transit db containing connections and tranfers.
    /// </summary>
    public class TransitDb
    {
        private readonly AttributesIndex _agencyAttributes;
        private readonly StopsDb _stopsDb;
        private readonly AttributesIndex _stopAttributes;
        private readonly ConnectionsDb _connectionsDb;
        private readonly TripsDb _tripsDb;
        private readonly AttributesIndex _tripAttributes;
        private readonly SchedulesDb _schedulesDb;
        private readonly Dictionary<string, TransfersDb> _transfersDbs;
        private readonly ShapesDb _shapesDb;

        /// <summary>
        /// Creates a new transit db.
        /// </summary>
        public TransitDb()
        {
            _agencyAttributes = new AttributesIndex(AttributesIndexMode.ReverseStringIndex);
            _connectionsDb = new ConnectionsDb();
            _stopsDb = new StopsDb();
            _stopAttributes = new AttributesIndex(AttributesIndexMode.ReverseStringIndex);
            _tripsDb = new TripsDb();
            _tripAttributes = new AttributesIndex(AttributesIndexMode.ReverseStringIndex);
            _transfersDbs = new Dictionary<string, TransfersDb>();
            _schedulesDb = new SchedulesDb();
            _shapesDb = new ShapesDb();
        }

        /// <summary>
        /// Creates a new transit db.
        /// </summary>
        private TransitDb(AttributesIndex agencyAttributes, ConnectionsDb connectionsDb, SchedulesDb schedulesDb, AttributesIndex stopAttributes, 
            StopsDb stopsDb, Dictionary<string, TransfersDb> transferDbs, AttributesIndex tripAttributes, TripsDb tripsDb, ShapesDb shapesDb)
        {
            _agencyAttributes = agencyAttributes;
            _connectionsDb = connectionsDb;
            _schedulesDb = schedulesDb;
            _stopAttributes = stopAttributes;
            _stopsDb = stopsDb;
            _transfersDbs = transferDbs;
            _tripAttributes = tripAttributes;
            _tripsDb = tripsDb;
            _shapesDb = shapesDb;
        }

        /// <summary>
        /// Gets the agency attributes.
        /// </summary>
        public AttributesIndex AgencyAttributes
        {
            get
            {
                return _agencyAttributes;
            }
        }
        
        /// <summary>
        /// Adds a new stop.
        /// </summary>
        public uint AddStop(float latitude, float longitude, uint metaId)
        {
            return _stopsDb.Add(latitude, longitude, metaId);
        }

        /// <summary>
        /// Sorts the stops.
        /// </summary>
        public void SortStops(Action<uint, uint> switchStops)
        {
            _stopsDb.Sort(switchStops);
        }

        /// <summary>
        /// Gets the stops enumerator.
        /// </summary>
        /// <returns></returns>
        public StopsDb.Enumerator GetStopsEnumerator()
        {
            return _stopsDb.GetEnumerator();
        }

        /// <summary>
        /// Gets the stop count.
        /// </summary>
        public uint StopsCount
        {
            get
            {
                return _stopsDb.Count;
            }
        }

        /// <summary>
        /// Gets the stop attributes.
        /// </summary>
        public AttributesIndex StopAttributes
        {
            get
            {
                return _stopAttributes;
            }
        }

        /// <summary>
        /// Adds a connection.
        /// </summary>
        public uint AddConnection(uint stop1, uint stop2, uint tripId, uint departureTime, uint arrivalTime)
        {
            if (stop1 >= _stopsDb.Count) { throw new ArgumentOutOfRangeException("stop1"); }
            if (stop2 >= _stopsDb.Count) { throw new ArgumentOutOfRangeException("stop2"); }
            if (tripId >= _tripsDb.Count) { throw new ArgumentOutOfRangeException("tripId"); }

            return _connectionsDb.Add(stop1, stop2, tripId, departureTime, arrivalTime);
        }

        /// <summary>
        /// Gets the sorting.
        /// </summary>
        public DefaultSorting? ConnectionSorting
        {
            get
            {
                return _connectionsDb.Sorting;
            }
        }

        /// <summary>
        /// Sorts the connections.
        /// </summary>
        public void SortConnections(DefaultSorting sorting, Action<uint, uint> switchConnections)
        {
            _connectionsDb.Sort(sorting, switchConnections);
        }

        /// <summary>
        /// Gets the connection enumerator with the given sorting.
        /// </summary>
        /// <returns></returns>
        public ConnectionsDb.Enumerator GetConnectionsEnumerator(DefaultSorting sorting)
        {
            return _connectionsDb.GetEnumerator(sorting);
        }

        /// <summary>
        /// Gets the connections db.
        /// </summary>
        public ConnectionsDb ConnectionsDb
        {
            get
            {
                return _connectionsDb;
            }
        }

        /// <summary>
        /// Adds a new trip.
        /// </summary>
        public uint AddTrip(uint scheduleId, uint agencyId, uint metaId)
        {
            return _tripsDb.Add(scheduleId, agencyId, metaId);
        }

        /// <summary>
        /// Gets the trips enumerator.
        /// </summary>
        /// <returns></returns>
        public TripsDb.Enumerator GetTripsEnumerator()
        {
            return _tripsDb.GetEnumerator();
        }

        /// <summary>
        /// Gets the trips db.
        /// </summary>
        public TripsDb TripsDb
        {
            get
            {
                return _tripsDb;
            }
        }

        /// <summary>
        /// Gets the trip attributes.
        /// </summary>
        public AttributesIndex TripAttributes
        {
            get
            {
                return _tripAttributes;
            }
        }

        /// <summary>
        /// Gets the trip count.
        /// </summary>
        public uint TripsCount
        {
            get
            {
                return _tripsDb.Count;
            }
        }

        /// <summary>
        /// Adds a new schedule.
        /// </summary>
        public uint AddSchedule()
        {
            return _schedulesDb.Add();
        }

        /// <summary>
        /// Adds a new schedule entry.
        /// </summary>
        public void AddScheduleEntry(uint id, DateTime start, DateTime end, byte weekMask)
        {
            _schedulesDb.AddEntry(id, start, end, weekMask);
        }

        /// <summary>
        /// Gets the schedule enumerator.
        /// </summary>
        /// <returns></returns>
        public SchedulesDb.Enumerator GetSchedulesEnumerator()
        {
            return _schedulesDb.GetEnumerator();
        }

        /// <summary>
        /// Gets the schedules db.
        /// </summary>
        public SchedulesDb SchedulesDb
        {
            get
            {
                return _schedulesDb;
            }
        }

        /// <summary>
        /// Gets the trip count.
        /// </summary>
        public uint ConnectionsCount
        {
            get
            {
                return _connectionsDb.Count;
            }
        }

        /// <summary>
        /// Adds the transfers db.
        /// </summary>
        public void AddTransfersDb(Profiles.Profile profile, TransfersDb db)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }
            if (db == null) { throw new ArgumentNullException("db"); }

            _transfersDbs[profile.Name] = db;
        }

        /// <summary>
        /// Returns true if there is a transfers db.
        /// </summary>
        public bool HasTransfersDb()
        {
            return _transfersDbs.Count > 0;
        }

        /// <summary>
        /// Returns true if there is a transfers db for the given profile.
        /// </summary>
        public bool HasTransfersDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            return _transfersDbs.ContainsKey(profile.Name);
        }

        /// <summary>
        /// Gets the transfers db.
        /// </summary>
        public TransfersDb GetTransfersDb(Profiles.Profile profile)
        {
            if (profile == null) { throw new ArgumentNullException("profile"); }

            TransfersDb transfersDb = null;
            if(_transfersDbs.TryGetValue(profile.Name, out transfersDb))
            {
                return transfersDb;
            }
            return null;
        }

        /// <summary>
        /// Gets the shapes db.
        /// </summary>
        public ShapesDb ShapesDb
        {
            get
            {
                return _shapesDb;
            }
        }

        /// <summary>
        /// Serializes to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            var position = stream.Position;
            stream.WriteByte(2); // write the version #.

            // write agencies attributes.
            _agencyAttributes.Serialize(new LimitedStream(stream));

            // write connections db.
            _connectionsDb.Serialize(stream);

            // write schedules db.
            _schedulesDb.Serialize(stream);

            // write stop attributes.
            _stopAttributes.Serialize(new LimitedStream(stream));

            // write stop db.
            _stopsDb.Serialize(stream);

            // write transfer db's.
            stream.WriteByte((byte)_transfersDbs.Count);
            foreach(var transferDb in _transfersDbs)
            {
                stream.WriteWithSize(transferDb.Key);
                transferDb.Value.Serialize(stream);
            }

            // write trip attributes.
            _tripAttributes.Serialize(new LimitedStream(stream));

            // write trips db.
            _tripsDb.Serialize(stream);

            // write shapes db.
            _shapesDb.Serialize(stream);

            return stream.Position - position;
        }

        /// <summary>
        /// Deserializes from stream.
        /// </summary>
        public static TransitDb Deserialize(Stream stream)
        {
            var version = stream.ReadByte();
            if (version > 2)
            {
                throw new Exception("Cannot deserialize db, version # doesn't match.");
            }     

            // read agencies attributes.
            var agencyAttributes = AttributesIndex.Deserialize(new LimitedStream(stream), true);

            // read connections db.
            var connectionsDb = ConnectionsDb.Deserialize(stream);

            // read schedules db.
            var schedulesDb = SchedulesDb.Deserialize(stream);

            // read stop attributes.
            var stopAttributes = AttributesIndex.Deserialize(new LimitedStream(stream), true);

            // read stop db.
            var stopsDb = StopsDb.Deserialize(stream);

            // read transfer db's.
            var transferDbsCount = stream.ReadByte();
            var transferDbs = new Dictionary<string, TransfersDb>();
            for(var i = 0; i < transferDbsCount; i++)
            {
                var profileName = stream.ReadWithSizeString();
                var transferDb = TransfersDb.Deserialize(stream);

                transferDbs.Add(profileName, transferDb);
            }

            // read trip attributes.
            var tripAttributes = AttributesIndex.Deserialize(new LimitedStream(stream), true);

            // write trips db.
            var tripsDb = TripsDb.Deserialize(stream);

            ShapesDb shapesDb = null;
            if (version != 1)
            { // write shapes db.
                shapesDb = ShapesDb.Deserialize(stream);
            }

            return new TransitDb(agencyAttributes, connectionsDb, schedulesDb, stopAttributes, stopsDb, transferDbs,
                tripAttributes, tripsDb, shapesDb);
        }
    }
}