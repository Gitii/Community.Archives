using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct GeneralPoolHeader
{
    public short type;
    public short headerSize;
    public int size;
}
