# Community.Archives

A collection of libraries that support fast and efficient forward-only reading of various popular archives. 

## Supported archive formats

|      | Package                                                                                        | Documentation                                                                                |
| ---- | ---------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| Ar   | [Get `Community.Archives.Ar` on nuget](https://www.nuget.org/packages/Community.Archives.Ar)   | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Ar)   |
| Cpio | [Get `Community.Archives.Cpio` on nuget](https://www.nuget.org/packages/Community.Archives.Ar) | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Cpio) |
| Rpm  | [Get `Community.Archives.Rpm` on nuget](https://www.nuget.org/packages/Community.Archives.Ar)  | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Rpm)  |
| Tar  | [Get `Community.Archives.Tar` on nuget](https://www.nuget.org/packages/Community.Archives.Ar)  | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Tar)  |

## Supported frameworks

- .NET Standard 2.1
- .Net 5
- .Net 6

On any platform that's supported by the above frameworks, including Windows, Linux and MacOS.

## Getting started

Each package exports an implementation of [`IArchiveReader`]([Community.Archives/IArchiveReader.cs at main · Gitii/Community.Archives · GitHub](https://github.com/Gitii/Community.Archives/blob/main/Community.Archives.Core/IArchiveReader.cs)).

## Extract all or specific files

```csharp
var reader = new TarArchiveReader(); // or RpmArchiveReader or ...

await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
) {
    // entry.Name
    // entry.Content
    Console.WriteLine($"Found file {entry.Name} ({entry.Content.Length} bytes)")
}
```

## Extract specific files only

```csharp
var reader = new TarArchiveReader(); // or RpmArchiveReader or ...

// use regular expression to match files (path + file name)
await foreach (
    var entry in reader
        .GetFileEntriesAsync(stream, "[.]md$", "[.]txt$")
) {
    // found a Markdown or text file
}
```

## Extract metadata of the archive

```csharp
var reader = new RpmArchiveReader();

var metaData = await reader.GetMetaDataAsync(stream);

Console.WriteLine(metaData.Package); // for example: "gh"
Console.WriteLine(metaData.Version); // for example: "2.4.0"
```

> :exclamation: Only `rpm` archives contain meta data. Check `reader.SupportsMetaData` at runtime or the documentation of the reader before using it.

# Recommended usage

The implementations of `IArchiveReader` allow forward-only access of supported archives.

But why forward-only and not random-access?

All of these archive formats do not have an central index of files. That means that (in worst case) the complete archive needs to be scanned to find a file. In addition, archives like `tar` are usually compressed. Decompressing them is easy but because the `tar` archive as a whole and not individual files are compressed, the whole file needs to be decompressed for random-access.

There are many different archive extractors (for example `7z`) that can easily extract any modern archive.

The purpose of  `IArchiveReader` is to quickly and efficiently find and extract one or more files. Without using native or *fat* dependencies like [RecursiveExtractor]([RecursiveExtractor](https://github.com/microsoft/RecursiveExtractor)) or [SharpZipLib](https://github.com/icsharpcode/SharpZipLib). 

`IArchiveReader` will only allocate memory (`byte[]`) for matched files. 

# Found a bug?

Please [create an issue](https://github.com/Gitii/Community.Archives/issues) and attach the file (if it's not confidental or contains personally identifiable information (PII))
