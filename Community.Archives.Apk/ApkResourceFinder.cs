using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Community.Archives.Core;

namespace Community.Archives.Apk;

public class ApkResourceFinder
{
    private const short RES_STRING_POOL_TYPE = 0x0001;
    private const short RES_TABLE_TYPE = 0x0002;
    private const short RES_TABLE_PACKAGE_TYPE = 0x0200;
    private const short RES_TABLE_TYPE_TYPE = 0x0201;
    private const short RES_TABLE_TYPE_SPEC_TYPE = 0x0202;

    string[] valueStringPool = Array.Empty<string>();
    string[] typeStringPool = Array.Empty<string>();
    string[] keyStringPool = Array.Empty<string>();

    private int package_id = 0;

    //// Contains no data.
    //static byte TYPE_NULL = 0x00;
    //// The 'data' holds an attribute resource identifier.
    //static byte TYPE_ATTRIBUTE = 0x02;
    //// The 'data' holds a single-precision floating point number.
    //static byte TYPE_FLOAT = 0x04;
    //// The 'data' holds a complex number encoding a dimension value,
    //// such as "100in".
    //static byte TYPE_DIMENSION = 0x05;
    //// The 'data' holds a complex number encoding a fraction of a
    //// container.
    //static byte TYPE_FRACTION = 0x06;
    //// The 'data' is a raw integer value of the form n..n.
    //static byte TYPE_INT_DEC = 0x10;
    //// The 'data' is a raw integer value of the form 0xn..n.
    //static byte TYPE_INT_HEX = 0x11;
    //// The 'data' is either 0 or 1, for input "false" or "true" respectively.
    //static byte TYPE_INT_BOOLEAN = 0x12;
    //// The 'data' is a raw integer value of the form #aarrggbb.
    //static byte TYPE_INT_COLOR_ARGB8 = 0x1c;
    //// The 'data' is a raw integer value of the form #rrggbb.
    //static byte TYPE_INT_COLOR_RGB8 = 0x1d;
    //// The 'data' is a raw integer value of the form #argb.
    //static byte TYPE_INT_COLOR_ARGB4 = 0x1e;
    //// The 'data' is a raw integer value of the form #rgb.
    //static byte TYPE_INT_COLOR_RGB4 = 0x1f;

    // The 'data' holds a ResTable_ref, a reference to another resource
    // table entry.
    private const byte TYPE_REFERENCE = 0x01;

    // The 'data' holds an index into the containing resource table's
    // global value string pool.
    public const byte TYPE_STRING = 0x03;

    private IDictionary<string, IList<string?>> responseMap = new Dictionary<
        string,
        IList<string?>
    >();

    Dictionary<int, List<string>> entryMap = new Dictionary<int, List<string>>();

    public Task<IDictionary<string, IList<string>>> ProcessResourceTableAsync(byte[] data)
    {
        using MemoryStream ms = new MemoryStream(data);

        return ProcessResourceTableAsync(ms);
    }

    public Task<IDictionary<string, IList<string>>> ProcessResourceTableAsync(Stream stream)
    {
        responseMap = new Dictionary<string, IList<string?>>();

        return ExtractDataAsync(stream);
    }

    private async Task<IDictionary<string, IList<string>>> ExtractDataAsync(Stream stream)
    {
        var header = await stream.ReadStructAsync<ResourceTableHeader>().ConfigureAwait(false);

        if (header.type != RES_TABLE_TYPE)
        {
            throw new Exception("No RES_TABLE_TYPE found!");
        }

        var (realStringPoolCount, realPackageCount) = await ExtractPoolsAndPackagesAsync(
                stream,
                header.packageCount
            )
            .ConfigureAwait(false);

        if (realStringPoolCount != 1)
        {
            throw new Exception("More than 1 string pool found!");
        }

        if (realPackageCount != header.packageCount)
        {
            throw new Exception("Real package count not equals the declared count.");
        }

        return responseMap;
    }

