using System.IO;
using System.IO.Compression;
using System.Net;

namespace Itinero.Transit.Test.Functional.Staging
{
    public static class Download
    {
        public static string BelgiumAllSource = "http://files.itinero.tech/data/itinero/routerdbs/planet/europe/belgium.a.routerdb";

        public static string NMBSSource = "http://files.itinero.tech/data/GTFS/nmbs/nmbs-latest.zip";

        /// <summary>
        /// Downloads the belgium routing file.
        /// </summary>
        public static void DownloadBelgiumAll()
        {
            if (!File.Exists("belgium.a.routerdb"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumAllSource,
                    "belgium.a.routerdb");
            }
        }

        /// <summary>
        /// Downloads the NMBS test feed.
        /// </summary>
        public static void DownloadNMBS()
        {
            if(!Directory.Exists("NMBS"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.NMBSSource, "nmbs-latest.zip");
                ZipFile.ExtractToDirectory("nmbs-latest.zip", "NMBS");
                File.Delete("nmbs-latest.zip");
            }
        }
    }
}