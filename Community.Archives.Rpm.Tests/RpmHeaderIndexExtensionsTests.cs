using System;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmHeaderIndexExtensionsTests
{
    [Test]
    public void Test_GetIndexType_ShouldThrowIfIndexInvalid()
    {
        var index = new RpmHeaderIndex() { type = 666, };

        var call = () => index.GetIndexType();

        call.Should().Throw<Exception>().WithMessage("Invalid index type 666");
    }

    [Test]
    public void Test_AssertType_ShouldThrowIfIndexInvalid()
    {
        var index = new RpmHeaderIndex() { type = (int)IndexType.RPM_BIN_TYPE, };

        var call = () => index.AssertType(IndexType.RPM_I18NSTRING_TYPE);

        call.Should()
            .Throw<Exception>()
            .WithMessage(
                $"Expected index type to be {IndexType.RPM_I18NSTRING_TYPE} but it's {IndexType.RPM_BIN_TYPE}"
            );
    }

    [Test]
    public void Test_GetValue_ShouldThrowIfIndexInvalid()
    {
        var index = new RpmHeaderIndex() { type = 666, };

        var call = () => index.GetValue((IndexType)666, 0, Array.Empty<byte>());

        call.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Test_GetString_ShouldReturnEmptyStringIfEmpty()
    {
        var index = new RpmHeaderIndex() { type = (int)IndexType.RPM_STRING_TYPE, };

        index.GetString(Array.Empty<byte>()).Should().Be(string.Empty);
    }
}
