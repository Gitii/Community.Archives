using System.IO.Compression;
using Community.Archives.Core;
using Community.Archives.Cpio;

namespace Community.Archives.Rpm;

public class RpmArchiveReader : IArchiveReader
{
    public virtual async IAsyncEnumerable<ArchiveEntry> GetFileEntriesAsync(
        Stream stream,
        params string[] regexMatcher
    )
    {
        var lead = await stream.ReadStructAsync<RpmLead>().ConfigureAwait(false);
        AssertLeadIsValid(lead);

        await stream.AlignToBoundaryAsync(8).ConfigureAwait(false);
        var signature = await stream.ReadStructAsync<RpmHeader>().ConfigureAwait(false);

        await stream.SkipAsync<RpmHeaderIndex>(signature.nindex).ConfigureAwait(false);
        await stream.SkipAsync(signature.hsize).ConfigureAwait(false);

        await stream.AlignToBoundaryAsync(8).ConfigureAwait(false);
        var header = await stream.ReadStructAsync<RpmHeader>().ConfigureAwait(false);
        await stream.SkipAsync<RpmHeaderIndex>(header.nindex).ConfigureAwait(false);
        await stream.SkipAsync(header.hsize).ConfigureAwait(false);

        var payload = await stream
            .ReadStreamAsync((int)(stream.Length - stream.Position))
            .ConfigureAwait(false);

        var cpio = new CpioArchiveReader();

        var decompressingStream = new GZipStream(payload, CompressionMode.Decompress, false);
        await using var _ = decompressingStream.ConfigureAwait(false);
        await foreach (
            var entry in cpio.GetFileEntriesAsync(decompressingStream, regexMatcher)
                .ConfigureAwait(false)
        )
        {
            yield return entry;
        }
    }

    public virtual async Task<IArchiveReader.ArchiveMetaData> GetMetaDataAsync(Stream stream)
    {
        var lead = await stream.ReadStructAsync<RpmLead>().ConfigureAwait(false);
        AssertLeadIsValid(lead);

        await stream.AlignToBoundaryAsync(8).ConfigureAwait(false);
        var signature = await stream.ReadStructAsync<RpmHeader>().ConfigureAwait(false);

        await stream.SkipAsync<RpmHeaderIndex>(signature.nindex).ConfigureAwait(false);
        await stream.SkipAsync(signature.hsize).ConfigureAwait(false);

        await stream.AlignToBoundaryAsync(8).ConfigureAwait(false);
        var header = await stream.ReadStructAsync<RpmHeader>().ConfigureAwait(false);
        var headerEntries = await stream
            .ReadStructAsync<RpmHeaderIndex>(header.nindex)
            .ConfigureAwait(false);

        var headerEntriesData = await stream.ReadBlockAsync(header.hsize).ConfigureAwait(false);
        var tag = RpmTagsExtensions.Parse(headerEntries, headerEntriesData);

        return new IArchiveReader.ArchiveMetaData()
        {
            Package = lead.GetName(),
            Description = tag.Description,
            Version = tag.Version,
            Architecture = tag.Architecture,
            AllFields = tag.GetFields(),
        };
    }

    public virtual bool SupportsMetaData { get; } = true;

    private void AssertLeadIsValid(in RpmLead lead)
    {
        if (!lead.IsValid())
        {
            throw new Exception("The stream is not a valid rpm archive!");
        }
    }
}
