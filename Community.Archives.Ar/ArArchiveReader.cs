using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Community.Archives.Core;

namespace Community.Archives.Ar;

/// <summary>
/// Implementation of the Ar Archive format
/// See: https://en.wikipedia.org/wiki/Deb_(file_format)#/media/File:Deb_File_Structure.svg
///
/// Source: https://github.com/microsoft/RecursiveExtractor/blob/main/RecursiveExtractor/DebArchiveFile.cs
/// </summary>
public class ArArchiveReader : IArchiveReader
{
    /// <summary>
    /// Enumerate the FileEntries in the given archive asynchronously
    /// </summary>
    /// <param name="stream">The archive file stream</param>
    /// <returns>The ArchiveEntry found</returns>
    public virtual async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
        Stream stream,
        params string[] regexMatcher
    )
    {
        Regex[] regexes = regexMatcher
            .Select((rm) => new Regex(rm, RegexOptions.None, TimeSpan.FromSeconds(1)))
            .ToArray();

        var header = await stream.ReadStructAsync<Header>().ConfigureAwait(false);
        if (!header.IsValid())
        {
            throw new Exception("The stream is not a valid ar archive!");
        }

        var size = Marshal.SizeOf<FileEntry>();

        ArchiveStringTable ast = new ArchiveStringTable();

        while (stream.Length - stream.Position >= size)
        {
            var fileEntry = await stream.ReadStructAsync<FileEntry>().ConfigureAwait(false);

            var filename = fileEntry.FileIdentifier.AsString().TrimEnd();
            if (!filename.StartsWith('/') && filename.EndsWith('/'))
            {
                filename = filename.TrimEnd('/'); // trim / if SVR4/GNU variant & regular file
            }

            filename = ast.Dereference(filename);

            var fileSize = fileEntry.FileSize.DecodeStringAsLong();

            if (filename == "//")
            {
                // This file is a "Archive String Table"
                // see https://www.freebsd.org/cgi/man.cgi?query=ar&sektion=5
                var astData = await stream.ReadBlockAsync(fileSize).ConfigureAwait(false);

                ast.Load(astData);
            }
            else if (regexes.IsMatch(filename))
            {
                // match: read bytes
                yield return new ArchiveEntry()
                {
                    Content = await stream.ReadStreamAsync(fileSize).ConfigureAwait(false), Name = filename,
                };
            }
            else
            {
                // no match: skip
                await stream.SkipAsync(fileSize).ConfigureAwait(false);
            }
        }
    }

    public virtual async Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        var header = await stream.ReadStructAsync<Header>().ConfigureAwait(false);
        if (!header.IsValid())
        {
            throw new Exception("The stream is not a valid ar archive!");
        }

        return new IArchiveReader.ArchiveMetaData()
        {
            Package = String.Empty,
            Version = String.Empty,
            Architecture = string.Empty,
            Description = string.Empty,
            AllFields = new Dictionary<string, string>(StringComparer.Ordinal),
        };
    }

    public virtual bool SupportsMetaData { get; } = true;
}
