using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Apk;

[StructLayout(LayoutKind.Sequential)]
internal struct PackageHeader
{
    public GeneralPoolHeader header;
    public PackageHeaderSuffix suffix;
}
