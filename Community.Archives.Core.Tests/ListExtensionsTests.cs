using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class ListExtensionsTests
{
    [Test]
    public void Test_FirstOrNullable_ShouldReturnFirst()
    {
        var result = (new int[] { 1 }).FirstOrNullable((_) => true);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Test]
    public void Test_FirstOrNullable_ShouldReturnNull()
    {
        var result = (new int[] { 1 }).FirstOrNullable((_) => false);
        result.HasValue.Should().BeFalse();
    }
}
