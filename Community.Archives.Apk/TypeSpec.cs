using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct TypeSpec
{
    public GeneralPoolHeader header;
    public TypeSpecSuffix suffix;
}
