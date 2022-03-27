using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

[ExcludeFromCodeCoverage]
public class ArchiveInventoryFixtureFile
{
    public readonly struct FileEntry
    {
        public string Hash { get; init; }
        public string FilePath { get; init; }
    }

    public ArchiveInventoryFixtureFile()
    {
        Files = Array.Empty<FileEntry>();
    }

    public ArchiveInventoryFixtureFile(string filePath, string pathPrefix = "") : this()
    {
        Load(filePath, pathPrefix);
    }

    public IReadOnlyList<FileEntry> Files { get; private set; }

    public void Load(string path, string pathPrefix = "")
    {
        string csv = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, path));

        Files = csv.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(
                (line) =>
                {
                    var parts = line.Trim().Split(';', StringSplitOptions.RemoveEmptyEntries);
                    return new FileEntry()
                    {
                        Hash = parts[0].Trim(),
                        FilePath = $"{pathPrefix}{parts[1].Trim()}"
                    };
                }
            )
            .ToList();
    }
}
