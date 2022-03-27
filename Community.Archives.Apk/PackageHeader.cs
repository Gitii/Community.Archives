using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct PackageHeader
{
    public GeneralPoolHeader header;
    public PackageHeaderSuffix suffix;
}
