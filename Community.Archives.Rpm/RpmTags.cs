namespace Community.Archives.Rpm;

public struct RpmTags
{
    //[RpmTag(62, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] HeaderSignatures;

    //[RpmTag(63, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] HeaderImmutable;

    //[RpmTag(100, IndexType.RPM_I18NSTRING_TYPE, 0, false)]
    //public string[] HeaderI18nTable;

    [RpmTag(1000, IndexType.RPM_INT32_TYPE, 1, true)]
    public int SignatureTagSize;

    [RpmTag(1007, IndexType.RPM_INT32_TYPE, 1, false)]
    public int SignatureTagPayloadSize;

    //[RpmTag(269, IndexType.RPM_STRING_TYPE, 1, false)]
    //public string SignatureTagSha1;

    //[RpmTag(1004, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] SignatureTagMd5;

    //[RpmTag(267, IndexType.RPM_BIN_TYPE, 65, false)]
    //public byte[] SignatureTagDsa;

    //[RpmTag(268, IndexType.RPM_BIN_TYPE, 1, false)]
    //public byte SignatureTagRsa;

    //[RpmTag(1002, IndexType.RPM_BIN_TYPE, 1, false)]
    //public byte SignatureTagPgp;

    //[RpmTag(1005, IndexType.RPM_BIN_TYPE, 65, false)]
    //public byte SignatureTagGpg;

    [RpmTag(1000, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Name = String.Empty;

    [RpmTag(1001, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Version = String.Empty;

    [RpmTag(1002, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Release = String.Empty;

    [RpmTag(1004, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Summary = String.Empty;

    [RpmTag(1005, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Description = String.Empty;

    [RpmTag(1009, IndexType.RPM_INT32_TYPE, 1, true)]
    public int Size;

    [RpmTag(1010, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Distribution = String.Empty;

    [RpmTag(1011, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Vendor = String.Empty;

    [RpmTag(1014, IndexType.RPM_STRING_TYPE, 1, true)]
    public string License = String.Empty;

    [RpmTag(1015, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Packager = String.Empty;

    [RpmTag(1016, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Group = String.Empty;

    [RpmTag(1020, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Url = String.Empty;

    [RpmTag(1021, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Os = String.Empty;

    [RpmTag(1022, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Architecture = String.Empty;

    [RpmTag(1044, IndexType.RPM_STRING_TYPE, 1, false)]
    public string SourceRpm = String.Empty;

    [RpmTag(1046, IndexType.RPM_INT32_TYPE, 1, false)]
    public int ArchiveSize;

    [RpmTag(1064, IndexType.RPM_STRING_TYPE, 1, false)]
    public string RpmVersion = String.Empty;

    [RpmTag(1094, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Cookie = String.Empty;

    [RpmTag(1123, IndexType.RPM_STRING_TYPE, 1, false)]
    public string DistUrl = String.Empty;

    [RpmTag(1124, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadFormat = String.Empty;

    [RpmTag(1125, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadCompressor = String.Empty;

    [RpmTag(1126, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadFlags = String.Empty;

    public IReadOnlyDictionary<string, string> GetFields()
    {
        var thisRef = __makeref(this);
        var fieldDict = new Dictionary<string, string>(StringComparer.InvariantCulture);

        foreach (var fieldInfo in GetType().GetFields())
        {
            var value = fieldInfo.GetValueDirect(thisRef);
            if (value != null && !ReferenceEquals(value, string.Empty))
            {
                fieldDict.Add(fieldInfo.Name, value.ToString()!);
            }
        }

        return fieldDict;
    }
}
