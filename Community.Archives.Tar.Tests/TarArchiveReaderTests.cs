using System.Threading.Tasks;
using Community.Archives.Core.Tests;
using NUnit.Framework;

namespace Community.Archives.Tar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public class TarArchiveReaderTests : ArchiveReaderTests<TarArchiveReader>
{
    [Test]
    [TestCase("Fixtures/archive.tar", "Fixtures/archive.tar.csv")]
    [TestCase("Fixtures/archive.tar.gz", "Fixtures/archive.tar.csv")]
    [TestCase("Fixtures/archive.tar.xz", "Fixtures/archive.tar.csv")]
    [TestCase("Fixtures/archive.tar.lz", "Fixtures/archive.tar.csv")]
    [TestCase("Fixtures/archive.tar.bz2", "Fixtures/archive.tar.csv")]
    public async Task Test_GetEntriesAsync_ShouldExtractAllFiles(
        string archivePath,
        string inventoryPath
    )
    {
        using (var archive = new StreamFixtureFile(archivePath))
        {
            var inventory = new ArchiveInventoryFixtureFile(inventoryPath, "./");

            await AssertAllFilesAreExtracted(archive.Content, inventory);
        }
    }

    [Test]
    public void Test_GetMetaDataAsync_ShouldFailNotSupported()
    {
        AssertMetaDataNotSupported();
    }
}
