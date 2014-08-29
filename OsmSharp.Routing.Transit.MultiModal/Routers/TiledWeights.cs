using OsmSharp.Math.Geo;
using OsmSharp.Osm.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.Routers
{
    /// <summary>
    /// Represents a collection of samples collected per tile.
    /// </summary>
    public class TiledWeights : IEnumerable<Tuple<Tile, double>>
    {
        /// <summary>
        /// Holds the zoom level.
        /// </summary>
        private int _zoomLevel;

        /// <summary>
        /// Holds the sample count per tile.
        /// </summary>
        private Dictionary<ulong, int> _samplesPerTile;

        /// <summary>
        /// Holds the value per tile.
        /// </summary>
        private Dictionary<ulong, double> _valuePerTile;

        /// <summary>
        /// Keeps samples into cells the size of tiles at the given zoom level.
        /// </summary>
        /// <param name="zoomLevel"></param>
        public TiledWeights(int zoomLevel)
        {
            _zoomLevel = zoomLevel;
            _samplesPerTile = new Dictionary<ulong, int>(10000);
            _valuePerTile = new Dictionary<ulong, double>(10000);
        }

        /// <summary>
        /// Adds a new sample at the given location.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="weight"></param>
        /// <returns>False when outside of the range of this matrix.</returns>
        public bool AddSample(double latitude, double longitude, double weight)
        {
            var tile = Tile.CreateAroundLocation(latitude, longitude, _zoomLevel);
            // add sample.
            int sampleCount;
            if (!_samplesPerTile.TryGetValue(tile.Id, out sampleCount))
            {
                _samplesPerTile[tile.Id] = 1;
                _valuePerTile[tile.Id] = weight;
                return true;
            }
            double value = _valuePerTile[tile.Id];
            _samplesPerTile[tile.Id] = sampleCount + 1;
            _valuePerTile[tile.Id] = (weight + (sampleCount * value)) / (sampleCount + 1);
            return true;
        }

        /// <summary>
        /// Gets the number of samples.
        /// </summary>
        public int Count
        {
            get
            {
                return _valuePerTile.Count;
            }
        }

        /// <summary>
        /// Returns the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Tuple<Tile, double>> GetEnumerator()
        {
            return new Enumerator(_valuePerTile.GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(_valuePerTile.GetEnumerator());
        }

        private class Enumerator : IEnumerator<Tuple<Tile, double>>
        {
            private IEnumerator<KeyValuePair<ulong, double>> _enumerator;

            public Enumerator(IEnumerator<KeyValuePair<ulong, double>> enumerator)
            {
                _enumerator = enumerator;
            }

            public Tuple<Tile, double> Current
            {
                get { return new Tuple<Tile,double>(new Tile(_enumerator.Current.Key), _enumerator.Current.Value); }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return new Tuple<Tile, double>(new Tile(_enumerator.Current.Key), _enumerator.Current.Value); }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}