using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Ar;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public unsafe struct Header
{
    public FixedString8 Magic;
};
