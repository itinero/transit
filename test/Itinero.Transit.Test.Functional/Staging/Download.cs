using System.IO;
using System.IO.Compression;
using System.Net;

namespace Itinero.Transit.Test.Functional.Staging
{
    public static class Download
    {
        public static string BelgiumRemote = "http://files.itinero.tech/data/OSM/planet/europe/belgium-latest.osm.pbf";
        public static string BelgiumLocal = "belgium-latest.osm.pbf";

        public static string NMBSSource = "http://files.itinero.tech/data/GTFS/nmbs/nmbs-latest.zip";

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
        public static void DownloadNMBS()
        {
            if (!Directory.Exists("NMBS"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.NMBSSource, "NMBS.zip");
                ZipFile.ExtractToDirectory("NMBS.zip", "NMBS");
                File.Delete("NMBS.zip");
            }
        }
    }
}