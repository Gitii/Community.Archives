using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Community.Archives.Core;
using Community.Archives.Core.Tests;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Apk.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "<Pending>"
)]
public class AndroidBinaryXmlReaderTests
{
    [Test]
    public async Task Test_Read_ShouldReturnDocument()
    {
        using var manifest = new StreamFixtureFile("Fixtures/AndroidManifest.xml");
        using var utf8Manifest = new StreamFixtureFile("Fixtures/AndroidManifest.utf8.xml");

        var reader = new AndroidBinaryXmlReader();

        var actualDocument = await reader.ReadAsync(manifest.Content).ConfigureAwait(false);

        var expectedDocument = XDocument.Load(utf8Manifest.Content);
        actualDocument.Should().BeEquivalentTo(expectedDocument).And.Be(reader.Document);
    }

    [Test]
    public async Task Test_Read_ShouldFailBecauseOfInvalidTag()
    {
        var manifest = new StreamFixtureFile("Fixtures/AndroidManifest.broken.xml");

        try
        {
            var reader = new AndroidBinaryXmlReader();

            var call = () => reader.ReadAsync(manifest.Content);

            var result = await call.Should().ThrowAsync<Exception>();

            result.WithMessage("Invalid tag code: 7012452");
        }
        finally
        {
            manifest.Dispose();
        }
    }

    [Test]
    public async Task Test_Read_ShouldFailBecauseOfInvalidAttributeIndex()
    {
        var manifest = new StreamFixtureFile("Fixtures/AndroidManifest.xml");

        try
        {
            var reader = new AndroidBinaryXmlReader();

            var data = manifest.Content.ToArray();
            data[4568] = 255;
            data[4569] = 255;
            data[4570] = 255;
            data[4571] = 255;

            var call = () => reader.Read(data);

            var result = call.Should()
                .Throw<Exception>()
                .WithMessage("Invalid string index: Must not be negative");
        }
        finally
        {
            manifest.Dispose();
        }
    }

    [Test]
    public async Task Test_Read_ShouldFailBecauseOfStreamIsIncomplete()
    {
        var stream = new MemoryStream(new byte[] { 1, 2 });
        stream.Position = 1;

        try
        {
            var reader = new AndroidBinaryXmlReader();

            var call = () => reader.ReadAsync(stream);

            var result = await call.Should().ThrowAsync<Exception>();

            result.WithMessage("Could not read all bytes from input stream");
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }
}
