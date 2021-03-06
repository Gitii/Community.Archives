using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Rpm;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Endianness(ByteOrder.BigEndian)]
internal struct RpmHeaderIndex
{
    public int tag;
    public int type;
    public int offset;
    public int count;

    public override string ToString()
    {
        return $"Tag = {tag}; Type = {type}; Offset = {offset}; Count = {count}";
    }
};
