// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Sorting;
using OsmSharp.Math.Algorithms;
using OsmSharp.Routing.Algorithms.Search;
using Reminiscence.Arrays;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Transit.Data
{
    /// <summary>
    /// Represents a database of transit-connections optimized for routing.
    /// </summary>
    public class ConnectionsDb
    { 
        // one connection is 4 uints
        // 0: stop1
        // 1: stop2
        // 2: profileId
        // 4: 17-bit time-of-day and 15-bit duration: 
        //    - departure time is accurate to the second.
        //    - the maximum duration of a single connection is 32768 seconds or 9h6m.
        private const int CONNECTION_SIZE = 4;
        private const int CONNECTION_MAX_DEPARTURETIME = 131072 - 1;
        private const int CONNECTION_MAX_DURATION = 32768 - 1;
        private const int CONNECTIONS_BLOCK_SIZE = 1000; // the connections block size.
        private const int STOP_SIZE = 3; // one connection is 3 uints.
        private const int STOP_BLOCK_SIZE = 1000; // the stop block size.
        private readonly ArrayBase<uint> _connections; // holds all connection data.
        private readonly ArrayBase<uint> _connectionsOrder; // hold the connections-order for the other sorting.
        private readonly ArrayBase<uint> _stops; // holds all hilbert-sorted stops.
        
        /// <summary>
        /// Creates new connections db.
        /// </summary>
        public ConnectionsDb()
            : this(256, 2048)
        {

        }

        /// <summary>
        /// Creates new connections db.
        /// </summary>
        public ConnectionsDb(int stops, int connections)
        {
            _connections = new MemoryArray<uint>(connections * CONNECTION_SIZE);
            _connectionsOrder = new MemoryArray<uint>(connections);
            _stops = new MemoryArray<uint>(stops);
        }
        
        private DefaultSorting? _sorting; // hold the current sorting.
        private uint _nextStopId = 0;
        private uint _nextConnectionId; // holds the maximum connection id.

        /// <summary>
        /// Adds a new stop.
        /// </summary>
        public uint AddStop(float latitude, float longitude, uint metaId)
        {
            var id = _nextStopId;
            _nextStopId++;

            var size = _stops.Length;
            while ((id * STOP_SIZE + STOP_SIZE) > size)
            {
                size += STOP_BLOCK_SIZE;
            }
            if(size != _stops.Length)
            {
                _stops.Resize(size);
            }

            _stops[id * STOP_SIZE + 0] = ConnectionsDb.Encode(latitude);
            _stops[id * STOP_SIZE + 1] = ConnectionsDb.Encode(longitude);
            _stops[id * STOP_SIZE + 2] = metaId;

            return id;
        }

        /// <summary>
        /// Returns the number of stops.
        /// </summary>
        public uint StopCount
        {
            get
            {
                return _nextStopId;
            }
        }

        /// <summary>
        /// Sorts the stops.
        /// </summary>
        public void SortStops(Action<uint, uint> switchConnections)
        {
            if (_nextStopId > 0)
            { // sort stops, assume all stops are filled-in.
                QuickSort.Sort((stop) =>
                    {
                        var latitude = ConnectionsDb.DecodeSingle(_stops[stop * STOP_SIZE + 0]);
                        var longitude = ConnectionsDb.DecodeSingle(_stops[stop * STOP_SIZE + 1]);
                        return HilbertCurve.HilbertDistance(latitude, longitude, Hilbert.DefaultHilbertSteps);
                    },
                    (stop1, stop2) =>
                    {
                        var stop10 = _stops[stop1 * STOP_SIZE + 0];
                        var stop11 = _stops[stop1 * STOP_SIZE + 1];
                        var stop12 = _stops[stop1 * STOP_SIZE + 2];
                        _stops[stop1 * STOP_SIZE + 0] = _stops[stop2 * STOP_SIZE + 0];
                        _stops[stop1 * STOP_SIZE + 1] = _stops[stop2 * STOP_SIZE + 1];
                        _stops[stop1 * STOP_SIZE + 2] = _stops[stop2 * STOP_SIZE + 2];
                        _stops[stop2 * STOP_SIZE + 0] = stop10;
                        _stops[stop2 * STOP_SIZE + 1] = stop11;
                        _stops[stop2 * STOP_SIZE + 2] = stop12;

                        if (switchConnections != null)
                        {
                            switchConnections((uint)stop1, (uint)stop2);
                        }
                    }, 0, _nextStopId - 1);
            }
        }

        /// <summary>
        /// Sets the connection with the given id.
        /// </summary>
        public uint AddConnection(uint stop1, uint stop2, uint profileId, uint departureTime, uint arrivalTime)
        {
            if (stop1 > _stops.Length) { throw new ArgumentOutOfRangeException("stop1"); }
            if (stop2 > _stops.Length) { throw new ArgumentOutOfRangeException("stop2"); }
            if (arrivalTime < departureTime) { throw new ArgumentException("Departure time must be smaller than or equal to arrival time."); }
            var duration = arrivalTime - departureTime;
            if (duration > CONNECTION_MAX_DURATION) { 
                throw new ArgumentException(string.Format("A connection with a duration > {0}s cannot be stored.", CONNECTION_MAX_DURATION)); }

            var id = _nextConnectionId;
            _nextConnectionId++;

            var size = _connections.Length;
            while ((id * CONNECTION_SIZE + CONNECTION_SIZE) > size)
            {
                size += STOP_BLOCK_SIZE;
            }
            if (size != _connections.Length)
            {
                _connections.Resize(size);
            }

            _connections[id * CONNECTION_SIZE + 0] = stop1;
            _connections[id * CONNECTION_SIZE + 1] = stop2;
            _connections[id * CONNECTION_SIZE + 2] = profileId;
            _connections[id * CONNECTION_SIZE + 3] = ConnectionsDb.Encode(departureTime, duration);

            return id;
        }

        /// <summary>
        /// Gets the sorting.
        /// </summary>
        public DefaultSorting? Sorting
        {
            get
            {
                return _sorting;
            }
        }

        /// <summary>
        /// Sorts the connections.
        /// </summary>
        public void SortConnections(DefaultSorting sorting, Action<uint, uint> switchConnections)
        {
            _sorting = sorting;

            if (_nextConnectionId > 0)
            {
                _connectionsOrder.Resize(_nextConnectionId);

                for (uint i = 0; i < _nextConnectionId; i++)
                {
                    _connectionsOrder[i] = i;
                }

                QuickSort.Sort((connection) =>
                    {
                        uint departureTime, duration;
                        ConnectionsDb.DecodeDepartureTimeAndDuration(
                            _connections[connection * CONNECTION_SIZE + 3],
                                out departureTime, out duration);
                        if (sorting == DefaultSorting.DepartureTime)
                        {
                            return departureTime;
                        }
                        return departureTime + duration;
                    },
                    (connection1, connection2) =>
                    {
                        var value0 = _connections[connection1 * CONNECTION_SIZE + 0];
                        var value1 = _connections[connection1 * CONNECTION_SIZE + 1];
                        var value2 = _connections[connection1 * CONNECTION_SIZE + 2];
                        var value3 = _connections[connection1 * CONNECTION_SIZE + 3];
                        _connections[connection1 * CONNECTION_SIZE + 0] = _connections[connection2 * CONNECTION_SIZE + 0];
                        _connections[connection1 * CONNECTION_SIZE + 1] = _connections[connection2 * CONNECTION_SIZE + 1];
                        _connections[connection1 * CONNECTION_SIZE + 2] = _connections[connection2 * CONNECTION_SIZE + 2];
                        _connections[connection1 * CONNECTION_SIZE + 3] = _connections[connection2 * CONNECTION_SIZE + 3];
                        _connections[connection2 * CONNECTION_SIZE + 0] = value0;
                        _connections[connection2 * CONNECTION_SIZE + 1] = value1;
                        _connections[connection2 * CONNECTION_SIZE + 2] = value2;
                        _connections[connection2 * CONNECTION_SIZE + 3] = value3;

                        if (switchConnections != null)
                        {
                            switchConnections((uint)connection1, (uint)connection2);
                        }
                    }, 0, _nextConnectionId - 1);

                QuickSort.Sort((connection) =>
                    {
                        uint departureTime, duration;
                        ConnectionsDb.DecodeDepartureTimeAndDuration(
                            _connections[_connectionsOrder[connection] * CONNECTION_SIZE + 3],
                                out departureTime, out duration);
                        if (sorting != DefaultSorting.DepartureTime)
                        {
                            return departureTime;
                        }
                        return departureTime + duration;
                    },
                     (connection1, connection2) =>
                     {
                         var value = _connectionsOrder[connection1];
                         _connectionsOrder[connection1] = _connectionsOrder[connection2];
                         _connectionsOrder[connection2] = value;
                     }, 0, _nextConnectionId - 1);
            }
        }

        /// <summary>
        /// Gets a stops enumerator.
        /// </summary>
        public StopsEnumerator GetStopEnumerator()
        {
            return new StopsEnumerator(_stops);
        }

        /// <summary>
        /// A stop enumerator.
        /// </summary>
        public class StopsEnumerator
        {
            private readonly ArrayBase<uint> _stops; // holds the stops-array.

            internal StopsEnumerator(ArrayBase<uint> stops)
            {
                _stops = stops;
            }

            private uint _index = uint.MaxValue;

            /// <summary>
            /// Moves this enumerator to the given stop.
            /// </summary>
            public void MoveTo(uint id)
            {
                _index = id * STOP_SIZE;
            }

            /// <summary>
            /// Gets the latitude.
            /// </summary>
            public float Latitude
            {
                get
                {
                    return ConnectionsDb.DecodeSingle(
                        _stops[_index + 0]);
                }
            }

            /// <summary>
            /// Gets the longitude.
            /// </summary>
            public float Longitude
            {
                get
                {
                    return ConnectionsDb.DecodeSingle(
                        _stops[_index + 1]);
                }
            }

            /// <summary>
            /// Gets the meta-id.
            /// </summary>
            public uint MetaId
            {
                get
                {
                    return _stops[_index + 2];
                }
            }
        }

        /// <summary>
        /// Gets a connection enumerator.
        /// </summary>
        /// <returns></returns>
        public ConnectionEnumerator GetConnectionEnumerator()
        {
            return new ConnectionEnumerator(_connections, _nextConnectionId);
        }

        /// <summary>
        /// Gets a connection enumerator.
        /// </summary>
        /// <returns></returns>
        public ConnectionEnumerator GetConnectionOrderEnumerator()
        {
            if (_connectionsOrder.Length == 0 && _connections.Length != 0)
            {
                throw new InvalidOperationException("Cannot get sorted enumerator, db is not sorted.");
            }
            return new ConnectionEnumerator(_connections, _connectionsOrder, _nextConnectionId);
        }

        /// <summary>
        /// Gets the connection enumerator with the given sorting.
        /// </summary>
        /// <returns></returns>
        public ConnectionEnumerator GetConnectionEnumerator(DefaultSorting sorting)
        {
            if (_sorting == null) { throw new InvalidOperationException("Cannot get sorted enumerator, db is not sorted."); }
            if(_sorting == sorting)
            {
                return this.GetConnectionEnumerator();
            }
            return this.GetConnectionOrderEnumerator();
        }

        /// <summary>
        /// A connection enumerator.
        /// </summary>
        public class ConnectionEnumerator
        {
            private readonly ArrayBase<uint> _connections;
            private readonly ArrayBase<uint> _connectionsOrder;
            private readonly uint _count;

            internal ConnectionEnumerator(ArrayBase<uint> connections, uint count)
            {
                _connections = connections;
                _connectionsOrder = null;
                _count = count;
            }

            internal ConnectionEnumerator(ArrayBase<uint> connections,
                ArrayBase<uint> connectionsOrder, uint count)
            {
                _connections = connections;
                _connectionsOrder = connectionsOrder;
                _count = count;
            }

            private uint _id = uint.MaxValue;
            private uint _index = uint.MaxValue;

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _id = uint.MaxValue;
            }

            /// <summary>
            /// Moves to the connection with the given id.
            /// </summary>
            public bool MoveTo(uint id)
            {
                _id = id;
                _index = _id * CONNECTION_SIZE;
                if (_id < _count)
                {
                    if (_connectionsOrder != null)
                    { // translate index.
                        _index = _connectionsOrder[_id] * CONNECTION_SIZE;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Move to the next connection.
            /// </summary>
            public bool MoveNext()
            {
                if (_id == uint.MaxValue)
                { // first move.
                    _id = 0;
                }
                else
                { // all other moves.
                    _id++;
                }
                _index = _id * CONNECTION_SIZE;
                if (_id < _count)
                {
                    if (_connectionsOrder != null)
                    { // translate index.
                        _index = _connectionsOrder[_index];
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets the departure stop.
            /// </summary>
            public uint Stop1
            {
                get
                {
                    return _connections[_index + 0];
                }
            }

            /// <summary>
            /// Gets the arrival stop.
            /// </summary>
            public uint Stop2
            {
                get
                {
                    return _connections[_index + 1];
                }
            }

            /// <summary>
            /// Gets the profile id.
            /// </summary>
            public uint ProfileId
            {
                get
                {
                    return _connections[_index + 2];
                }
            }

            /// <summary>
            /// Gets the departure time.
            /// </summary>
            public uint DepartureTime
            {
                get
                {
                    uint departureTime;
                    uint duration;
                    ConnectionsDb.DecodeDepartureTimeAndDuration(_connections[_index + 3],
                        out departureTime, out duration);
                    return departureTime;
                }
            }

            /// <summary>
            /// Gets the arrival time.
            /// </summary>
            public uint ArrivalTime
            {
                get
                {
                    uint departureTime;
                    uint duration;
                    ConnectionsDb.DecodeDepartureTimeAndDuration(_connections[_index + 3],
                        out departureTime, out duration);
                    return departureTime + duration;
                }
            }
        }

        /// <summary>
        /// Encodes a departure time and duration.
        /// </summary>
        private static uint Encode(uint departureTime, uint duration)
        {
            if (departureTime > CONNECTION_MAX_DEPARTURETIME)
            {
                throw new ArgumentException(string.Format("Cannot store a connection with a departure time bigger than {0}s.", CONNECTION_MAX_DEPARTURETIME));
            }
            if (duration > CONNECTION_MAX_DURATION)
            {
                throw new ArgumentException(string.Format("Cannot store a connection with a duration bigger than {0}s.", CONNECTION_MAX_DURATION));
            }
            return departureTime + (duration << 17);
        }

        /// <summary>
        /// Encodes a departure time and duration.
        /// </summary>
        private static void DecodeDepartureTimeAndDuration(uint value , out uint departureTime, out uint duration)
        {
            departureTime = value << 15 >> 15;
            duration = value >> 17;
        }

        /// <summary>
        /// Encodes a float into a uint.
        /// </summary>
        private static uint Encode(float latitude)
        {
            return System.BitConverter.ToUInt32(
                System.BitConverter.GetBytes(latitude), 0);
        }

        /// <summary>
        /// Encodes a float into a uint.
        /// </summary>
        private static float DecodeSingle(uint value)
        {
            return System.BitConverter.ToSingle(
                System.BitConverter.GetBytes(value), 0);
        }
    }
}
