using System;
using System.IO;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class TrackingStreamTests
{
    [Test]
    public void Test_Constructor()
    {
        (new TrackingStream(new MemoryStream(), 2)).Position.Should().Be(2);
    }

    private static object[] TrackingStreamNoWrapSources =
    {
        new MemoryStream(),
        new TrackingStream(new MemoryStream())
    };

    [TestCaseSource(nameof(TrackingStreamNoWrapSources))]
    public void Test_Wrap_ShouldNotWrap(Stream stream)
    {
        TrackingStream.Wrap(stream).Should().BeSameAs(stream);
    }

    [Test]
    public void Test_Wrap_ShouldWrap()
    {
        TrackingStream
            .Wrap(new NonSeekableStream(new MemoryStream()))
            .Should()
            .BeOfType<TrackingStream>();
    }

    [Test]
    public void Test_Flush()
    {
        var stream = A.Fake<Stream>();

        (new TrackingStream(stream)).Flush();

        A.CallTo(() => stream.Flush()).MustHaveHappened();
    }

    [Test]
    public void Test_Read()
    {
        var stream = A.Fake<Stream>();

        var callToRead = () =>
            A.CallTo(() => stream.Read(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored));
        callToRead().Returns(4);

        var buffer = new byte[] { 0, 0 };
        var trackingStream = new TrackingStream(stream, 11);
        trackingStream.Position.Should().Be(11);
        var bytesRead = trackingStream.Read(buffer, 0, 0);

        callToRead().MustHaveHappened();

        bytesRead.Should().Be(4, "Should return bytes read");
        trackingStream.Position.Should().Be(15);
    }

    [Test]
    public void Test_Write()
    {
        var stream = A.Fake<Stream>();

        var callToWrite = () =>
            A.CallTo(() => stream.Write(A<byte[]>.Ignored, A<int>.Ignored, A<int>.Ignored));

        var buffer = new byte[] { 0, 0 };
        var trackingStream = new TrackingStream(stream, 11);
        trackingStream.Position.Should().Be(11);
        trackingStream.Write(buffer, 0, 4);

        callToWrite().MustHaveHappened();

        trackingStream.Position.Should().Be(15);
    }

    [Test]
    public void Test_Seek()
    {
        var stream = A.Fake<Stream>();

        var callToSeek = () => A.CallTo(() => stream.Seek(A<long>.Ignored, A<SeekOrigin>.Ignored));
        callToSeek().Returns(20);

        var buffer = new byte[] { 0, 0 };
        var trackingStream = new TrackingStream(stream, 11);
        trackingStream.Position.Should().Be(11);
        var newPosition = trackingStream.Seek(99, SeekOrigin.Begin);

        callToSeek().MustHaveHappened();

        newPosition.Should().Be(20, "Should return new position");
        trackingStream.Position.Should().Be(20);
    }

    [Test]
    public void Test_SetLength()
    {
        var stream = A.Fake<Stream>();

        var callToSetLength = () => A.CallTo(() => stream.SetLength(A<long>.Ignored));

        var trackingStream = new TrackingStream(stream);
        trackingStream.SetLength(99);

        callToSetLength().MustHaveHappened();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Test_CanWrite(bool canWrite)
    {
        var stream = A.Fake<Stream>();

        var callToCanWrite = () => A.CallTo(() => stream.CanWrite);
        callToCanWrite().Returns(canWrite);

        var trackingStream = new TrackingStream(stream);
        trackingStream.CanWrite.Should().Be(canWrite);

        callToCanWrite().MustHaveHappened();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-100)]
    [TestCase(100)]
    public void Test_Position(long position)
    {
        var stream = A.Fake<Stream>();

        var callToPosition = () => A.CallTo(() => stream.Position);
        callToPosition().Returns(position);

        var trackingStream = new TrackingStream(stream, 666);
        trackingStream.Position.Should().Be(666);

        callToPosition().MustNotHaveHappened();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-100)]
    [TestCase(100)]
    public void Test_Length(long length)
    {
        var stream = A.Fake<Stream>();

        var callToLength = () => A.CallTo(() => stream.Length);
        callToLength().Returns(length);

        var trackingStream = new TrackingStream(stream);
        trackingStream.Length.Should().Be(length);

        callToLength().MustHaveHappened();
    }

    [Test]
    public void Test_Position_Set()
    {
        var call = () => new TrackingStream(new MemoryStream()).Position = 10;

        call.Should().Throw<NotSupportedException>().WithMessage("Seeking is not supported.");
    }
}
