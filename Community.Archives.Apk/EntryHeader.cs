using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct EntryHeader
{
    public short entrySize;
    public short entryFlag;
    public int entryKey;
}
