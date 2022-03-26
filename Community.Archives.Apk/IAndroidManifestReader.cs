using System.Xml.Linq;

namespace Community.Archives.Apk;

/// <summary> Unpacks and reads data from a compressed apk manifest file. </summary>
public interface IAndroidManifestReader
{
    /// <summary>Returns the uncompressed Xml Manifest or <c>null</c> if <see cref="AndroidManifestReader.ReadAsync"/> or <see cref="AndroidManifestReader.Read"/> hasn't been called, yet.</summary>
    XDocument? Manifest { get; }

    /// <summary>
    /// Reads all bytes from the passed in stream, reads the document from the data and returns the uncompressed xml manifest.
    /// </summary>
    /// <param name="stream">The input stream</param>
    /// <returns>The uncompressed xml document.</returns>
    /// <exception cref="Exception">Could not read all bytes from input stream.</exception>
    Task<XDocument> ReadAsync(Stream stream);

    /// <summary>
    /// Reads the document from the data and returns the uncompressed xml manifest.
    /// </summary>
    /// <param name="data">The data from which the document is being read.</param>
    /// <returns>The uncompressed xml document.</returns>
    XDocument Read(byte[] data);
}
