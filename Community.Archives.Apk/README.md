# Community.Archives.Apk

A fast and efficient forward-only reader for `Apk` packages (used by Android). 

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

`Apk` files are normal zip-files with compiled java/kotlin and resource files. `ApkArchiveReader` will return all files unchanged.
For extracted metadata, the files `AndroidManifest.xml` and `resources.arsc` are extracted to memory, decompiled and metdata extracted from them.

## Getting started

### Extract all or specific files

```csharp
var reader = new ApkArchiveReader();

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
var reader = new ApkArchiveReader();

// use regular expression to match files (path + file name)
await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, "[.]png$", "[.]xml$")
) {
    // found a (binary) xml or png file
}
```

### Decompile compiled (binary) xml files like `AndroidManifest.xml`

```
Stream stream = ...
IAndroidBinaryXmlReader reader = new AndroidBinaryXmlReader();

XDocument document = reader.Read(stream);

// document represent the raw xml document
```

> ! **NOTE**: To dereference values, you need `resources.arsc`!

### Dereference values from `AndroidManifest.xml`

```
Stream stream = ...
IApkResourceDecoder reader = new ApkResourceDecoder();

IDictionary<string, IList<string?>> references = await reader.DecodeAsync(stream);

// Map of references to actual value list
```

