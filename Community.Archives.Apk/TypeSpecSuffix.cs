using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct TypeSpecSuffix
{
    public int id;
    public byte res0;
    public short res1;
    public int entryCount;
}
