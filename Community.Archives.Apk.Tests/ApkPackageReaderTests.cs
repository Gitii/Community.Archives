using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.Archives.Core;
using Community.Archives.Core.Tests;
using NUnit.Framework;

namespace Community.Archives.Apk.Tests;

public class ApkPackageReaderTests : ArchiveReaderTests<ApkPackageReader>
{
    [Test]
    public async Task GetMetaData_ShouldEqualKnownValuesAsync()
    {
        using var archive = new StreamFixtureFile("Fixtures/app-sample.apk");

        await AssertMetaDataSupported(
            archive.Content,
            new IArchiveReader.ArchiveMetaData()
            {
                Package = "de.markusfisch.android.binaryeye",
                Version = "1.48.1",
                Architecture = "",
                Description = "@2131689505",
                AllFields = new Dictionary<string, string>(StringComparer.Ordinal) { ["VersionCode"] = "@93" },
            }
        );
    }
}
