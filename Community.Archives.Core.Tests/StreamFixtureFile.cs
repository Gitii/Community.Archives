using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

[ExcludeFromCodeCoverage]
public class StreamFixtureFile : IDisposable
{
    public StreamFixtureFile()
    {
        Content = Stream.Null;
    }

    public StreamFixtureFile(string archivePath) : this()
    {
        Load(archivePath);
    }

    public Stream Content { get; private set; }

    public void Load(string path)
    {
        var content = File.ReadAllBytes(
            Path.Combine(TestContext.CurrentContext.TestDirectory, path)
        );

        Content = new MemoryStream(content, false);
    }

    public void Dispose()
    {
        Content.Dispose();
    }
}
