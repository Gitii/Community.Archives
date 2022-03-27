using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

[ExcludeFromCodeCoverage]
public class StreamFixtureFile : IDisposable
{
    private readonly bool _isWritable;

    public StreamFixtureFile()
    {
        Content = new MemoryStream(Array.Empty<byte>());
        _isWritable = false;
    }

    public StreamFixtureFile(string archivePath, bool isWritable = false) : this()
    {
        _isWritable = isWritable;
        Load(archivePath);
    }

    public MemoryStream Content { get; private set; }

    public void Load(string path)
    {
        var content = File.ReadAllBytes(
            Path.Combine(TestContext.CurrentContext.TestDirectory, path)
        );

        Content = new MemoryStream(content, _isWritable);
    }

    public void Dispose()
    {
        Content.Dispose();
    }
}