    private async Task<(int actualStringPoolCount, int actualPackageCount)> ExtractPoolsAndPackagesAsync(
        Stream stream,
        int packageCount
    )
    {
        var actualStringPoolCount = 0;
        var actualPackageCount = 0;

        for (int i = 0; i < (packageCount + 1); i++)
        {
            long pos = stream.Position;
            var header = await stream.ReadStructAsync<GeneralPoolHeader>().ConfigureAwait(false);

            if (header.type == RES_STRING_POOL_TYPE)
            {
                if (actualStringPoolCount == 0)
                {
                    // Only the first string pool is processed.
                    Debug.WriteLine("Processing the string pool ...");

                    valueStringPool = await ReadStringPoolAsync(stream).ConfigureAwait(false);
                }

                actualStringPoolCount++;
            }
            else if (header.type == RES_TABLE_PACKAGE_TYPE)
            {
                // Process the package
                Debug.WriteLine("Processing package {0} ...", actualPackageCount);

                await ExtractPackageAsync(stream, header).ConfigureAwait(false);

                actualPackageCount++;
            }
            else
            {
                throw new InvalidOperationException("Unsupported Type");
            }

            await stream.SkipAsync(header.size - (stream.Position - pos)).ConfigureAwait(false);
        }

        return (actualStringPoolCount, actualPackageCount);
    }

    private async Task<(int typeSpecCount, int typeCount)> ExtractPackageAsync(Stream ms, GeneralPoolHeader header)
    {
        long lastPosition = ms.Position - Marshal.SizeOf<GeneralPoolHeader>();
        var headerSuffix = await ms.ReadStructAsync<PackageHeaderSuffix>().ConfigureAwait(false);

        package_id = headerSuffix.id;

        if (headerSuffix.typeStrings != header.headerSize)
        {
            throw new Exception(
                "TypeStrings must immediately follow the package structure header."
            );
        }

        await ms.SkipAsync(headerSuffix.typeStrings - (ms.Position - lastPosition))
            .ConfigureAwait(false); // skip rest of header

        var keyStringHeader = await ExtractPoolsAsync(ms, headerSuffix, lastPosition).ConfigureAwait(false);

        // Iterate through all chunks
        //
        int typeSpecCount = 0;
        int typeCount = 0;

        await ms.SkipAsync(
                (headerSuffix.keyStrings + keyStringHeader.size) - (ms.Position - lastPosition)
            )
            .ConfigureAwait(false);

        long bytesLeft = header.size - (ms.Position - lastPosition);

        while (bytesLeft > 0)
        {
            int pos = (int)ms.Position;
            var h = await ms.ReadStructAsync<GeneralPoolHeader>().ConfigureAwait(false);

            switch (h.type)
            {
                case RES_TABLE_TYPE_SPEC_TYPE:
                    // Process the string pool
                    await ExtractTypeSpecAsync(ms, typeStringPool).ConfigureAwait(false);

                    typeSpecCount++;
                    break;
                case RES_TABLE_TYPE_TYPE:
                    // Process the package
                    await ExtractTypesAsync(ms, h).ConfigureAwait(false);

                    typeCount++;
                    break;
            }

            ms.Seek(pos + h.size, SeekOrigin.Begin);
            bytesLeft -= h.size;
        }

        return (typeSpecCount, typeCount);
    }

    private async Task<GeneralPoolHeader> ExtractPoolsAsync(Stream ms, PackageHeaderSuffix headerSuffix,
        long lastPosition)
    {
        Debug.WriteLine("Type strings:");
        await ms.SkipAsync<GeneralPoolHeader>().ConfigureAwait(false);
        typeStringPool = await ReadStringPoolAsync(ms).ConfigureAwait(false);

        // goto next pool
        await ms.SkipAsync(headerSuffix.keyStrings - (ms.Position - lastPosition))
            .ConfigureAwait(false);

        Debug.WriteLine("Key strings:");
        var keyStringHeader = await ms.ReadStructAsync<GeneralPoolHeader>().ConfigureAwait(false);
        keyStringPool = await ReadStringPoolAsync(ms).ConfigureAwait(false);
        return keyStringHeader;
    }

