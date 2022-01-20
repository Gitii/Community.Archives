# Community.Archives.Ar

A fast and efficient forward-only reader for `Ar` archives. 

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

`ArArchiveReader` is implemented based on [BSD File Formats Manual](https://www.freebsd.org/cgi/man.cgi?query=ar&sektion=5).

* Only `SVR4/GNU` format is supported

* `Ar` files do not have metadata. `GetMetaDataAsync` will throw an exception at runtime.

## Getting started

### Extract all or specific files

```csharp
var reader = new ArArchiveReader();

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
var reader = new ArArchiveReader();

// use regular expression to match files (path + file name)
await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, "[.]md$", "[.]txt$")
) {
    // found a Markdown or text file
}
```


