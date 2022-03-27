using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.Archives.Core;
using Community.Archives.Core.Tests;
using FluentAssertions;
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
                Description = "Binary Eye",
                AllFields = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["VersionCode"] = "93",
                    ["Permissions"] =
                        "android.permission.ACCESS_FINE_LOCATION,android.permission.CAMERA,android.permission.INTERNET,android.permission.VIBRATE,android.permission.WRITE_EXTERNAL_STORAGE,android.permission.CHANGE_WIFI_STATE,android.permission.ACCESS_WIFI_STATE",
                    ["Icons"] = "res/8X.png,res/u3.png,res/SD.png,res/jy.png,res/D2.png,res/CG.png"
                },
            }
        );
    }

    [TestCase("Fixtures/app-sample.no-manifest.apk", "The apk doesn't contain a manifest.")]
    [TestCase("Fixtures/app-sample.no-resources.apk", "The apk doesn't contain a resource file.")]
    public async Task GetMetaData_ShouldFailWhenApkMissesFilesAsync(
        string fixtureFileName,
        string exceptionMessage
    )
    {
        var actualResourcesStream = new StreamFixtureFile(fixtureFileName);

        IArchiveReader reader = new ApkPackageReader();

        var call = () => reader.GetMetaDataAsync(actualResourcesStream.Content);

        (await call.Should().ThrowAsync<Exception>()).WithMessage(exceptionMessage);
    }
}
