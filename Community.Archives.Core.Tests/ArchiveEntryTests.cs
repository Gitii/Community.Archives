using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests
{
    public class ArchiveEntryTests
    {
        [Test]
        public void Test_Constructor_ShouldNotBeInitialized()
        {
            var ae = new ArchiveEntry();
            ae.Name.Should().BeNull();
            ae.Content.Should().BeNull();
        }

        [Test]
        public void Test_Constructor_ShouldBeInitialized()
        {
            var ae = new ArchiveEntry() { Name = "n", Content = new MemoryStream(), };
            ae.Name.Should().Be("n");
            ae.Content.Should().BeOfType<MemoryStream>();
        }

        [Test]
        public void Test_ToString_ShouldKnown()
        {
            var ae = new ArchiveEntry() { Name = "n", Content = new MemoryStream(), };

            ae.ToString().Should().Be("n (0 bytes)");
        }

        [Test]
        public void Test_Constructor_ShouldBeDisposed()
        {
            var stream = A.Fake<Stream>();
            var ae = new ArchiveEntry() { Content = stream, };
            ae.Dispose();

            A.CallTo(() => stream.Close()).MustHaveHappened();
        }

        [Test]
        public async Task Test_Constructor_ShouldBeDisposedAsync()
        {
            var stream = A.Fake<Stream>();
            var ae = new ArchiveEntry() { Content = stream, };
            await ae.DisposeAsync();

            A.CallTo(() => stream.DisposeAsync()).MustHaveHappened();
        }
    }
}
