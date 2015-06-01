OsmSharp.Transit
================

This projects enables applications based on OsmSharp to add multimodal routing capabilities for Public Transit related applications. It brings togehter the GTFS module and the OsmSharp core.

![dependencies](doc/dependencies.png)

### A small sample

```csharp
// create a routing instance from an OSM PBF file.
var router = MultiModalRouter.CreateFromPBF(@"path\to\somefile.osm.pbf.routing", new OsmRoutingInterpreter());

// read a GTFS file.
var reader = new GTFSReader<GTFSFeed>(false);
var feed = reader.Read(new GTFSDirectorySource(new DirectoryInfo("path/to/feed/directory"))); 

// add feed information to router.

```

### Install

Installing OsmSharp.Transit into one of your projects is easiest using Nuget:

```
PM> Install-Package OsmSharp.Transit
```

