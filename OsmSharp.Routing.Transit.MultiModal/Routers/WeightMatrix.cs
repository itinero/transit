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
    /// Represents a matrix of weights divided based on tiles. One weight per tile.
    /// </summary>
    public class WeightMatrix
    {
        /// <summary>
        /// Holds the tile range.
        /// </summary>
        private TileRange _tileRange;

        /// <summary>
        /// Holds the sample count per tile.
        /// </summary>
        private Dictionary<ulong, int> _samplesPerTile;

        /// <summary>
        /// Holds the value per tile.
        /// </summary>
        private Dictionary<ulong, double> _valuePerTile;

        /// <summary>
        /// Builds a matrix that at least covers the given box and divides cells into tiles the size of tiles at the given zoom level.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="zoomLevel"></param>
        public WeightMatrix(GeoCoordinateBox box, int zoomLevel)
        {
            _tileRange = TileRange.CreateAroundBoundingBox(box, zoomLevel);
            _samplesPerTile = new Dictionary<ulong, int>(_tileRange.Count);
            _valuePerTile = new Dictionary<ulong, double>(_tileRange.Count);
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
            var tile = Tile.CreateAroundLocation(latitude, longitude, _tileRange.Zoom);
            if(_tileRange.Contains(tile))
            { // add sample.
                int sampleCount;
                if(!_samplesPerTile.TryGetValue(tile.Id, out sampleCount))
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
            return false;
        }

        /// <summary>
        /// Returns the sample value.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetSample(int row, int column, out double latitude, out double longitude, out double value)
        {
            var tile = new Tile(_tileRange.XMin + row, _tileRange.YMin + column, _tileRange.Zoom);
            var middle = tile.Box.Middle;
            latitude = middle[1];
            longitude = middle[0];
            return _valuePerTile.TryGetValue(tile.Id, out value);
        }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int Columns
        {
            get
            {
                return _tileRange.YMax - _tileRange.YMin;
            }
        }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        public int Rows
        {
            get
            {
                return _tileRange.XMax - _tileRange.XMin;
            }
        }
    }
}