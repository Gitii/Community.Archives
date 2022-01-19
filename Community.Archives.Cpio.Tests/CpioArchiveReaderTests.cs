using System.Threading.Tasks;
using Community.Archives.Core.Tests;
using NUnit.Framework;

namespace Community.Archives.Cpio.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public class CpioArchiveReaderTests : ArchiveReaderTests<CpioArchiveReader>
{
    [Test]
    public Task Test_GetMetaDataAsync()
    {
        return AssertMetaDataNotSupported();
    }

    [Test]
    public async Task Test_GetEntriesAsync_ShouldExtractAllFiles()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.cpio"))
        {
            var inventory = new ArchiveInventoryFixtureFile("Fixtures/archive.cpio.csv");
            await AssertAllFilesAreExtracted(archive.Content, inventory);
        }
    }

    [Test]
    public async Task Test_GetEntriesAsync_ShouldExtractFirstFile()
    {
        using (var archive = new StreamFixtureFile("Fixtures/archive.cpio"))
        {
            var inventory = new ArchiveInventoryFixtureFile("Fixtures/archive.cpio.csv");
            await AssertFirstFileIsExtracted(archive.Content, inventory, "large.bin");
        }
    }

    [Test]
    public Task Test_GetEntriesAsync_InvalidHeader()
    {
        return AssertInvalidHeader(120, "The stream is not a valid cpio archive!", true);
    }
}
