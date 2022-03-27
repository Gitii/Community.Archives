using System;
using System.Collections.Generic;
using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Apk.Tests;

public class XDocumentExtensionsTests
{
    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnElementValue()
    {
        XDocument doc = new XDocument(new XElement("root", "val!"));

        doc.SelectAllWithXPath(
                "/root",
                new Dictionary<string, IList<string?>>(StringComparer.Ordinal)
            )
            .Should()
            .BeEquivalentTo("val!");
    }

    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnAllElementValues()
    {
        XDocument doc = new XDocument(
            new XElement(
                "root",
                new XElement("container", new XElement("child", "c1"), new XElement("child", "c2"))
            )
        );

        doc.SelectAllWithXPath(
                "/root/container/child",
                new Dictionary<string, IList<string?>>(StringComparer.Ordinal),
                true
            )
            .Should()
            .BeEquivalentTo("c1", "c2");
    }

    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnAttributeValue()
    {
        XDocument doc = new XDocument(new XElement("root", "val!", new XAttribute("a", "attr!")));

        doc.SelectAllWithXPath(
                "/root/@a",
                new Dictionary<string, IList<string?>>(StringComparer.Ordinal)
            )
            .Should()
            .BeEquivalentTo("attr!");
    }

    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnAllAttributeValues()
    {
        XDocument doc = new XDocument(
            new XElement("root", new XElement("container", new XAttribute("child", "c1")))
        );

        doc.SelectAllWithXPath(
                "/root/container/@child",
                new Dictionary<string, IList<string?>>(StringComparer.Ordinal),
                true
            )
            .Should()
            .BeEquivalentTo("c1");
    }

    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnFirstDereferencedValue()
    {
        XDocument doc = new XDocument(new XElement("root", new XElement("container", "@1")));

        doc.SelectAllWithXPath(
                "/root/container",
                new Dictionary<string, IList<string?>>(StringComparer.Ordinal)
                {
                    {
                        "@1",
                        new List<string?>() { "p", "p2" }
                    }
                },
                false
            )
            .Should()
            .BeEquivalentTo("p");
    }

    [Test]
    public void Test_SelectAllWithXPath_ShouldReturnDereferencedValue()
    {
        XDocument doc = new XDocument(new XElement("root", new XElement("container", "@1")));

        doc.SelectAllWithXPath(
                "/root/container",
                new Dictionary<string, IList<string?>>()
                {
                    {
                        "@1",
                        new List<string?>() { "p", "p2" }
                    }
                },
                true
            )
            .Should()
            .BeEquivalentTo("p", "p2");
    }
}
