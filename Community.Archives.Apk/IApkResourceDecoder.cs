namespace Community.Archives.Apk;

public interface IApkResourceDecoder
{
    Task<IDictionary<string, IList<string>>> DecodeAsync(byte[] data);
    Task<IDictionary<string, IList<string>>> DecodeAsync(Stream stream);
}
