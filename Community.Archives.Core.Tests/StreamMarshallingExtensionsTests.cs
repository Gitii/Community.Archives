using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
public class StreamMarshallingExtensionsTests
{
    [StructLayout(LayoutKind.Sequential)]
    struct TwoByteStruct
    {
        public byte Field1;
        public byte Field2;
    }

    [Test]
    [TestCase(2, 12)]
    [TestCase(4, 12)]
    [TestCase(6, 12)]
    [TestCase(8, 16)]
    public async Task Test_AlignToBoundaryAsync_ShouldSeekToAlignPosition(
        int boundary,
        int expectedPosition
    )
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = 11;
        var seeked = await stream.AlignToBoundaryAsync(boundary);

        seeked.Should().BeTrue();

        stream.Position.Should().Be(expectedPosition);
    }

    [Test]
    [TestCase(2, 12)]
    [TestCase(4, 12)]
    [TestCase(6, 12)]
    [TestCase(8, 16)]
    public async Task Test_AlignToBoundaryAsync_ShouldSeekToAlignPosition_NonSeekableStream(
        int boundary,
        int expectedPosition
    )
    {
        var stream = new MemoryStream(new byte[30], false);
        var nonSeekableStream = new NonSeekableStream(stream);
        stream.Position = 11;
        var seeked = await nonSeekableStream.AlignToBoundaryAsync(boundary, 11);

        seeked.Should().BeTrue();

        stream.Position.Should().Be(expectedPosition);
    }

    [Test]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(6)]
    [TestCase(8)]
    public async Task Test_AlignToBoundaryAsync_ShouldNotSeek(int boundary)
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = boundary * 2;
        var seeked = await stream.AlignToBoundaryAsync(boundary);

        seeked.Should().BeFalse();

        stream.Position.Should().Be(boundary * 2);
    }

    [TestCase(0, 11)]
    [TestCase(1, 12)]
    public async Task Test_SkipAsync_ShouldSeekToSkip(int numberOfBytes, int expectedPosition)
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = 11;
        await stream.SkipAsync(numberOfBytes);

        stream.Position.Should().Be(expectedPosition);
    }

    [Test]
    public async Task Test_SkipAsync_ShouldFailWhenNegativeNumberOfBytes()
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = 11;

        var call = () => stream.SkipAsync(-1);

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("Cannot seek backward");
    }

    [Test]
    public async Task Test_SkipTAsync_ShouldSeekToSkip()
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = 11;
        await stream.SkipAsync<TwoByteStruct>();

        stream.Position.Should().Be(13);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public async Task Test_SkipTAsync_ShouldSeekToSkip_Multiple(int countOfStructs)
    {
        var stream = new MemoryStream(new byte[30], false);
        stream.Position = 11;
        await stream.SkipAsync<TwoByteStruct>(countOfStructs);

        stream.Position.Should().Be(11 + 2 * countOfStructs);
    }

    [Test]
    [TestCase("")]
    [TestCase("a")]
    [TestCase("abcd")]
    [TestCase("12345678901234567890123456789012345678901")]
    public async Task Test_ReadAnsiStringAsync_ShouldReadString(string stringValue)
    {
        var bytesValue = Encoding.ASCII.GetBytes(stringValue);
        var stream = new MemoryStream(bytesValue);

        var actualString = await stream.ReadAnsiStringAsync();

        actualString.Should().Be(stringValue);
        stream.Position.Should().Be(stringValue.Length);
    }

    [Test]
    [TestCase("\0", "")]
    [TestCase("a\0awdawd", "a")]
    [TestCase("abcd\0a", "abcd")]
    [TestCase(
        "12345678901234567890123456789012345678901\0",
        "12345678901234567890123456789012345678901"
    )]
    public async Task Test_ReadAnsiStringAsync_ShouldReadString_NullTerminated(
        string stringValue,
        string expectedValue
    )
    {
        var bytesValue = Encoding.ASCII.GetBytes(stringValue);
        var stream = new MemoryStream(bytesValue);

        var actualString = await stream.ReadAnsiStringAsync();

        actualString.Should().Be(expectedValue);
        stream.Position.Should().Be(expectedValue.Length + 1);
    }

    [Test]
    [TestCase("")]
    [TestCase("a")]
    [TestCase("abcd")]
    [TestCase("12345678901234567890123456789012345678901")]
    public async Task Test_ReadFixedLengthAnsiStringAsync_ShouldReadString(string stringValue)
    {
        var bytesValue = Encoding.ASCII.GetBytes(stringValue);
        Array.Resize(ref bytesValue, bytesValue.Length + 10);
        var stream = new MemoryStream(bytesValue);

        var actualString = await stream.ReadFixedLengthAnsiStringAsync(stringValue.Length);

        actualString.Should().Be(stringValue);
        stream.Position.Should().Be(stringValue.Length);
    }

    [Test]
    public async Task Test_ReadStructAsync_ShouldReadStruct()
    {
        var buffer = new byte[] { 1, 2 };
        var stream = new MemoryStream(buffer);

        var value = await stream.ReadStructAsync<TwoByteStruct>();

        stream.Position.Should().Be(2);
        value.Field1.Should().Be(1);
        value.Field2.Should().Be(2);
    }

    [Test]
    public async Task Test_ReadStructAsync_ShouldFailNotEnoughData()
    {
        var buffer = new byte[] { 1 };
        var stream = new MemoryStream(buffer);

        var valueTask = () => stream.ReadStructAsync<TwoByteStruct>();

        var result = await valueTask.Should().ThrowAsync<Exception>();
        result.WithMessage($"Could not read 2 bytes from stream");
    }

    [Test]
    public async Task Test_ReadBlockAsync_ShouldReadStructs()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var value = await stream.ReadBlockAsync<TwoByteStruct>(2);

        stream.Position.Should().Be(4);
        value[0].Field1.Should().Be(1);
        value[0].Field2.Should().Be(2);
        value[1].Field1.Should().Be(3);
        value[1].Field2.Should().Be(4);
    }

    [Test]
    public async Task Test_ReadBlockAsync_ShouldFailNotEnoughData2()
    {
        var buffer = new byte[] { 1 };
        var stream = new MemoryStream(buffer);

        var valueTask = () => stream.ReadBlockAsync<TwoByteStruct>(1);

        var result = await valueTask.Should().ThrowAsync<Exception>();
        result.WithMessage($"Failed to read all data from stream");
    }

    [Test]
    public async Task Test_ReadStructAsync_Array_ShouldReadStructs()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var values = await stream.ReadStructAsync<TwoByteStruct>(2);

        stream.Position.Should().Be(4);
        values[0].Field1.Should().Be(1);
        values[0].Field2.Should().Be(2);

        values[1].Field1.Should().Be(3);
        values[1].Field2.Should().Be(4);
    }

    [Test]
    public async Task Test_ReadStreamAsync_ShouldReadBlock()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var subStream = await stream.ReadStreamAsync(2);

        stream.Position.Should().Be(2);
        subStream.Length.Should().Be(2);
        subStream.ToArray().Should().Equal(1, 2);
    }

    [Test]
    public async Task Test_ReadStreamAsync_ShouldFailNotEnoughData()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var call = () => stream.ReadStreamAsync(20);

        var result = await call.Should().ThrowAsync<Exception>();
        result.WithMessage("Not all data could be read from stream");
    }

    [Test]
    public async Task Test_ReadBlockAsync_ShouldReadBlock()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var subStream = await stream.ReadBlockAsync(2);

        stream.Position.Should().Be(2);
        subStream.Length.Should().Be(2);
        subStream.Should().Equal(1, 2);
    }

    [Test]
    public async Task Test_ReadBlockAsync_ShouldFailNotEnoughData()
    {
        var buffer = new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(buffer);

        var call = () => stream.ReadBlockAsync(20);

        var result = await call.Should().ThrowAsync<Exception>();
        result.WithMessage("Not all data could be read from stream");
    }
}
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
