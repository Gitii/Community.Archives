using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Community.Archives.Ar;

public class ArchiveStringTable
{
    private Regex NameReferenceRegex = new Regex(
        "^[/]\\d+$",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    private byte[] _data = Array.Empty<byte>();

    public void Load(byte[] data)
    {
        _data = data;
    }

    /// <summary>
    /// Dereferences a string if it's a valid reference.
    /// Otherwise the string will be returned unchanged.
    /// </summary>
    public string Dereference(string fileName)
    {
        if (NameReferenceRegex.IsMatch(fileName))
        {
            var offset = int.Parse(
                fileName.AsSpan().Slice(1),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture
            );

            return ExtractString(offset);
        }

        return fileName;
    }

    private string ExtractString(int offset)
    {
        for (int i = offset; i < _data.Length - 1; i++)
        {
            if (_data[i] == 0x2F && _data[i + 1] == 0x0A)
            {
                return Encoding.ASCII.GetString(_data.AsSpan().Slice(offset, i - offset));
            }
        }

        throw new Exception($"Could not find a string at offset {offset}");
    }
}
