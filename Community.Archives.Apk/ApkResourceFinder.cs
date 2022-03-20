using System.Diagnostics;
using System.Text;
using Community.Archives.Core;

namespace Community.Archives.Apk;

#pragma warning disable MA0051 // Method is too long
#pragma warning disable IDE0059 // Method is too long

public class ApkResourceFinder
{
    private const short RES_STRING_POOL_TYPE = 0x0001;
    private const short RES_TABLE_TYPE = 0x0002;
    private const short RES_TABLE_PACKAGE_TYPE = 0x0200;
    private const short RES_TABLE_TYPE_TYPE = 0x0201;
    private const short RES_TABLE_TYPE_SPEC_TYPE = 0x0202;

    string[] valueStringPool = null;
    string[] typeStringPool = null;
    string[] keyStringPool = null;

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
    static byte TYPE_STRING = 0x03;

    private IDictionary<string, IList<string>> responseMap = new Dictionary<
        string,
        IList<string>
    >();

    Dictionary<int, List<string>> entryMap = new Dictionary<int, List<string>>();

    public Task<IDictionary<string, IList<string>>> ProcessResourceTableAsync(byte[] data)
    {
        responseMap = new Dictionary<string, IList<string>>();

        using MemoryStream ms = new MemoryStream(data);
        using BinaryReader br = new BinaryReader(ms);

        return ExtractDataAsync(br);
    }

    private async Task<IDictionary<string, IList<string>>> ExtractDataAsync(
        BinaryReader binaryReader
    )
    {
        short type = binaryReader.Readt16();
        short headerSize = binaryReader.Readt16();
        int size = binaryReader.Readt32();
        int packageCount = binaryReader.Readt32();

        if (type != RES_TABLE_TYPE)
        {
            throw new Exception("No RES_TABLE_TYPE found!");
        }

        if (size != binaryReader.BaseStream.Length)
        {
            throw new Exception("The buffer size not matches to the resource table size.");
        }

        var tuple = await ExtractPoolsAndPackagesAsync(binaryReader).ConfigureAwait(false);
        var realStringPoolCount = tuple.Item1;
        var realPackageCount = tuple.Item2;

        if (realStringPoolCount != 1)
        {
            throw new Exception("More than 1 string pool found!");
        }

        if (realPackageCount != packageCount)
        {
            throw new Exception("Real package count not equals the declared count.");
        }

        return responseMap;
    }

    private async Task<Tuple<int, int>> ExtractPoolsAndPackagesAsync(BinaryReader binaryReader)
    {
        var actualStringPoolCount = 0;
        var actualPackageCount = 0;

        while (true)
        {
            long pos = binaryReader.BaseStream.Position;
            short t = binaryReader.Readt16();
            short hs = binaryReader.Readt16();
            int s = binaryReader.Readt32();

            if (t == RES_STRING_POOL_TYPE)
            {
                if (actualStringPoolCount == 0)
                {
                    // Only the first string pool is processed.
                    Debug.WriteLine("Processing the string pool ...");

                    byte[] buffer = new byte[s];
                    binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    buffer = binaryReader.ReadBytes(s);
                    //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                    valueStringPool = ExtractStringPool(buffer);
                }

                actualStringPoolCount++;
            }
            else if (t == RES_TABLE_PACKAGE_TYPE)
            {
                // Process the package
                Debug.WriteLine("Processing package {0} ...", actualPackageCount);

                byte[] buffer = new byte[s];
                binaryReader.BaseStream.Seek(pos, SeekOrigin.Begin);
                buffer = binaryReader.ReadBytes(s);
                //br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);
                await ExtractPackageAsync(buffer).ConfigureAwait(false);

                actualPackageCount++;
            }
            else
            {
                throw new InvalidOperationException("Unsupported Type");
            }

            binaryReader.BaseStream.Seek(pos + (long)s, SeekOrigin.Begin);
            if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length)
            {
                break;
            }
        }

