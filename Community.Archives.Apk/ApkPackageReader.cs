using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using Community.Archives.Core;
using SharpCompress.Readers.Zip;

namespace Community.Archives.Apk;

public class ApkPackageReader : IArchiveReader
{
    public async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
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
                yield return new ArchiveEntry()
                {
                    Name = item.Key,
                    Content = zip.OpenEntryStream()
                };
            }
        }
    }

    public async Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        MemoryStream manifest = new MemoryStream();
        MemoryStream resources = new MemoryStream();

        await foreach (
            var entry in GetFileEntriesAsync(stream, "^AndroidManifest.xml$", "^resources.arsc$")
                .ConfigureAwait(false)
        )
        {
            if (entry.Name == "AndroidManifest.xml")
            {
                await entry.Content.CopyToAsync(manifest).ConfigureAwait(false);
            }
            else
            {
                await entry.Content.CopyToAsync(resources).ConfigureAwait(false);
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

        var decodedManifest = DecodeBinaryXml(manifest);
        // var decodedResources = DecodeBinaryResources(resources);

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
        var reader = new ApkResourceFinder();

        return reader.ProcessResourceTableAsync(resources.ToArray());
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

    private XDocument DecodeBinaryXml(MemoryStream manifest)
    {
        var reader = new AndroidManifestReader(manifest.ToArray());

        return reader.Manifest;
    }

    public bool SupportsMetaData { get; } = true;
}
