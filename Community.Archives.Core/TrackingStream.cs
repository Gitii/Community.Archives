namespace Community.Archives.Core;

/// <summary>
/// Simple wrapper for a <seealso cref="Stream"/> that tracks the read bytes and
/// reports that in <seealso cref="Stream.Position"/>.
/// </summary>
public class TrackingStream : Stream
{
    private readonly Stream _stream;
    private long _position;

    public TrackingStream(Stream stream, long position = 0)
    {
        _stream = stream;
        _position = position;
    }

    /// <summary>
    /// Wraps any stream in a <seealso cref="TrackingStream"/>.
    /// If <paramref name="stream"/> is already of type <seealso cref="TrackingStream"/> or <seealso cref="Stream.CanSeek"/> is <c>true</c>, the same
    /// instance is returned.
    /// </summary>
    /// <param name="stream">The stream that will be wrapped.</param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Stream Wrap(Stream stream, long position = 0)
    {
        if (stream is TrackingStream)
        {
            return stream;
        }

        if (stream.CanSeek)
        {
            return stream;
        }

        return new TrackingStream(stream, position);
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _stream.Read(buffer, offset, count);
        _position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _position = _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
        _position += count;
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException("Seeking is not supported.");
    }
}
