using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class FixedString8Tests
{
    private static long FixedStringDataLong = 12345678;
    private static string FixedStringDataString = FixedStringDataLong.ToString();
    private static readonly byte[] FixedStringData = Encoding.ASCII.GetBytes(FixedStringDataString);

    private static FixedString8 CreateFixedString(string? strValue = null)
    {
        strValue ??= FixedStringDataString;

        strValue.Should().HaveLength(8);

        return new FixedString8
        {
            Data1 = (byte)strValue[0],
            Data2 = (byte)strValue[1],
            Data3 = (byte)strValue[2],
            Data4 = (byte)strValue[3],
            Data5 = (byte)strValue[4],
            Data6 = (byte)strValue[5],
            Data7 = (byte)strValue[6],
            Data8 = (byte)strValue[7]
        };
    }

    [Test]
    public void Test_AsByteArray()
    {
        FixedString8 fs = CreateFixedString();

        fs.AsByteArray().Should().Equal(FixedStringData);
    }

    [Test]
    public void Test_AsString()
    {
        FixedString8 fs = CreateFixedString();

        fs.ToString().Should().Be(FixedStringDataString);
    }

    [Test]
    [TestCase("00000001", 1)]
    [TestCase("10000000", 10000000)]
    [TestCase("12345678", 12345678)]
    public void Test_DecodeStringAsLong_decimal(string strValue, long lngValue)
    {
        FixedString8 fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(false).Should().Be(lngValue);
    }

    [Test]
    [TestCase("00000001", 1)]
    [TestCase("00989680", 10000000)]
    [TestCase("00BC614E", 12345678)]
    public void Test_DecodeStringAsLong_hex(string strValue, long lngValue)
    {
        FixedString8 fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(true).Should().Be(lngValue);
    }
}
