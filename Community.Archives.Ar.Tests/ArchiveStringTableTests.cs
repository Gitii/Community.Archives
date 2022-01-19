using System;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Ar.Tests;

public class ArchiveStringTableTests
{
    [Test]
    public void Test_Dereference_ShouldDereferenceButNoStringFound()
    {
        ArchiveStringTable ast = new ArchiveStringTable();

        var call = () => ast.Dereference("/1");

        call.Should().Throw<Exception>("Could not find a string at offset 1");
    }
}
