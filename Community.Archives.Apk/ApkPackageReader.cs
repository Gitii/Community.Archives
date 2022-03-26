using System.Collections;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Community.Archives.Core;
using Microsoft.Extensions.Logging.Abstractions;
using SharpCompress.Readers.Zip;

namespace Community.Archives.Apk;

public class ApkPackageReader : IArchiveReader
{
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
                yield return new ArchiveEntry() { Name = item.Key, Content = zip.OpenEntryStream() };
            }
        }
    }

    public async Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        Stream manifest = Stream.Null;
        Stream resources = Stream.Null;

        await foreach (
            var entry in GetFileEntriesAsync(stream, "^AndroidManifest.xml$", "^resources.arsc$")
                .ConfigureAwait(false)
        )
        {
            if (entry.Name == "AndroidManifest.xml")
            {
                manifest = entry.Content;
            }
            else
            {
                resources = entry.Content;
            }
        }

        if (manifest.Length == 0)
        {
            throw new Exception("The apk doesn't contain a manifest.");
        }

        if (resources.Length == 0)
        {
            throw new Exception("The apk doesn't contain a resource file.");
        }

        var decodedManifest = await DecodeBinaryXmlAsync(manifest).ConfigureAwait(false);

        var package = SelectWithXPath(decodedManifest, "/*/manifest[1]/@package");
        var versionName = SelectWithXPath(decodedManifest, "/*/manifest[1]/@versionName");
        var description = SelectWithXPath(decodedManifest, "/*/manifest[1]/application[1]/@label");
        var versionCode = SelectWithXPath(decodedManifest, "/*/manifest[1]/@versionCode");

        return new IArchiveReader.ArchiveMetaData()
        {
            Package = package,
            Version = versionName,
            Architecture = string.Empty,
            Description = description,
            AllFields = new Dictionary<string, string>() { { "VersionCode", versionCode } }
        };
    }

    private Task<IDictionary<string, IList<string>>> DecodeBinaryResourcesAsync(
        MemoryStream resources
    )
    {
        var reader = new ApkResourceDecoder(new NullLogger<ApkResourceDecoder>());

        return reader.DecodeAsync(resources);
    }

    private string SelectWithXPath(XDocument document, string xpath)
    {
        var selector = document.XPathEvaluate(xpath);
        if (selector is IEnumerable selectedElements)
        {
            foreach (var selectedElement in selectedElements)
            {
                if (selectedElement is XAttribute attribute)
                {
                    return attribute.Value;
                }

                if (selectedElement is XElement element)
                {
                    return element.Value;
                }
            }
        }

        return selector.ToString();
    }

    private Task<XDocument> DecodeBinaryXmlAsync(Stream manifest)
    {
        var reader = new AndroidManifestReader();

        return reader.ReadAsync(manifest);
    }

    public bool SupportsMetaData { get; } = true;
}
