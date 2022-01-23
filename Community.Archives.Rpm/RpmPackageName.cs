namespace Community.Archives.Rpm;

/// <summary>
/// A record struct that holds different parts of a rpm package name.
/// </summary>
public record struct RpmPackageName
{
    public RpmPackageName()
    {
        Name = String.Empty;
        Version = String.Empty;
        Release = String.Empty;
        Architecture = null;
    }

    public RpmPackageName(string name, string version, string release, string? architecture = null)
    {
        Name = name;
        Version = version;
        Release = release;
        Architecture = architecture;
    }

    /// <summary>
    /// A name describing the packaged software.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The version of the packaged software.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// The number of times this version of the software has been packaged
    /// </summary>
    public string Release { get; set; }

    /// <summary>
    /// a shorthand name describing the type of computer hardware the
    /// packaged software is meant to run on. It may also be the string src,
    /// or nosrc. Both of these strings indicate the file is an RPM source package.
    /// The nosrc string means that the file contains only package building files,
    /// while the src string means the file contains the necessary package
    /// building files and the software's source code.
    /// </summary>
    public string? Architecture { get; set; }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Architecture))
        {
            return $"{Name}-{Version}-{Release}";
        }

        return $"{Name}-{Version}-{Release}-{Architecture}";
    }
}
