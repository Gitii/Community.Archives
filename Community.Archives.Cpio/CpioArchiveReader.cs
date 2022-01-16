﻿using Community.Archives.Core;

namespace Community.Archives.Cpio;

public class CpioArchiveReader : IArchiveReader
{
    public async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
        Stream stream,
        params string[] regexMatcher
    )
    {
        bool endOfFile = false;
        do
        {
            var header = await stream.ReadStructAsync<Header>().ConfigureAwait(false);
            string fileName = await stream
                .ReadFixedLengthAnsiStringAsync(header.c_filesize.DecodeStringAsLong())
                .ConfigureAwait(false);

            if (fileName != "TRAILER!!!")
            {
                await stream.AlignToBoundaryAsync(4)
                    .ConfigureAwait(false); // 0-3 bytes as needed to align the file stream to a 4 byte boundary.
                if (regexMatcher.IsMatch(fileName))
                {
                    yield return new ArchiveEntry()
                    {
                        Content = await stream
                            .CopyRangeToAsync(header.c_filesize.DecodeStringAsLong(true))
                            .ConfigureAwait(false),
                        Name = fileName
                    };
                }
                else
                {
                    await stream
                        .SkipAsync(header.c_filesize.DecodeStringAsLong(true))
                        .ConfigureAwait(false);
                }

                await stream.AlignToBoundaryAsync(4)
                    .ConfigureAwait(false); // 0-3 bytes as needed to align the file stream to a 4 byte boundary.
            }
            else
            {
                endOfFile = true;
            }
        } while (!endOfFile);
    }

    /// <summary>
    /// Always throws an exception because a cpio archive consists of a series of file objects and has no header.
    /// </summary>
    /// <param name="stream">The stream to extract the header from</param>
    /// <returns>Not specified</returns>
    /// <exception cref="NotSupportedException">A cpio archive consists of a series of file objects and has no header.</exception>
    public Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        throw new NotSupportedException(
            "A cpio archive consists of a series of file objects and has no header."
        );
    }

    public bool SupportsMetaData { get; } = false;
}
