using System.Collections.Generic;
using System.Threading.Tasks;
using Community.Archives.Core;
using Community.Archives.Core.Tests;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public class RpmArchiveReaderTests : ArchiveReaderTests<RpmArchiveReader>
{
    [Test]
    public async Task Test_GetMetaDataAsync()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.rpm"))
        {
            await AssertMetaDataSupported(
                archive.Content,
                new IArchiveReader.ArchiveMetaData()
                {
                    Architecture = "x86_64",
                    Description = "GitHub’s official command line tool.",
                    Package = "gh-2.4.0-1",
                    Version = "2.4.0",
                    AllFields = new Dictionary<string, string>()
                    {
                        ["SignatureTagSize"] = "0",
                        ["SignatureTagPayloadSize"] = "0",
                        ["Name"] = "gh",
                        ["Version"] = "2.4.0",
                        ["Release"] = "1",
                        ["Summary"] = "GitHub's official command line tool.",
                        ["Description"] = "GitHub's official command line tool.",
                        ["Size"] = "0",
                        ["License"] = "MIT",
                        ["Packager"] = "GitHub",
                        ["Url"] = "https://github.com/cli/cli",
                        ["Os"] = "linux",
                        ["Architecture"] = "x86_64",
                        ["SourceRpm"] = "gh-2.4.0-1.src.rpm",
                        ["ArchiveSize"] = "0",
                        ["PayloadFormat"] = "cpio",
                        ["PayloadCompressor"] = "gzip",
                        ["PayloadFlags"] = "9"
                    }
                }
            );
        }
    }

    [Test]
    public async Task Test_GetEntriesAsync_ShouldExtractAllFiles()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.rpm"))
        {
            var inventory = new ArchiveInventoryFixtureFile("Fixtures/archive.rpm.csv");
            await AssertAllFilesAreExtracted(archive.Content, inventory);
        }
    }

    [Test]
    public async Task Test_GetEntriesAsync_ShouldExtractFirstFile()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.rpm"))
        {
            var inventory = new ArchiveInventoryFixtureFile("Fixtures/archive.rpm.csv");
            await AssertFirstFileIsExtracted(
                archive.Content,
                inventory,
                "/usr/share/man/man1/gh-repo-fork.1"
            );
        }
    }

    [Test]
    public Task Test_GetEntriesAsync_InvalidHeader()
    {
        return AssertInvalidHeader(120, "The stream is not a valid rpm archive!", true);
    }
}
