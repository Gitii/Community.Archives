using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmPackageNameTests
{
    [Test]
    public void Test_Constructor_ShouldBeInitializedWithEmptyString()
    {
        var pn = new RpmPackageName();
        pn.Name.Should().BeEmpty();
        pn.Version.Should().BeEmpty();
        pn.Release.Should().BeEmpty();
        pn.Architecture.Should().BeNull(); // architecture is optional
    }

    [Test]
    public void Test_Constructor_Explicit()
    {
        var pn = new RpmPackageName("a", "b", "c");
        pn.Name.Should().Be("a");
        pn.Version.Should().Be("b");
        pn.Release.Should().Be("c");
        pn.Architecture.Should().BeNull();
    }

    [Test]
    public void Test_Constructor_Explicit_WithArch()
    {
        var pn = new RpmPackageName("a", "b", "c", "d");
        pn.Name.Should().Be("a");
        pn.Version.Should().Be("b");
        pn.Release.Should().Be("c");
        pn.Architecture.Should().Be("d");
    }

    [Test]
    public void Test_ToString()
    {
        var pn = new RpmPackageName("a", "b", "c");
        pn.ToString().Should().Be("a-b-c");
    }

    [Test]
    public void Test_ToString_WithArch()
    {
        var pn = new RpmPackageName("a", "b", "c", "d");
        pn.ToString().Should().Be("a-b-c-d");
    }
}
