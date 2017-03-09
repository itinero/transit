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

using GTFS;
using GTFS.IO;
using Itinero.Transit.Data;
using Itinero.Transit.GTFS;
using System;
using System.IO;

namespace Itinero.Transit.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests for transit db building.
    /// </summary>
    public static class TransitDbBuilder
    {
        /// <summary>
        /// Builds a transit db from a GTFS folder.
        /// </summary>
        public static TransitDb Run(string gtfsFolder)
        {
            return TransitDbBuilder.GetTestBuildTransitDb(gtfsFolder).TestPerf(
                string.Format("Testing loading GTFS: {0}", gtfsFolder));
        }

        /// <summary>
        /// Builds a transitdb from a GTFS or loads it if it's there already.
        /// </summary>
        public static TransitDb RunOrLoad(string gtfsFolder)
        {
            TransitDb transitDb = null;
            var transitDbFile = gtfsFolder + ".transitdb";
            if (File.Exists(transitDbFile))
            {
                try
                {
                    using (var stream = File.OpenRead(transitDbFile))
                    {
                        transitDb = TransitDb.Deserialize(stream);
                    }
                    Itinero.Logging.Logger.Log("TransitDbBuilder", Itinero.Logging.TraceEventType.Warning, "Existing TransitDb found, not rebuilding, delete file to retest.");
                }
                catch
                {
                    Itinero.Logging.Logger.Log("TransitDbBuilder", Itinero.Logging.TraceEventType.Warning, "Invalid existing TransitDb file, could not load file.");
                    transitDb = null;
                }
            }

            if (transitDb == null)
            {
                transitDb = TransitDbBuilder.Run(gtfsFolder);

                using (var stream = File.Open(transitDbFile, FileMode.Create))
                {
                    transitDb.Serialize(stream);
                }
            }
            return transitDb;
        }

        /// <summary>
        /// Merges this given transit db's into one.
        /// </summary>
        public static TransitDb Merge(params TransitDb[] transitDbs)
        {
            return TransitDbBuilder.GetTestMergeTransitDbs(transitDbs).TestPerf(
                "Testing merging all transit db's.");
        }

        /// <summary>
        /// Tests building a transitdb.
        /// </summary>
        public static Func<TransitDb> GetTestBuildTransitDb(string gtfsFolder)
        {
            var reader = new GTFSReader<GTFSFeed>(false);
            return () =>
            {
                var feed = reader.Read(new GTFSDirectorySource(gtfsFolder));
                var transitDb = new TransitDb();
                transitDb.LoadFrom(feed);
                transitDb.SortConnections(DefaultSorting.DepartureTime, null);
                transitDb.SortStops();
                return transitDb;
            };
        }

        /// <summary>
        /// Tests merge transitdbs.
        /// </summary>
        public static Func<TransitDb> GetTestMergeTransitDbs(TransitDb[] transitDbs)
        {
            return () =>
            {
                var transitDb = new TransitDb();
                for(var i = 0; i < transitDbs.Length;i++)
                {
                    transitDb.CopyFrom(transitDbs[i]);
                }
                transitDb.SortConnections(DefaultSorting.DepartureTime, null);
                transitDb.SortStops();
                return transitDb;
            };
        }
    }
}