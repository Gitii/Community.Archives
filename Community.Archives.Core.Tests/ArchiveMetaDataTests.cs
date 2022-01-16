using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests
{
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
    }
}
