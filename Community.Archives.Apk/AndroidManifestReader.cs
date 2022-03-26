﻿using System.Text;
using System.Xml.Linq;

namespace Community.Archives.Apk;

/// <remarks>
/// Code coming from:
/// https://stackoverflow.com/questions/18997163/how-can-i-read-the-manifest-of-an-android-apk-file-using-c-sharp-net/22314629
/// </remarks>
public class AndroidManifestReader : IAndroidManifestReader
{
    // Parses the 'compressed' binary form of Android XML docs
    // such as for AndroidManifest.binaryXml in .apk files
    private const int EndDocTag = 0x00100101;
    private const int StartTag = 0x00100102;
    private const int EndTag = 0x00100103;

    // StringIndexTable starts at offset 24x, an array of 32 bit LE offsets
    // of the length/string data in the StringTable.
    private const int StringIndexTableOffset = 0x24;

    private byte[] _xml = Array.Empty<byte>();

    private XDocument? _manifest;

    /// <summary>Returns the uncompressed Xml Manifest or <c>null</c> if <see cref="ReadAsync"/> or <see cref="Read"/> hasn't been called, yet.</summary>
    public XDocument? Manifest => _manifest;

    /// <summary>
    /// Reads all bytes from the passed in stream, reads the document from the data and returns the uncompressed xml manifest.
    /// </summary>
    /// <param name="stream">The input stream</param>
    /// <returns>The uncompressed xml document.</returns>
    /// <exception cref="Exception">Could not read all bytes from input stream.</exception>
    public async Task<XDocument> ReadAsync(Stream stream)
    {
        var buffer = new byte[stream.Length];

        var readBytes = await stream.ReadAsync(buffer).ConfigureAwait(false);

        if (readBytes != stream.Length)
        {
            throw new Exception("Could not read all bytes from input stream");
        }

        return Read(buffer);
    }

    /// <summary>
    /// Reads the document from the data and returns the uncompressed xml manifest.
    /// </summary>
    /// <param name="data">The data from which the document is being read.</param>
    /// <returns>The uncompressed xml document.</returns>
    public XDocument Read(byte[] data)
    {
        _xml = data;

        return (_manifest = ReadManifest());
    }

    private XDocument ReadManifest()
    {
        var result = new XDocument();
        result.Add(new XElement("root"));

        var tagStack = new Stack<XElement>();
        tagStack.Push(result.Root);

        var tagOffset = FindStartOfTags();
        while (tagOffset < _xml.Length)
        {
            var tagCode = BitConverter.ToInt32(_xml, tagOffset);
            switch (tagCode)
            {
                case StartTag:
                    tagOffset += ReadStartTag(tagOffset, out var startTag);
                    tagStack.Peek().Add(startTag);
                    tagStack.Push(startTag);
                    break;
                case EndTag:
                    var expectedTagName = tagStack.Peek().Name.LocalName;
                    tagOffset += ReadEndTag(tagOffset, expectedTagName);
                    tagStack.Pop();
                    break;
                case EndDocTag:
                    goto manifest_read;
                default:
                    goto manifest_read;
            }
        }

        manifest_read:
        return result;
    }

    /// <summary>Reads a start tag and returns the number of consumed bytes.</summary>
    /// <param name="offset"></param>
    /// <param name="element"></param>
    /// <remarks>
    /// XML tags and attributes:
    /// Every XML start and end tag consists of 6 32 bit words:
    ///   0th word: 02011000 for startTag, 03011000 for endTag, 01011000 for end of document
    ///   1st word: a flag?, like 38000000
    ///   2nd word: Line of where this tag appeared in the original source file
    ///   3rd word: FFFFFFFF ??
    ///   4th word: StringIndex of NameSpace name, or FFFFFFFF for default NS
    ///   5th word: StringIndex of Element Name
    /// 
    /// Start tags (not end tags) contain 3 more words:
    ///   6th word: 14001400 meaning?? 
    ///   7th word: Number of Attributes that follow this tag(follow word 8th)
    ///   8th word: 00000000 meaning??
    /// </remarks>
    /// <returns>The number of bytes consumed by the tag.</returns>
    private int ReadStartTag(int offset, out XElement element)
    {
        const int startTagDataSize = 9 * 4;
        element = new XElement(ReadTagName(offset));
        var bytesConsumed = startTagDataSize;

        var attributesCount = BitConverter.ToInt32(_xml, offset + 7 * 4);

        for (var attrIdx = 0; attrIdx < attributesCount; attrIdx++)
        {
            bytesConsumed += ReadAttribute(offset + bytesConsumed, out var attr);
            element.Add(attr);
        }

        return bytesConsumed;
    }

