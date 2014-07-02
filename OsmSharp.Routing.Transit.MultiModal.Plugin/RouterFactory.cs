using GTFS;
using GTFS.IO;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Graphs;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams.Graphs;
using OsmSharp.Routing.Transit.MultiModal.RouteCalculators;
using OsmSharp.Routing.Transit.MultiModal.Routers;
using OsmSharp.Service.Routing;
using OsmSharp.Service.Routing.Core;
using System.Configuration;
using System.IO;

namespace OsmSharp.Routing.Transit.MultiModal.Plugin
{
    /// <summary>
    /// A router factory to create a custom router.
    /// </summary>
    public class RouterFactory : RouterFactoryBase
    {
        /// <summary>
        /// Holds the memory data.
        /// </summary>
        private TypedRouterMultiModal _typeRouterMultiModal;

        /// <summary>
        /// Initializes this factory.
        /// </summary>
        public override void Initialize()
        {
            // build datareader.
            using (var fileStream = new FileInfo(ConfigurationManager.AppSettings["osm.pbf"].ToString()).OpenRead())
            {
                // create reader.
                var reader = new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(fileStream);

                // creates a tagged index.
                var tagsIndex = new TagsTableCollectionIndex(); 

                // read from the OSM-stream.
                var interpreter = new OsmRoutingInterpreter();
                var data = new MultiModalGraphRouterDataSource(tagsIndex);
                var targetData = new LiveGraphOsmStreamTarget(data, interpreter, tagsIndex);
                targetData.RegisterSource(reader);
                targetData.Pull();

                // creates the live edge router.
                var multiModalEdgeRouter = new TypedRouterMultiModal(
                    data, interpreter, new ReferenceCalculator());

                // read all GTFS feeds.
                var gftsReader = new GTFSReader<GTFSFeed>(false);
                var gtfsPaths = ConfigurationManager.AppSettings["gtfs.paths"].ToString();
                var gtfsPathArray = gtfsPaths.Split(';');
                for(int idx = 0; idx < gtfsPathArray.Length; idx++)
                {
                    // parse feed.
                    gftsReader.DateTimeReader = (dateString) =>
                    {
                        var year = int.Parse(dateString.Substring(0, 4));
                        var month = int.Parse(dateString.Substring(4, 2));
                        var day = int.Parse(dateString.Substring(6, 2));
                        return new System.DateTime(year, month, day);
                    };
                    var feed = gftsReader.Read(new GTFSDirectorySource(gtfsPathArray[idx]));

                    // add feed.
                    multiModalEdgeRouter.AddGTFSFeed(feed);

                    _typeRouterMultiModal = multiModalEdgeRouter;
                }
            }
        }

        /// <summary>
        /// Creates a router.
        /// </summary>
        /// <returns></returns>
        public override IPluggedInRouter CreateRouter()
        {
            return new MultiModalPluggedInRouter(new MultiModalRouter(_typeRouterMultiModal));
        }

        /// <summary>
        /// Returns true if this factory is ready.
        /// </summary>
        public override bool IsReady
        {
            get { return _typeRouterMultiModal != null; }
        }
    }
}