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