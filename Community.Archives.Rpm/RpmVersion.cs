// Copyright (c) SAS Institute Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Globalization;
using System.Text.RegularExpressions;

namespace Community.Archives.Rpm;

/// <summary>
/// /
/// </summary>
public static class RpmVersion
{
    private static Regex FindNonAlphaNumericPrefix = new Regex(
        @"^([^a-zA-Z0-9~\^]*)(.*)$",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    private static Regex IsNumeric = new Regex(
        @"^([\d]+)(.*)$",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    private static Regex IsAlpha = new Regex(
        @"^([a-zA-Z]+)(.*)$",
        RegexOptions.None,
        TimeSpan.FromSeconds(1)
    );

    /// <summary>
    /// Compares two rpm version string.
    /// </summary>
    /// <returns>
    /// <c>0</c> when both are identical,
    /// <c>-1</c> if <paramref name="left"/> is lower than <paramref name="right"/> or
    /// <c>+1</c> if <paramref name="left"/> is greater than <paramref name="right"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "MA0051:Method is too long",
        Justification = "Ported from python"
    )]
    public static int Compare(string left, string right)
    {
        bool isnum;
        while (!string.IsNullOrEmpty(left) || !string.IsNullOrEmpty(right))
        {
            var m1 = FindNonAlphaNumericPrefix.Match(left);
            var m2 = FindNonAlphaNumericPrefix.Match(right);
            var m1Head = m1.Groups[1].Value;
            left = m1.Groups[2].Value; // tail
            var m2Head = m2.Groups[1].Value;
            right = m2.Groups[2].Value; // tail
            if (!string.IsNullOrEmpty(m1Head) || !string.IsNullOrEmpty(m2Head))
            {
                // Ignore junk at the beginning
                continue;
            }

            // handle the tilde separator, it sorts before everything else
            if (left.StartsWith('~'))
            {
                if (!right.StartsWith('~'))
                {
                    return -1;
                }

                left = left.Substring(1);
                right = right.Substring(1);
                continue;
            }

            if (right.StartsWith('~'))
            {
                return 1;
            }

            // Now look at the caret, which is like the tilde but pointier.
            if (left.StartsWith('^'))
            {
                // first has a caret but second has ended
                if (string.IsNullOrEmpty(right))
                {
                    return 1; // first > second
                }

                // first has a caret but second continues on
                if (!right.StartsWith('^'))
                {
                    return -1; // first < second
                }

                // strip the ^ and start again
                left = left.Substring(1);
                right = right.Substring(1);
                continue;
            }

            // Caret means the version is less... Unless the other version
            // has ended, then do the exact opposite.
            if (right.StartsWith('^'))
            {
                if (string.IsNullOrEmpty(left))
                {
                    return -1;
                }

                return 1;
            }

            // We've run out of characters to compare.
            // Note: we have to do this after we compare the ~ and ^ madness
            // because ~'s and ^'s take precedance.
            // If we ran to the end of either, we are finished with the loop
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                break;
            }

            // grab first completely alpha or completely numeric segment
            m1 = IsNumeric.Match(left);
            if (m1.Success)
            {
                m2 = IsNumeric.Match(right);
                if (!m2.Success)
                {
                    // numeric segments are always newer than alpha segments
                    return 1;
                }

                isnum = true;
            }
            else
            {
                m1 = IsAlpha.Match(left);
                m2 = IsAlpha.Match(right);
                if (!m2.Success)
                {
                    return -1;
                }

                isnum = false;
            }

            m1Head = m1.Groups[1].Value;
            left = m1.Groups[2].Value; // tail
            m2Head = m2.Groups[1].Value;
            right = m2.Groups[2].Value; // tail
            if (isnum)
            {
                var m1Num = Int32.Parse(m1Head, CultureInfo.InvariantCulture);
                var m2Num = Int32.Parse(m2Head, CultureInfo.InvariantCulture);

                var cmp = m1Num.CompareTo(m2Num);

                if (cmp < 0)
                {
                    return -1;
                }

                if (cmp > 0)
                {
                    return 1;
                }
            }
            else
            {
                var headCmp = String.Compare(m1Head, m2Head, StringComparison.Ordinal);

                if (headCmp < 0)
                {
                    return -1;
                }

                if (headCmp > 0)
                {
                    return 1;
                }
            }
            // Both segments equal
            // continue with next segment
        }

        if (left.Length == 0 && right.Length == 0)
        {
            return 0;
        }

        if (left.Length != 0)
        {
            return 1;
        }

        return -1;
    }
}
