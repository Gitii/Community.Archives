using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct StringPoolHeader
{
    public GeneralPoolHeader generalHeader;
    public StringPoolHeaderSuffix suffix;
}