        return Tuple.Create(actualStringPoolCount, actualPackageCount);
    }

    private async Task ExtractPackageAsync(byte[] data)
    {
        long lastPosition = 0;
        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                var header = await ms.ReadStructAsync<PackageHeader>().ConfigureAwait(false);

                package_id = header.id;

                if (header.typeStrings != header.headerSize)
                {
                    throw new Exception(
                        "TypeStrings must immediately follow the package structure header."
                    );
                }

                Debug.WriteLine("Type strings:");
                lastPosition = br.BaseStream.Position;
                br.BaseStream.Seek(header.typeStrings, SeekOrigin.Begin);
                byte[] bbTypeStrings = br.ReadBytes(
                    (int)(br.BaseStream.Length - br.BaseStream.Position)
                );
                br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                typeStringPool = ExtractStringPool(bbTypeStrings);

                Debug.WriteLine("Key strings:");

                br.BaseStream.Seek(header.keyStrings, SeekOrigin.Begin);
                short key_type = br.Readt16();
                short key_headerSize = br.Readt16();
                int key_size = br.Readt32();

                lastPosition = br.BaseStream.Position;
                br.BaseStream.Seek(header.keyStrings, SeekOrigin.Begin);
                byte[] bbKeyStrings = br.ReadBytes(
                    (int)(br.BaseStream.Length - br.BaseStream.Position)
                );
                br.BaseStream.Seek(lastPosition, SeekOrigin.Begin);

                keyStringPool = ExtractStringPool(bbKeyStrings);

                // Iterate through all chunks
                //
                int typeSpecCount = 0;
                int typeCount = 0;

                br.BaseStream.Seek((header.keyStrings + key_size), SeekOrigin.Begin);

                while (true)
                {
                    int pos = (int)br.BaseStream.Position;
                    short t = br.Readt16();
                    short hs = br.Readt16();
                    int s = br.Readt32();

                    if (t == RES_TABLE_TYPE_SPEC_TYPE)
                    {
                        // Process the string pool
                        byte[] buffer = new byte[s];
                        br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        buffer = br.ReadBytes(s);

                        ExtractTypeSpec(buffer);

                        typeSpecCount++;
                    }
                    else if (t == RES_TABLE_TYPE_TYPE)
                    {
                        // Process the package
                        byte[] buffer = new byte[s];
                        br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        buffer = br.ReadBytes(s);

                        ExtractTypes(buffer);

                        typeCount++;
                    }

                    br.BaseStream.Seek(pos + s, SeekOrigin.Begin);
                    if (br.BaseStream.Position == br.BaseStream.Length)
                    {
                        break;
                    }
                }

                return;
            }
        }
    }

    private void AddKeyValuePairToResponseMap(string resId, string value)
    {
        IList<string> valueList = null;
        if (responseMap.ContainsKey(resId.ToUpper()))
        {
            valueList = responseMap[resId.ToUpper()];
        }

        if (valueList == null)
        {
            valueList = new List<string>();
        }

        valueList.Add(value);
        if (responseMap.ContainsKey(resId.ToUpper()))
        {
            responseMap[resId.ToUpper()] = valueList;
        }
        else
        {
            responseMap.Add(resId.ToUpper(), valueList);
        }

        return;
    }

    private void ExtractTypes(byte[] typeData)
    {
        using (MemoryStream ms = new MemoryStream(typeData))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                short type = br.Readt16();
                short headerSize = br.Readt16();
                int size = br.Readt32();
                byte id = br.ReadByte();
                byte res0 = br.ReadByte();
                short res1 = br.Readt16();
                int entryCount = br.Readt32();
                int entriesStart = br.Readt32();

                Dictionary<string, int> refKeys = new Dictionary<string, int>();

                int config_size = br.Readt32();

                // Skip the config data
                br.BaseStream.Seek(headerSize, SeekOrigin.Begin);

                if (headerSize + entryCount * 4 != entriesStart)
                {
                    throw new Exception("HeaderSize, entryCount and entriesStart are not valid.");
                }

                // Start to get entry indices
                int[] entryIndices = new int[entryCount];
                for (int i = 0; i < entryCount; ++i)
                {
                    entryIndices[i] = br.Readt32();
                }

                // Get entries
                for (int i = 0; i < entryCount; ++i)
                {
                    if (entryIndices[i] == -1)
                    {
                        continue;
                    }

                    int resource_id = (package_id << 24) | (id << 16) | i;

                    long pos = br.BaseStream.Position;
                    short entry_size = br.Readt16();
                    short entry_flag = br.Readt16();
                    int entry_key = br.Readt32();

                    // Get the value (simple) or map (complex)
                    int FLAG_COMPLEX = 0x0001;

                    if ((entry_flag & FLAG_COMPLEX) == 0)
                    {
                        // Simple case
                        short value_size = br.Readt16();
                        byte value_res0 = br.ReadByte();
                        byte value_dataType = br.ReadByte();
                        int value_data = br.Readt32();

                        string idStr = resource_id.ToString("X4");
                        string keyStr = keyStringPool[entry_key];
                        string data = null;

                        Debug.WriteLine(
                            "Entry 0x" + idStr + ", key: " + keyStr + ", simple value type: "
                        );

                        List<string> entryArr = null;
                        if (
                            entryMap.ContainsKey(
                                int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)
                            )
                        )
                        {
                            entryArr = entryMap[
                                int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)
                            ];
                        }

                        if (entryArr == null)
                        {
                            entryArr = new List<string>();
                        }

                        entryArr.Add(keyStr);
                        if (
                            entryMap.ContainsKey(
                                int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)
                            )
                        )
                        {
                            entryMap[
                                int.Parse(idStr, System.Globalization.NumberStyles.HexNumber)
                            ] = entryArr;
                        }
                        else
                        {
                            entryMap.Add(
                                int.Parse(idStr, System.Globalization.NumberStyles.HexNumber),
                                entryArr
                            );
                        }

                        if (value_dataType == TYPE_STRING)
                        {
                            data = valueStringPool[value_data];
                            Debug.WriteLine(", data: " + valueStringPool[value_data] + "");
                        }
                        else if (value_dataType == TYPE_REFERENCE)
                        {
                            string hexIndex = value_data.ToString("X4");
                            refKeys.Add(idStr, value_data);
                        }
                        else
                        {
                            data = value_data.ToString();
                            Debug.WriteLine(", data: " + value_data + "");
                        }

                        // if (inReqList("@" + idStr)) {
                        AddKeyValuePairToResponseMap("@" + idStr, data);
                    }
                    else
                    {
                        int entry_parent = br.Readt32();
                        int entry_count = br.Readt32();

                        for (int j = 0; j < entry_count; ++j)
                        {
                            int ref_name = br.Readt32();
                            short value_size = br.Readt16();
                            byte value_res0 = br.ReadByte();
                            byte value_dataType = br.ReadByte();
                            int value_data = br.Readt32();
                        }

                        Debug.WriteLine(
                            "Entry 0x"
                            + resource_id.ToString("X4")
                            + ", key: "
                            + keyStringPool[entry_key]
                            + ", complex value, not printed."
                        );
                    }
                }

                HashSet<string> refKs = new HashSet<string>(refKeys.Keys);

                foreach (string refK in refKs)
                {
                    IList<string> values = null;
                    if (responseMap.ContainsKey("@" + refKeys[refK].ToString("X4").ToUpper()))
                    {
                        values = responseMap["@" + refKeys[refK].ToString("X4").ToUpper()];
                    }

                    if (values != null)
                    {
                        foreach (string value in values)
                        {
                            AddKeyValuePairToResponseMap("@" + refK, value);
                        }
                    }
                }

                return;
            }
        }
    }

    private string[] ExtractStringPool(byte[] data)
    {
        long lastPosition = 0;

        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                short type = br.Readt16();
                short headerSize = br.Readt16();
                int size = br.Readt32();
                int stringCount = br.Readt32();
                int styleCount = br.Readt32();
                int flags = br.Readt32();
                int stringsStart = br.Readt32();
                int stylesStart = br.Readt32();

                bool isUTF_8 = (flags & 256) != 0;

                int[] offsets = new int[stringCount];
                for (int i = 0; i < stringCount; ++i)
                {
                    offsets[i] = br.Readt32();
                }

                string[] strings = new string[stringCount];

                for (int i = 0; i < stringCount; i++)
                {
                    int pos = stringsStart + offsets[i];
                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                    strings[i] = "";
                    if (isUTF_8)
                    {
                        int u16len = br.ReadByte(); // u16len
                        if ((u16len & 0x80) != 0)
                        {
                            // larger than 128
                            u16len = ((u16len & 0x7F) << 8) + br.ReadByte();
                        }

                        int u8len = br.ReadByte(); // u8len
                        if ((u8len & 0x80) != 0)
                        {
                            // larger than 128
                            u8len = ((u8len & 0x7F) << 8) + br.ReadByte();
                        }

                        if (u8len > 0)
                        {
                            strings[i] = Encoding.UTF8.GetString(br.ReadBytes(u8len));
                        }
                        else
                        {
                            strings[i] = "";
                        }
                    }
                    else // UTF_16
                    {
                        int u16len = br.ReadUInt16();
                        if ((u16len & 0x8000) != 0)
                        {
                            // larger than 32768
                            u16len = ((u16len & 0x7FFF) << 16) + br.ReadUInt16();
                        }

                        if (u16len > 0)
                        {
                            strings[i] = Encoding.Unicode.GetString(br.ReadBytes(u16len * 2));
                        }
                    }

                    Debug.WriteLine("Parsed value: {0}", strings[i]);
                }

                return strings;
            }
        }
    }

    private void ExtractTypeSpec(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader br = new BinaryReader(ms))
            {
                short type = br.Readt16();
                short headerSize = br.Readt16();
                int size = br.Readt32();
                byte id = br.ReadByte();
                byte res0 = br.ReadByte();
                short res1 = br.Readt16();
                int entryCount = br.Readt32();

                Debug.WriteLine("Processing type spec {0}", typeStringPool[id - 1]);

                int[] flags = new int[entryCount];
                for (int i = 0; i < entryCount; ++i)
                {
                    flags[i] = br.Readt32();
                }

                return;
            }
        }
    }
}
