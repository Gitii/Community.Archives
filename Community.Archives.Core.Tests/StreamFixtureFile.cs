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
        Content = new MemoryStream(Array.Empty<byte>());
    }

    public StreamFixtureFile(string archivePath) : this()
    {
        Load(archivePath);
    }

    public MemoryStream Content { get; private set; }

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
