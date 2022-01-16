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

        Files = csv.ReplaceLineEndings()
            .Split(
                Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
            .Select(
                (line) =>
                {
                    var parts = line.Split(
                        ';',
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                    );
                    return new FileEntry() { Hash = parts[0], FilePath = $"{pathPrefix}{parts[1]}" };
                }
            )
            .ToList();
    }
}
