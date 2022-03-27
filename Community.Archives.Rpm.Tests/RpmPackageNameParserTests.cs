using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmPackageNameParserTests
{
    [TestCase("a-b-c", true)]
    [TestCase("a-b-c-d", true)]
    [TestCase("a-b", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("a-b-c-d-e", false)]
    [TestCase("a---", false)]
    [TestCase("a", false)]
    public void Test_IsValid_ShouldDetectValidNames(string? name, bool isValid)
    {
        RpmPackageNameParser reader = new RpmPackageNameParser();
        reader.IsValid(name!).Should().Be(isValid);
    }

    public static object[] TryParseTestCases = new[]
    {
        new object?[] { "a-b-c", new RpmPackageName("a", "b", "c") },
        new object?[] { "a-b-c-d", new RpmPackageName("a", "b", "c", "d") },
        new object?[] { "a-b", null },
        new object?[] { "", null },
        new object?[] { null, null },
        new object?[] { "a-b-c-d-e", null },
        new object?[] { "a----", null },
        new object?[] { "a", null },
    };

    [TestCaseSource(nameof(TryParseTestCases))]
    public void Test_TryParse_ShouldParse(string? name, RpmPackageName? expectedParsedName)
    {
        RpmPackageNameParser reader = new RpmPackageNameParser();
        var isValid = reader.TryParse(name!, out var actualParsedName);

        isValid.Should().Be(expectedParsedName != null);
        actualParsedName.Should().Be(expectedParsedName);
    }
}
