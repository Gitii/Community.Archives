using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests
{
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

        private static async Task<string> GetHash(HashAlgorithm hashAlgorithm, Stream input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = await hashAlgorithm.ComputeHashAsync(input).ConfigureAwait(false);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public void AssertMetaDataNotSupported()
        {
            IArchiveReader reader = new T();

            reader.SupportsMetaData
                .Should()
                .BeFalse("Should return false because Metadata is not supported");
            var call = () => reader.GetMetaDataAsync(Stream.Null);

            call.Should().ThrowAsync<Exception>("Should throw because Metadata is not supported");
        }
    }
}
