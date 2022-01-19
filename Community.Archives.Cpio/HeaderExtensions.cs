using System.Text;

namespace Community.Archives.Cpio;

public static class HeaderExtensions
{
    public static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("070701");

    public static unsafe bool IsValid(this Header header)
    {
        for (var i = 0; i < MAGIC.Length; i++)
        {
            if (header.c_magic[i] != MAGIC[i])
            {
                return false;
            }
        }

        return true;
    }

    public static FileMode GetFileMode(this Header header)
    {
        var mode = header.c_mode.DecodeStringAsLong(true);
        long type = mode >> 9; // drop permission flags (octal 777)

        return (FileMode)type;
    }
}
