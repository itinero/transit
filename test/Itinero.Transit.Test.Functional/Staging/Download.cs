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