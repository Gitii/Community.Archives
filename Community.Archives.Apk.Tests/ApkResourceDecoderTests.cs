using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Community.Archives.Core.Tests;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Community.Archives.Apk.Tests;

public class ApkResourceDecoderTests
{
    [Test]
    public async Task Decode_ShouldEqualKnownValuesAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc");
        var expectedResourcesStream = new StreamFixtureFile("Fixtures/resources.json");

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var actualEntries = await reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var expectedEntries = DeserializeFromStream<IDictionary<string, IList<string>>>(
            expectedResourcesStream.Content
        );

        actualEntries.Should().BeEquivalentTo(expectedEntries);
    }

    [Test]
    public async Task Decode_ShouldFailWhenHeaderTypeIsUnsupportedAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // set invalid type
        actualResourcesStream.Content.Seek(12, SeekOrigin.Begin);
        actualResourcesStream.Content.WriteByte(25);

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("Unsupported Type");
    }

    [Test]
    public async Task Decode_ShouldFailWhenHeaderIsInvalidAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // set invalid typeStrings
        actualResourcesStream.Content.Seek(164624, SeekOrigin.Begin);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("TypeStrings must immediately follow the package structure header.");
    }

    [Test]
    public async Task Decode_ShouldFailWhenHeaderIsInvalid2Async()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // set invalid entryCount
        actualResourcesStream.Content.Seek(219184, SeekOrigin.Begin);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);
        actualResourcesStream.Content.WriteByte(0);

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("HeaderSize, entryCount and entriesStart are not valid.");
    }

    [Test]
    public async Task Decode_ShouldFailWhenUtf16StringIsTooLongAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // set invalid utf16 length
        actualResourcesStream.Content.Seek(164744, SeekOrigin.Begin);
        actualResourcesStream.Content.Write(BitConverter.GetBytes(32772));

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("Length of Utf16 string is supposed to be >32768.");
    }

    [Test]
    public async Task Decode_ShouldFailWhenUtf16StringCannotBeenFullyReadAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // cut of buffer right in the middle of a utf16 string
        actualResourcesStream.Content.SetLength(164745);

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("Failed to read ushort from stream");
    }

    [Test]
    public async Task Decode_ShouldFailWhenHasInvalidTypeAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc", true);

        // set invalid header
        actualResourcesStream.Content.WriteByte(0);

        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());
        var call = () => reader.DecodeAsync(actualResourcesStream.Content.ToArray());

        var result = await call.Should().ThrowAsync<Exception>();

        result.WithMessage("No RES_TABLE_TYPE found!");
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
        var serializer = new JsonSerializer();

        using (var sr = new StreamReader(stream))
        using (var jsonTextReader = new JsonTextReader(sr))
        {
            return serializer.Deserialize<T>(jsonTextReader)!;
        }
    }
}
