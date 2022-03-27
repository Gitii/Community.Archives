using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

[TestFixture]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public abstract class ArchiveReaderTests<T> where T : IArchiveReader, new()
{
    public async Task AssertAllFilesAreExtracted(
        Stream stream,
        ArchiveInventoryFixtureFile expectedArchive
    )
    {
        var hasher = SHA256.Create();

        IArchiveReader reader = new T();
        int counter = 0;
        await foreach (
            var actualEntry in reader
                .GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
                .ConfigureAwait(false)
        )
        {
            counter += 1;
            var expectedFileEntry = expectedArchive.Files.FirstOrNullable(
                (fe) => fe.FilePath == actualEntry.Name
            );

            if (!expectedFileEntry.HasValue)
            {
                throw new Exception($"Unexpected file found: {actualEntry}");
            }

            var hash = await GetHash(hasher, actualEntry.Content).ConfigureAwait(false);

            hash.Should()
                .Be(expectedFileEntry.Value.Hash, $"Hash mismatch for file {actualEntry.Name}");
        }

        counter
            .Should()
            .Be(
                expectedArchive.Files.Count,
                $"Number of files in the archive must be {expectedArchive.Files.Count}"
            );
    }

    public async Task AssertFirstFileIsExtracted(
        Stream stream,
        ArchiveInventoryFixtureFile expectedArchive,
        string firstEntryFilePath
    )
    {
        var hasher = SHA256.Create();

        IArchiveReader reader = new T();

        var expectedFileEntry = expectedArchive.Files.First(
            (e) => e.FilePath == firstEntryFilePath
        );
        bool foundEntry = false;
        await foreach (
            var actualEntry in reader
                .GetFileEntriesAsync(stream, $"^({firstEntryFilePath})$")
                .ConfigureAwait(false)
        )
        {
            if (foundEntry)
            {
                throw new Exception(
                    $"Should only find one file but found another one: {actualEntry}"
                );
            }

            actualEntry.Name.Should().Be(expectedFileEntry.FilePath, "Should find this file");

            foundEntry = true;

            var hash = await GetHash(hasher, actualEntry.Content).ConfigureAwait(false);

            hash.Should().Be(expectedFileEntry.Hash, $"Hash mismatch for file {actualEntry.Name}");
        }

        foundEntry.Should().BeTrue("Should find exactly one file in the archive but found none.");
    }

    private static async Task<string> GetHash(HashAlgorithm hashAlgorithm, Stream input)
    {
#if NETCOREAPP3_1_OR_GREATER
#pragma warning disable AsyncFixer02 // Long-running or blocking operations inside an async method
        byte[] data = await Task.FromResult(hashAlgorithm.ComputeHash(input)).ConfigureAwait(false);
#pragma warning restore AsyncFixer02 // Long-running or blocking operations inside an async method
#else
        byte[] data = await hashAlgorithm.ComputeHashAsync(input).ConfigureAwait(false);
#endif
        // Convert the input string to a byte array and compute the hash.


        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    public Task AssertInvalidHeader(
        string invalidHeader,
        string expectedErrorMessage,
        bool skipMetaDataTest = false
    )
    {
        return AssertInvalidHeader(
            Encoding.ASCII.GetBytes(invalidHeader),
            expectedErrorMessage,
            skipMetaDataTest
        );
    }

    public Task AssertInvalidHeader(
        int invalidHeaderLength,
        string expectedErrorMessage,
        bool skipMetaDataTest = false
    )
    {
        return AssertInvalidHeader(
            new byte[invalidHeaderLength],
            expectedErrorMessage,
            skipMetaDataTest
        );
    }

    public async Task AssertInvalidHeader(
        byte[] invalidHeader,
        string expectedErrorMessage,
        bool skipMetaDataTest = false
    )
    {
        if (!skipMetaDataTest)
        {
            await AssertInvalidHeaderGetMetaDataAsync(invalidHeader, expectedErrorMessage)
                .ConfigureAwait(false);
        }

        await AssertInvalidHeaderGetFileEntriesAsync(invalidHeader, expectedErrorMessage)
            .ConfigureAwait(false);
    }

    private static async Task AssertInvalidHeaderGetFileEntriesAsync(
        byte[] invalidHeader,
        string expectedErrorMessage
    )
    {
        IArchiveReader reader = new T();

        var stream = new MemoryStream(invalidHeader, false);

        var call = async () =>
        {
            await foreach (
                var _ in reader
                    .GetFileEntriesAsync(stream, IArchiveReader.MATCH_ALL_FILES)
                    .ConfigureAwait(false)
            )
            {
                throw new Exception(
                    "This code should not be reached because the header is supposed to be invalid"
                );
            }
        };

        var result = await call.Should()
            .ThrowAsync<Exception>("Should throw because header is invalid")
            .ConfigureAwait(false);
        result.WithMessage(expectedErrorMessage);
    }

    private static async Task AssertInvalidHeaderGetMetaDataAsync(
        byte[] invalidHeader,
        string expectedErrorMessage
    )
    {
        IArchiveReader reader = new T();

        var stream = new MemoryStream(invalidHeader, false);

        var call = () => reader.GetMetaDataAsync(stream);

        var result = await call.Should()
            .ThrowAsync<Exception>("Should throw because header is invalid")
            .ConfigureAwait(false);
        result.WithMessage(expectedErrorMessage);
    }

    public Task AssertMetaDataNotSupported()
    {
        IArchiveReader reader = new T();

        reader.SupportsMetaData
            .Should()
            .BeFalse("Should return false because Metadata is not supported");
        var call = () => reader.GetMetaDataAsync(Stream.Null);

        return call.Should()
            .ThrowAsync<Exception>("Should throw because Metadata is not supported");
    }

    public async Task AssertMetaDataSupported(
        Stream archive,
        IArchiveReader.ArchiveMetaData expectedMetaData
    )
    {
        IArchiveReader reader = new T();

        reader.SupportsMetaData.Should().BeTrue("Should return true because Metadata is supported");

        var actualMetaData = await reader.GetMetaDataAsync(archive).ConfigureAwait(false);

        actualMetaData.Should().BeEquivalentTo(expectedMetaData);
    }
}
