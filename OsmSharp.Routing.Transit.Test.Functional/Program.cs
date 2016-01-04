using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            // download test data.
            var client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri("ftp://build.osmsharp.com/data/OSM/routing/develop/belgium.a.routing.zip"), 
                "belgium.a.routing.zip");

            Console.ReadLine();
        }

        static void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            File.Delete("belgium.a.routing");
            ZipFile.ExtractToDirectory("belgium.a.routing.zip", ".");
            Console.WriteLine("Belgium downloaded and extracted.");
        }

        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write(".");
        }
    }
}
