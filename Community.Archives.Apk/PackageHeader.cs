using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct PackageHeader
{
    public short type;
    public short headerSize;
    public int size;
    public int id;
    public unsafe fixed char name[256];
    public int typeStrings;
    public int lastPublicType;
    public int keyStrings;
    public int lastPublicKey;
}
