using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class ArchiveMetaDataTests
{
    [Test]
    public void Test_Constructor()
    {
        var md = new IArchiveReader.ArchiveMetaData()
        {
            Package = "p",
            Version = "v",
            Description = "d",
            Architecture = "a",
            AllFields = new Dictionary<string, string>(),
        };

        md.Package.Should().Be("p");
        md.Version.Should().Be("v");
        md.Description.Should().Be("d");
        md.Architecture.Should().Be("a");
        md.AllFields.Should().BeOfType<Dictionary<string, string>>();
    }

    private static object?[] ArchiveMetaDataTestCases =
    {
        new object?[] { new IArchiveReader.ArchiveMetaData(), new IArchiveReader.ArchiveMetaData(), true },
        new object?[]
        {
            new IArchiveReader.ArchiveMetaData() { Package = "a" },
            new IArchiveReader.ArchiveMetaData() { Package = "b" }, false
        },
        new object?[]
        {
            new IArchiveReader.ArchiveMetaData() { Description = "a" },
            new IArchiveReader.ArchiveMetaData() { Description = "b" }, false
        },
        new object?[]
        {
            new IArchiveReader.ArchiveMetaData() { Architecture = "a" },
            new IArchiveReader.ArchiveMetaData() { Architecture = "b" }, false
        },
        new object?[]
        {
            new IArchiveReader.ArchiveMetaData() { Version = "a" },
            new IArchiveReader.ArchiveMetaData() { Version = "b" }, false
        }
    };

    [TestCaseSource(nameof(ArchiveMetaDataTestCases))]
    public void Test_Equals(
        IArchiveReader.ArchiveMetaData left,
        IArchiveReader.ArchiveMetaData right,
        bool areEqual
    )
    {
        left.Equals(right).Should().Be(areEqual);
    }

    [TestCaseSource(nameof(ArchiveMetaDataTestCases))]
    public void Test_Object_Equals(object left, object? right, bool areEqual)
    {
        left.Equals(right).Should().Be(areEqual);
    }

    [Test]
    public void Test_Object_Equals_null()
    {
        (new IArchiveReader.ArchiveMetaData()).Equals(null).Should().Be(false);
    }

    [Test]
    public void Test_Object_Equals_other()
    {
        (new IArchiveReader.ArchiveMetaData()).Equals(new object()).Should().Be(false);
    }

    [Test]
    public void Test_Object_GetHashCode()
    {
        (new IArchiveReader.ArchiveMetaData()).GetHashCode().Should().NotBe(0);
    }
}
