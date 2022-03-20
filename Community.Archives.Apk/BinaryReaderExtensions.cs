namespace Community.Archives.Apk;

public static class BinaryReaderExtensions
{
    public static short Readt16(this BinaryReader reader)
    {
        return reader.ReadInt16();
    }

    public static int Readt32(this BinaryReader reader)
    {
        return reader.ReadInt32();
    }
}