    private void AddKeyValuePairToResponseMap(string resId, string? value)
    {
        var upperResId = resId.ToUpper();

        if (!responseMap.TryGetValue(upperResId, out var valueList))
        {
            valueList = new List<string?>();
            responseMap.Add(upperResId, valueList);
        }

        valueList.Add(value);
    }

    private async Task ExtractTypesAsync(Stream ms, GeneralPoolHeader header)
    {
        var headerSuffix = await ms.ReadStructAsync<TypeSuffix>().ConfigureAwait(false);

        Dictionary<string, int> refKeys = new Dictionary<string, int>();

        // Skip the config data
        await ms.SkipAsync(header.headerSize - Marshal.SizeOf<TypeSuffix>() - Marshal.SizeOf<GeneralPoolHeader>())
            .ConfigureAwait(false);

        if (header.headerSize + headerSuffix.entryCount * 4 != headerSuffix.entriesStart)
        {
            throw new Exception("HeaderSize, entryCount and entriesStart are not valid.");
        }

        // Start to get entry indices
        int[] entryIndices = await ms.ReadBlockAsync<int>(headerSuffix.entryCount)
            .ConfigureAwait(false);

        // Get entries
        for (int i = 0; i < headerSuffix.entryCount; ++i)
        {
            await ProcessEntryAsync(ms, entryIndices, i, headerSuffix, refKeys).ConfigureAwait(false);
        }

        BuildResponseMap(refKeys);
    }

    private void BuildResponseMap(Dictionary<string, int> refKeys)
    {
        HashSet<string> refKs = new HashSet<string>(refKeys.Keys);

        foreach (string refK in refKs)
        {
            IList<string?>? values = null;
            string key = $"@{refKeys[refK].ToString("X4").ToUpper()}";
            if (responseMap.ContainsKey(key))
            {
                values = responseMap[key];
            }

            if (values == null)
            {
                continue;
            }

            foreach (string? value in values)
            {
                AddKeyValuePairToResponseMap("@" + refK, value);
            }
        }
    }

    private async Task ProcessEntryAsync(Stream ms, int[] entryIndices, int i, TypeSuffix headerSuffix,
        Dictionary<string, int> refKeys)
    {
        if (entryIndices[i] == -1)
        {
            return;
        }

        int resourceId = (package_id << 24) | (headerSuffix.id << 16) | i;

        var entryHeader = await ms.ReadStructAsync<EntryHeader>().ConfigureAwait(false);

        // Get the value (simple) or map (complex)
        int FLAG_COMPLEX = 0x0001;

        if ((entryHeader.entryFlag & FLAG_COMPLEX) == FLAG_COMPLEX)
        {
            var entry = await ms.ReadStructAsync<ComplexEntryBody>().ConfigureAwait(false);

            await ms.ReadStructAsync<ComplexEntryItemBody>(entry.entryCount).ConfigureAwait(false);

            Debug.WriteLine(
                $"Entry 0x{resourceId:X4}, key: {keyStringPool[entryHeader.entryKey]}, complex value, not printed."
            );
        }
        else
        {
            // Simple case
            await ProcessSimpleEntryBodyAsync(ms, refKeys, resourceId, entryHeader).ConfigureAwait(false);
        }
    }

    private async Task ProcessSimpleEntryBodyAsync(Stream ms, IDictionary<string, int> refKeys, int resourceId,
        EntryHeader entryHeader)
    {
        var entry = await ms.ReadStructAsync<SimpleEntryBody>()
            .ConfigureAwait(false);

        string idStr = resourceId.ToString("X4");
        string keyStr = keyStringPool[entryHeader.entryKey];
        string? data = null;

        Debug.WriteLine(
            $"Entry 0x{idStr}, key: {keyStr}, simple value type: "
        );

        var key = int.Parse(idStr, System.Globalization.NumberStyles.HexNumber);

        if (!entryMap.TryGetValue(key, out var entryArr))
        {
            entryArr = new List<string>();
            entryMap.Add(key, entryArr);
        }

        entryArr.Add(keyStr);

        switch (entry.valueDataType)
        {
            case TYPE_STRING:
                data = valueStringPool[entry.valueData];
                Debug.WriteLine($", data: {data}");
                break;
            case TYPE_REFERENCE:
                refKeys.Add(idStr, entry.valueData);
                break;
            default:
                data = entry.valueData.ToString();
                Debug.WriteLine($", data: {data}");
                break;
        }

        AddKeyValuePairToResponseMap("@" + idStr, data);
    }

