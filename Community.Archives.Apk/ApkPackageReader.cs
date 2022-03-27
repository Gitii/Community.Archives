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
            if (!item.IsDirectory && regexMatcher.Any((regex) => Regex.IsMatch(item.Key, regex)))
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
        var package = SelectWithXPath(decodedManifest, "/*/manifest[1]/@package", decodedResources);
        var versionName = SelectWithXPath(
            decodedManifest,
            "/*/manifest[1]/@versionName",
            decodedResources
        );
        var description = SelectWithXPath(
            decodedManifest,
            "/*/manifest[1]/application[1]/@label",
            decodedResources
        );
        var versionCode = SelectWithXPath(
            decodedManifest,
            "/*/manifest[1]/@versionCode",
            decodedResources
        );
        var perms = String.Join(
            MANIFEST_ARRAY_SEPARATOR,
            SelectAllWithXPath(
                decodedManifest,
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
            AllFields = new Dictionary<string, string>()
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
        return SelectAllWithXPath(
                decodedManifest,
                "/*/manifest[1]/application[1]/@icon",
                decodedResources,
                true
            )
            .Where((item) => !item.EndsWith(".xml"));
    }

    private Task<IDictionary<string, IList<string?>>> DecodeResourcesAsync(Stream resources)
    {
        var decoder = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());

        return decoder.DecodeAsync(resources);
    }

    [ExcludeFromCodeCoverage]
    private string SelectWithXPath(
        XDocument document,
        string xpath,
        IDictionary<string, IList<string?>> resources
    )
    {
        return SelectAllWithXPath(document, xpath, resources).FirstOrDefault() ?? String.Empty;
    }

    [ExcludeFromCodeCoverage]
    private IEnumerable<string> SelectAllWithXPath(
        XDocument document,
        string xpath,
        IDictionary<string, IList<string?>> resources,
        bool all = false
    )
    {
        var selector = document.XPathEvaluate(xpath);
        if (selector is IEnumerable selectedElements)
        {
            foreach (var selectedElement in selectedElements)
            {
                if (selectedElement is XAttribute attribute)
                {
                    foreach (var value in DereferenceIfUnique(attribute.Value))
                    {
                        yield return value;
                    }
                }

                if (selectedElement is XElement element)
                {
                    foreach (var value in DereferenceIfUnique(element.Value))
                    {
                        yield return value;
                    }
                }
            }
        }

        IEnumerable<string> DereferenceIfUnique(string valueOrReference)
        {
            if (!valueOrReference.StartsWith("@"))
            {
                yield return valueOrReference;
            }
            else
            {
                string refKey = valueOrReference;
                if (resources.TryGetValue(refKey, out var values))
                {
                    if (all)
                    {
                        foreach (var value in values)
                        {
                            yield return value!;
                        }
                    }
                    else
                    {
                        yield return values.FirstOrDefault() ?? valueOrReference;
                    }
                }
            }
        }
    }

    private Task<XDocument> DecodeBinaryXmlAsync(Stream manifest)
    {
        var reader = new AndroidBinaryXmlReader();

        return reader.ReadAsync(manifest);
    }

    public bool SupportsMetaData { get; } = true;
}
