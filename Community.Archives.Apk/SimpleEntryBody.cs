using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct SimpleEntryBody
{
    public short valueSize;
    public byte valueRes0;
    public byte valueDataType;
    public int valueData;
}