    private async Task<string[]> ReadStringPoolAsync(Stream ms)
    {
        long innerInitialOffset = ms.Position - Marshal.SizeOf<GeneralPoolHeader>();

        var headerSuffix = await ms.ReadStructAsync<StringPoolHeaderSuffix>().ConfigureAwait(false);

        bool isUtf8String = (headerSuffix.flags & 256) != 0;

        var offsets = await ms.ReadBlockAsync<int>(headerSuffix.stringCount).ConfigureAwait(false);

        string[] strings = new string[headerSuffix.stringCount];

        for (int i = 0; i < headerSuffix.stringCount; i++)
        {
            long pos = headerSuffix.stringsStart + offsets[i] + innerInitialOffset;
            await ms.SkipAsync(pos - ms.Position).ConfigureAwait(false);

            strings[i] = await ReadStringAsync(isUtf8String, ms).ConfigureAwait(false);
        }

        return strings;
    }

    private static async Task<string> ReadStringAsync(bool isUtf8String, Stream ms)
    {
        string newString = String.Empty;
        if (isUtf8String)
        {
            var lengthOfUtf8String = ReadUtf8StringLength(ms);

            if (lengthOfUtf8String > 0)
            {
                var utf8Data = await ms.ReadBlockAsync(lengthOfUtf8String).ConfigureAwait(false);
                newString = Encoding.UTF8.GetString(utf8Data);
            }
        }
        else // UTF_16
        {
            var lengthOfUtf16String = ReadUtf816StringLength(ms);

            if (lengthOfUtf16String > 0)
            {
                var utf16Data = await ms.ReadBlockAsync(lengthOfUtf16String * 2)
                    .ConfigureAwait(false);
                newString = Encoding.Unicode.GetString(utf16Data);
            }
        }

        return newString;
    }

    private static int ReadUtf816StringLength(Stream ms)
    {
        int u16len = ReadUInt16();
        if ((u16len & 0x8000) != 0)
        {
            // larger than 32768
            u16len = ((u16len & 0x7FFF) << 16) + ReadUInt16();
        }

        return u16len;

        ushort ReadUInt16()
        {
            Span<byte> ushortData = stackalloc byte[2];
            var bytesRead = ms.Read(ushortData);
            if (bytesRead != 2)
            {
                throw new Exception("Failed to read ushort from stream");
            }

            return BinaryPrimitives.ReadUInt16LittleEndian(ushortData);
        }
    }

    private static int ReadUtf8StringLength(Stream ms)
    {
        int u16len = ms.ReadByte(); // u16len
        if ((u16len & 0x80) != 0)
        {
            // larger than 128
            // u16len = ((u16len & 0x7F) << 8) + ms.ReadByte();
            ms.ReadByte();
        }

        int u8len = ms.ReadByte(); // u8len
        if ((u8len & 0x80) != 0)
        {
            // larger than 128
            u8len = ((u8len & 0x7F) << 8) + ms.ReadByte();
        }

        return u8len;
    }

    private async Task ExtractTypeSpecAsync(Stream ms, string[] stringPool)
    {
        var header = await ms.ReadStructAsync<TypeSpecSuffix>().ConfigureAwait(false);

        Debug.WriteLine("Processing type spec {0}", stringPool[header.id - 1]);

        await ms.SkipAsync<int>(header.entryCount).ConfigureAwait(false);
    }
}
