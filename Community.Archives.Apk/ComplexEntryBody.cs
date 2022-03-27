using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct ComplexEntryBody
{
    public int entryParent;
    public int entryCount;
}
