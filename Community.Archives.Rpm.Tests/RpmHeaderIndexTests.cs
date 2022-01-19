using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmHeaderIndexTests
{
    [Test]
    public void Test_ToString_ShouldReturnString()
    {
        var headerIndex = (new RpmHeaderIndex() { count = 1, offset = 1, tag = 1, type = 1 });
        headerIndex.ToString().Should().Be("Tag = 1; Type = 1; Offset = 1; Count = 1");
    }
}
