using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Community.Archives.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpCompress.Readers.Zip;

namespace Community.Archives.Apk;

public class ApkPackageReader : IArchiveReader
{
    public const string MANIFEST_VERSION_CODE_KEY = "VersionCode";
    public const string MANIFEST_PERMISSION_ARRAY_KEY = "Permissions";
    public const string MANIFEST_ICON_FILE_NAMES_KEY = "Icons";
    public const string MANIFEST_ARRAY_SEPARATOR = ",";

    private const string ANDROID_MANIFEST_FILE_NAME = "AndroidManifest.xml";
    private const string ANROID_RESOURCE_FILE_NAME = "resources.arsc";

#pragma warning disable CS1998
    public async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
#pragma warning restore CS1998
        Stream stream,
        params string[] regexMatcher
    )
    {
        using var zip = ZipReader.Open(stream);

        while (zip.MoveToNextEntry())
        {
            var item = zip.Entry;
            if (
                !item.IsDirectory
                && regexMatcher.Any(
                    (regex) =>
                        Regex.IsMatch(item.Key, regex, RegexOptions.None, TimeSpan.FromSeconds(1))
                )
            )
            {
                var data = new MemoryStream(new byte[item.Size]);

                await zip.OpenEntryStream().CopyToAsync(data).ConfigureAwait(false);

                data.Position = 0;

                yield return new ArchiveEntry() { Name = item.Key, Content = data, };
            }
        }

        yield break;
    }

    public async Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        Stream manifest = Stream.Null;
        Stream resources = Stream.Null;

        await foreach (
            var entry in GetFileEntriesAsync(
                    stream,
                    $"^{ANDROID_MANIFEST_FILE_NAME}$",
                    $"^{ANROID_RESOURCE_FILE_NAME}$"
                )
                .ConfigureAwait(false)
        )
        {
            if (entry.Name == ANDROID_MANIFEST_FILE_NAME)
            {
                manifest = entry.Content;
            }
            else
            {
                resources = entry.Content;
            }
        }

        if (manifest == Stream.Null)
        {
            throw new Exception("The apk doesn't contain a manifest.");
        }

        if (resources == Stream.Null)
        {
            throw new Exception("The apk doesn't contain a resource file.");
        }

        var decodedManifest = await DecodeBinaryXmlAsync(manifest).ConfigureAwait(false);
        var decodedResources = await DecodeResourcesAsync(resources).ConfigureAwait(false);

        return ExtractMetaData(decodedManifest, decodedResources);
    }

    private IArchiveReader.ArchiveMetaData ExtractMetaData(
        XDocument decodedManifest,
        IDictionary<string, IList<string?>> decodedResources
    )
    {
        var package = decodedManifest.SelectWithXPath("/*/manifest[1]/@package", decodedResources);
        var versionName = decodedManifest.SelectWithXPath(
            "/*/manifest[1]/@versionName",
            decodedResources
        );
        var description = decodedManifest.SelectWithXPath(
            "/*/manifest[1]/application[1]/@label",
            decodedResources
        );
        var versionCode = decodedManifest.SelectWithXPath(
            "/*/manifest[1]/@versionCode",
            decodedResources
        );
        var perms = String.Join(
            MANIFEST_ARRAY_SEPARATOR,
            decodedManifest.SelectWithXPath(
                "/*/manifest[1]/uses-permission/@name",
                decodedResources
            )
        );
        var icons = String.Join(
            MANIFEST_ARRAY_SEPARATOR,
            GetAllIconFileNames(decodedManifest, decodedResources)
        );

        return new IArchiveReader.ArchiveMetaData()
        {
            Package = package,
            Version = versionName,
            Architecture = string.Empty,
            Description = description,
            AllFields = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { MANIFEST_VERSION_CODE_KEY, versionCode },
                { MANIFEST_PERMISSION_ARRAY_KEY, perms },
                { MANIFEST_ICON_FILE_NAMES_KEY, icons }
            }
        };
    }

    private IEnumerable<string> GetAllIconFileNames(
        XDocument decodedManifest,
        IDictionary<string, IList<string?>> decodedResources
    )
    {
        return decodedManifest
            .SelectAllWithXPath("/*/manifest[1]/application[1]/@icon", decodedResources, true)
            .Where((item) => !item.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
    }

    private Task<IDictionary<string, IList<string?>>> DecodeResourcesAsync(Stream resources)
    {
        var decoder = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());

        return decoder.DecodeAsync(resources);
    }

    private Task<XDocument> DecodeBinaryXmlAsync(Stream manifest)
    {
        var reader = new AndroidBinaryXmlReader();

        return reader.ReadAsync(manifest);
    }

    public bool SupportsMetaData { get; } = true;
}
