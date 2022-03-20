using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Community.Archives.Core.Tests;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Community.Archives.Apk.Tests;

public class ApkResourceFinderTests
{
    [Test]
    public async Task Decode_ShouldEqualKnownValuesAsync()
    {
        var actualResourcesStream = new StreamFixtureFile("Fixtures/resources.arsc");
        var expectedResourcesStream = new StreamFixtureFile("Fixtures/resources.json");

        var reader = new ApkResourceFinder();
        var actualEntries = await reader.ProcessResourceTableAsync(
            actualResourcesStream.Content.ToArray()
        );

        var expectedEntries = DeserializeFromStream<IDictionary<string, IList<string>>>(
            expectedResourcesStream.Content
        );

        actualEntries.Should().BeEquivalentTo(expectedEntries);
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
