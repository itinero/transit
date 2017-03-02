// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.IO.Compression;
using System.Net;

namespace Itinero.Transit.Test.Functional.Staging
{
    public static class Download
    {
        public static string BelgiumRemote = "http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf";
        public static string BelgiumLocal = "belgium-latest.osm.pbf";

        public static string NMBSSource = "http://files.itinero.tech/data/GTFS/nmbs/nmbs-latest-shapes.zip";
        public static string DeLijnSource = "http://files.itinero.tech/data/GTFS/delijn/delijn-latest-shapes.zip";
        public static string MIVBSource = "http://files.itinero.tech/data/GTFS/mivb/mivb-20170114.zip";
        public static string TECSource = "http://files.itinero.tech/data/GTFS/tec/tec-20161111.zip";

        /// <summary>
        /// Downloads the belgium routing file.
        /// </summary>
        public static void DownloadBelgiumAll()
        {
            if (!File.Exists("belgium-latest.osm.pbf"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumRemote,
                    Download.BelgiumLocal);
            }
        }

        /// <summary>
        /// Downloads the NMBS test feed.
        /// </summary>
        public static string DownloadNMBS()
        {
            if (!Directory.Exists("NMBS"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.NMBSSource, "NMBS.zip");
                ZipFile.ExtractToDirectory("NMBS.zip", "NMBS");
                File.Delete("NMBS.zip");
            }
            return "NMBS";
        }

        /// <summary>
        /// Downloads the De Lijn test feed.
        /// </summary>
        public static string DownloadDeLijn()
        {
            if (!Directory.Exists("DeLijn"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.DeLijnSource, "DeLijn.zip");
                ZipFile.ExtractToDirectory("DeLijn.zip", "DeLijn");
                File.Delete("DeLijn.zip");
            }
            return "DeLijn";
        }

        /// <summary>
        /// Downloads the MIVB test feed.
        /// </summary>
        public static string DownloadMIVB()
        {
            if (!Directory.Exists("MIVB"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.MIVBSource, "MIVB.zip");
                ZipFile.ExtractToDirectory("MIVB.zip", "MIVB");
                File.Delete("MIVB.zip");
            }
            return "MIVB";
        }

        /// <summary>
        /// Downloads the TEC test feed.
        /// </summary>
        public static string DownloadTEC()
        {
            if (!Directory.Exists("TEC"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.TECSource, "TEC.zip");
                ZipFile.ExtractToDirectory("TEC.zip", "TEC");
                File.Delete("TEC.zip");
            }
            return "TEC";
        }
    }
}