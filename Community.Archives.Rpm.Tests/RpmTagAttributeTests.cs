using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests;

public class RpmTagAttributeTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Test_Required(bool isRequired)
    {
        var attr = new RpmTagAttribute(0, 0, 0, isRequired);

        attr.IsRequired.Should().Be(isRequired);
    }
}