    /// <summary>Reads an attribute starting at the specified offset,
    /// and returns the consumed bytes count.</summary>
    /// <remarks>
    /// Attributes consist of 5 words: 
    ///   0th word: StringIndex of Attribute Name's Namespace, or FFFFFFFF
    ///   1st word: StringIndex of Attribute Name
    ///   2nd word: StringIndex of Attribute Value, or FFFFFFF if ResourceId used
    ///   3rd word: Flags?
    ///   4th word: str ind of attr value again, or ResourceId of value
    /// </remarks>
    private int ReadAttribute(int offset, out XAttribute attribute)
    {
        var attributeNameStringIndex = BitConverter.ToInt32(_xml, offset + 1 * 4);
        var attrName = RetrieveFromStringTable(attributeNameStringIndex);

        var attributeValueStringIndex = BitConverter.ToInt32(_xml, offset + 2 * 4);
        // AttrValue ResourceId or dup AttrValue StrInd
        var attributeResourceId = BitConverter.ToInt32(_xml, offset + 4 * 4);

        var attrValue =
            attributeValueStringIndex >= 0
                ? RetrieveFromStringTable(attributeValueStringIndex)
                : $"@{attributeResourceId}";

        attribute = new XAttribute(attrName, attrValue);
        return 20;
    }

    private int ReadEndTag(int tagOffset, string expectedTagName)
    {
        var tagName = ReadTagName(tagOffset);
        // Skip over 6 words of endTag data
        return tagName == expectedTagName
            ? 6 * 4
            : throw new InvalidOperationException(
                $"Malformed XML: expecting {expectedTagName} but found {tagName}"
            );
    }

    private string ReadTagName(int tagOffset)
    {
        var nameIndexOffset = tagOffset + 5 * 4;
        var nameStringIndex = BitConverter.ToInt32(_xml, nameIndexOffset);
        return RetrieveFromStringTable(nameStringIndex);
    }

    private int GetStringTableOffset()
    {
        // Compressed XML file/bytes starts with 24x bytes of data,
        // 9 32 bit words in little endian order (LSB first):
        //   0th word is 03 00 08 00
        //   3rd word SEEMS TO BE:  Offset at then of StringTable
        //   4th word is: Number of strings in string table
        var stringsCount = BitConverter.ToInt32(_xml, 4 * 4);
        return StringIndexTableOffset + stringsCount * 4;
    }

    /// <summary> Return the string stored in StringTable format at
    /// offset strOff.  This offset points to the 16 bit string length, which 
    /// is followed by that number of 16 bit (Unicode) chars. </summary>
    /// <returns></returns>
    private string RetrieveFromStringTable(int strInd)
    {
        if (strInd < 0)
        {
            return null;
        }

        // StringTable, each string is represented with a 16 bit little endian
        // character count, followed by that number of 16 bit (LE) (Unicode) chars.
        // StringTable follows StrIndexTable
        var stringOffset =
            GetStringTableOffset()
            + BitConverter.ToInt32(_xml, StringIndexTableOffset + strInd * 4);
        var stringLength = BitConverter.ToInt16(_xml, stringOffset);
        return Encoding.Unicode.GetString(_xml, stringOffset + 2, stringLength * 2);
    }

    /// <summary> The XML tag tree starts after some unknown content after the StringTable.
    /// There is some unknown data after the StringTable, scan forward from this point
    /// to the flag for the start of an XML start tag. </summary>
    /// <returns></returns>
    internal int FindStartOfTags()
    {
        // 12 is the index of the word containing the base offset of the xml tree.
        var xmlTagOffset = BitConverter.ToInt32(_xml, 12);

        for (var offset = xmlTagOffset; offset < _xml.Length - 4; offset += 4)
        {
            if (BitConverter.ToInt32(_xml, offset) == StartTag)
            {
                return offset;
            }
        }

        return xmlTagOffset;
    }
}
