using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct PackageHeaderSuffix
{
    public int id;
    public unsafe fixed byte name[256];
    public int typeStrings;
    public int lastPublicType;
    public int keyStrings;
    public int lastPublicKey;
}
