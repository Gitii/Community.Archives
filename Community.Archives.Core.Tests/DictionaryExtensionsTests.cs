using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class DictionaryExtensionsTests
{
    private static Dictionary<bool, bool?> dictionary = new();

    static object?[] AreEqualTestCases =
    {
        new object?[] { null, null, true }, new object?[] { null, new Dictionary<bool, bool?>(), false },
        new object?[] { new Dictionary<bool, bool?>(), null, false },
        new object?[] { dictionary, dictionary, true },
        new object?[] { new Dictionary<bool, bool?>() { { true, false } }, dictionary, false },
        new object?[]
        {
            new Dictionary<bool, bool?>() { { true, false } },
            new Dictionary<bool, bool?>() { { true, false } }, true
        },
        new object?[]
        {
            new Dictionary<bool, bool?>() { { true, true } }, new Dictionary<bool, bool?>() { { true, false } },
            false
        },
        new object?[]
        {
            new Dictionary<bool, bool?>() { { true, null } }, new Dictionary<bool, bool?>() { { true, null } },
            true
        },
        new object?[]
        {
            new Dictionary<bool, bool?>() { { true, null } }, new Dictionary<bool, bool?>() { { true, false } },
            false
        },
        new object?[]
        {
            new Dictionary<bool, bool?>() { { true, null } }, new Dictionary<bool, bool?>() { { false, null } },
            false
        }
    };

    [TestCaseSource(nameof(AreEqualTestCases))]
    public void Test_AreEqual(
        IReadOnlyDictionary<bool, bool?>? left,
        IReadOnlyDictionary<bool, bool?>? right,
        bool areEqual
    )
    {
        left.AreEqual(right).Should().Be(areEqual);
    }
}
