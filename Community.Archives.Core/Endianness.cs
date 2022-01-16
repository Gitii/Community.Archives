namespace Community.Archives.Core;

/// <summary>
/// A attribute that either specifies the byte order of all fields in a struct or on an individual field.
/// If the target of the attribute is a struct, field targets will be ignored.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class Endianness : Attribute
{
    public Endianness(ByteOrder byteOrder)
    {
        ByteOrder = byteOrder;
    }

    /// <summary>
    /// Byte order of the field or struct.
    /// </summary>
    public ByteOrder ByteOrder { get; }
}
