using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Community.Archives.Apk;

internal static class XDocumentExtensions
{
    public static string SelectWithXPath(
        this XDocument document,
        string xpath,
        IDictionary<string, IList<string?>> resources
    )
    {
        return SelectAllWithXPath(document, xpath, resources).FirstOrDefault() ?? String.Empty;
    }

    public static IEnumerable<string> SelectAllWithXPath(
        this XDocument document,
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
            if (!valueOrReference.StartsWith("@", StringComparison.Ordinal))
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
}
