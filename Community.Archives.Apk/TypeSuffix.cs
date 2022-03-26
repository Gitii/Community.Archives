using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct TypeSuffix
{
    public byte id;
    public byte res0;
    public short res1;
    public int entryCount;
    public int entriesStart;
    public int configSize;
}
