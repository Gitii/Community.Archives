using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.Archives.Core;
using Community.Archives.Core.Tests;
using NUnit.Framework;

namespace Community.Archives.Ar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public class ArArchiveReaderTests : ArchiveReaderTests<ArArchiveReader>
{
    [Test]
    public async Task Test_GetMetaDataAsync()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.ar"))
        {
            await AssertMetaDataSupported(
                archive.Content,
                new IArchiveReader.ArchiveMetaData()
                {
                    Package = String.Empty,
                    Version = String.Empty,
                    Architecture = string.Empty,
                    Description = string.Empty,
                    AllFields = new Dictionary<string, string>(StringComparer.Ordinal),
                }
            );
        }
    }

    [Test]
    [TestCase("Fixtures/archive.ar", "Fixtures/archive.ar.csv")]
    [TestCase("Fixtures/archive2.ar", "Fixtures/archive2.ar.csv")]
    public async Task Test_GetEntriesAsync_ShouldExtractAllFiles(
        string archivePath,
        string inventoryPath
    )
    {
        using (var archive = new StreamFixtureFile(archivePath))
        {
            var inventory = new ArchiveInventoryFixtureFile(inventoryPath);
            await AssertAllFilesAreExtracted(archive.Content, inventory);
        }
    }

    [Test]
    public async Task Test_GetEntriesAsync_ShouldExtractFirstFile()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.ar"))
        {
            var inventory = new ArchiveInventoryFixtureFile("Fixtures/archive.ar.csv");
            await AssertFirstFileIsExtracted(archive.Content, inventory, "large.bin");
        }
    }

    [Test]
    public Task Test_GetEntriesAsync_InvalidHeader()
    {
        return AssertInvalidHeader("foobar!!!!!!!!", "The stream is not a valid ar archive!");
    }
}
