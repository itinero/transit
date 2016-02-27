using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Itinero.Transit.Test.Functional.Staging
{
    public static class Download
    {
        public static string BelgiumAllSource = "ftp://build.osmsharp.com/data/OSM/routing/develop/belgium.a.routing.zip";

        public static string NMBSSource = "ftp://build.osmsharp.com/data/GTFS/NMBS/irail-2016-20160105.zip";

        /// <summary>
        /// Downloads the belgium routing file.
        /// </summary>
        public static void DownloadBelgiumAll()
        {
            if (!File.Exists("belgium.a.routing"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumAllSource,
                    "belgium.a.routing.zip");
                ZipFile.ExtractToDirectory("belgium.a.routing.zip", ".");
                File.Delete("belgium.a.routing.zip");
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
                client.DownloadFile(Download.NMBSSource, "NMBS.zip");
                ZipFile.ExtractToDirectory("NMBS.zip", "NMBS");
                File.Delete("NMBS.zip");
            }
        }
    }
}
