using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests
{
    public class RegexExtensionsTests
    {
        [Test]
        public void Test_IsMatch_Strings_ShouldMatch()
        {
            (new string[] { "a", "b" }).IsMatch("a").Should().BeTrue();
        }

        [Test]
        public void Test_IsMatch_Regexes_ShouldMatch()
        {
            (new Regex[] { new Regex("a", RegexOptions.None, TimeSpan.FromSeconds(1)) })
                .IsMatch("a")
                .Should()
                .BeTrue();
        }
    }
}
