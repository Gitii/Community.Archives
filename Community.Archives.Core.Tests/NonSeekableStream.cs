using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Community.Archives.Core.Tests;

[ExcludeFromCodeCoverage]
class NonSeekableStream : Stream
{
    private readonly Stream _stream;

    public NonSeekableStream(Stream baseStream)
    {
        _stream = baseStream;
    }

    public override bool CanRead
    {
        get { return _stream.CanRead; }
    }

    public override bool CanSeek
    {
        get { return false; }
    }

    public override bool CanWrite
    {
        get { return _stream.CanWrite; }
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override long Length
    {
        get { throw new NotSupportedException(); }
    }

    public override long Position
    {
        get { return _stream.Position; }
        set { throw new NotSupportedException(); }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(buffer, offset, count);
    }
}
