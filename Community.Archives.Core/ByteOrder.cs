namespace Community.Archives.Core;

/// <summary>
/// Specifies the byte order of a field on a struct.
/// If applied on a struct it specified the byte order of all fields.
/// </summary>
public enum ByteOrder
{
    /// <summary>
    /// The field value is stored in big-endian (network) order
    /// </summary>
    BigEndian,
    /// <summary>
    /// The field value is stored in little-endian order
    /// </summary>
    LittleEndian,
}
