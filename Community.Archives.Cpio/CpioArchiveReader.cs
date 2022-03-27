using Community.Archives.Core;

namespace Community.Archives.Cpio;

public class CpioArchiveReader : IArchiveReader
{
    public virtual async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
        Stream stream,
        params string[] regexMatcher
    )
    {
        stream = TrackingStream.Wrap(stream);

        bool endOfFile = false;
        do
        {
            var header = await stream.ReadStructAsync<Header>().ConfigureAwait(false);
            if (!header.IsValid())
            {
                throw new Exception("The stream is not a valid cpio archive!");
            }

            string fileName = await stream
                .ReadFixedLengthAnsiStringAsync(header.c_namesize.DecodeStringAsLong(true))
                .ConfigureAwait(false);

            fileName = fileName.TrimEnd('\0'); // length of name is known but it's still NUL-terminated.

            if (fileName != "TRAILER!!!")
            {
                var mode = header.GetFileMode();
                await stream.AlignToBoundaryAsync(4).ConfigureAwait(false); // 0-3 bytes as needed to align the file stream to a 4 byte boundary.

                var fileSize = header.c_filesize.DecodeStringAsLong(true);

                if (mode == FileMode.FILE && regexMatcher.IsMatch(fileName))
                {
                    yield return new ArchiveEntry()
                    {
                        Content = await stream.CopyRangeToAsync(fileSize).ConfigureAwait(false),
                        Name = fileName
                    };
                }
                else
                {
                    await stream.SkipAsync(fileSize).ConfigureAwait(false);
                }

                await stream.AlignToBoundaryAsync(4).ConfigureAwait(false); // 0-3 bytes as needed to align the file stream to a 4 byte boundary.
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
    public virtual Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        throw new NotSupportedException(
            "A cpio archive consists of a series of file objects and has no global header."
        );
    }

    public virtual bool SupportsMetaData { get; } = false;
}
