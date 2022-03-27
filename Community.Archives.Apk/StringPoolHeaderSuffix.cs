using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct StringPoolHeaderSuffix
{
    public int stringCount;
    public int styleCount;
    public int flags;
    public int stringsStart;
    public int stylesStart;
}
