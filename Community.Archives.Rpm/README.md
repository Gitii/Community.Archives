# Community.Archives.Rpm

A fast and efficient forward-only reader for `Rpm` archives. 

* :rocket: Fast and efficient: Only extracts matched file. Forward-only access. Uses `Task` to offload `IO` to separate threads.
* :grinning: Licensed under MIT. Similar projects are licensed under GPL.
* :heart_eyes: 100% test coverage 

This package is part of [Gitii/Community.Archives: A collection of libraries that support reading various popular archives.](https://github.com/Gitii/Community.Archives)

## Supported frameworks

- .Net Standard 2.1
- .Net 5
- .Net 6

On any platform that's supported by the above frameworks, including Windows, Linux and MacOS.

## Reader specifications

`RpmArchiveReader` is implemented based on [Cap. 24. RPM Package File Structure](https://docs.fedoraproject.org/ro/Fedora_Draft_Documentation/0.1/html/RPM_Guide/ch-package-structure.html).

* Only a subset of all metadata is extracted.

* A `rpm` file contains an `cpio` archive. It contains all files. `RpmArchiveReader` uses a `CpioArchiveReader` under the hood to extract the files.

## Getting started

### Extract all or specific files

```csharp
var reader = new RpmArchiveReader();

await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
) {
    // entry.Name
    // entry.Content
    Console.WriteLine($"Found file {entry.Name} ({entry.Content.Length} bytes)")
}
```

### Extract specific files only

```csharp
var reader = new RpmArchiveReader();

// use regular expression to match files (path + file name)
await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, "[.]md$", "[.]txt$")
) {
    // found a Markdown or text file
}
```

### Extract metadata of the archive

```csharp
var reader = new RpmArchiveReader();

var metaData = await reader.GetMetaDataAsync(stream);

Console.WriteLine(metaData.Package); // for example: "gh"
Console.WriteLine(metaData.Version); // for example: "2.4.0"
```
