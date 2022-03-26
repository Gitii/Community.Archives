using System.Runtime.InteropServices;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct ComplexEntryItemBody
{
    public int refName;
    public short valueSize;
    public byte valueRes0;
    public byte value_dataType;
    public int valueData;
}
