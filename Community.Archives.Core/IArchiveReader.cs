namespace Community.Archives.Core;

public interface IArchiveReader
{
    public readonly struct ArchiveMetaData : IEquatable<ArchiveMetaData>
    {
        public string Package { get; init; }
        public string Version { get; init; }
        public string Architecture { get; init; }
        public string Description { get; init; }

        public IReadOnlyDictionary<string, string> AllFields { get; init; }

        public override bool Equals(object? obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is ArchiveMetaData)
            {
                return Equals((ArchiveMetaData)obj);
            }

            return false;
        }

        public bool Equals(ArchiveMetaData other)
        {
            return Package == other.Package
                   && Version == other.Version
                   && Architecture == other.Architecture
                   && Description == other.Description
                   && AllFields.AreEqual(other.AllFields);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Package, Version, Architecture, Description, AllFields);
        }
    }

    /// <summary>
    /// Enumerate the FileEntries in the given archive asynchronously.
    /// </summary>
    /// <param name="stream">The archive file stream</param>
    /// <returns>The ArchiveEntry found</returns>
    IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(Stream stream, params string[] regexMatcher);

    Task<ArchiveMetaData> GetMetaDataAsync(Stream stream);

    public bool SupportsMetaData { get; }

    public const string MATCH_ALL_FILES = ".*";
}
