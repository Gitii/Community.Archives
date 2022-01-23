using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmTagsTests
{
    [Test]
    public void Test_GetFields_ShouldNotReturnNull()
    {
        var tags = new RpmTags { Name = null!, Version = String.Empty, Description = "a" };

        tags.GetFields()
            .Should()
            .BeEquivalentTo(
                new Dictionary<string, string>()
                {
                    { "Description", "a" },
                    { "SignatureTagSize", "0" },
                    { "SignatureTagPayloadSize", "0" },
                    { "Size", "0" },
                    { "ArchiveSize", "0" }
                }
            );
    }
}
