using System;
using System.Collections.Generic;
using System.Text;

namespace Community.Archives.Rpm;

/// <summary>
/// A simple parser that splits a rpm package file name (or package name), into it's different parts:
/// The name of the package, it's version, the release number and (optionally) the architecture.
/// </summary>
public class RpmPackageNameParser
{
    /// <summary>
    /// Checks whether the file name follows the specific naming convention.
    /// </summary>
    /// <param name="packageFileName">The file name to check (without extension).</param>
    /// <returns><c>true</c> if it's valid, otherwise <c>false</c>.</returns>
    public bool IsValid(string packageFileName)
    {
        return TryParse(packageFileName, out _);
    }

    /// <summary>
    /// Splits a rpm package file name (or package name), into it's different parts
    /// </summary>
    /// <param name="packageFileName">The file name to split (without extension).</param>
    /// <param name="packageName">The different split parts.</param>
    /// <returns><c>true</c> if it's valid, otherwise <c>false</c>.</returns>
    public bool TryParse(string packageFileName, out RpmPackageName? packageName)
    {
        if (string.IsNullOrWhiteSpace(packageFileName))
        {
            packageName = null;
            return false;
        }

        string[] parts = packageFileName.Trim().Split('-');
        if (parts.Length is not (3 or 4) || parts.Any((s) => s.Length == 0))
        {
            packageName = null;
            return false;
        }

        packageName = new RpmPackageName()
        {
            Name = parts[0],
            Version = parts[1],
            Release = parts[2],
            Architecture = parts.Length == 4 ? parts[3] : null,
        };
        return true;
    }
}
