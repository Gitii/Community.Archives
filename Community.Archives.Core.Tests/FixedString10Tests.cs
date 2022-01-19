using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class FixedString10Tests
{
    private static long FixedStringDataLong = 1234567890;
    private static string FixedStringDataString = FixedStringDataLong.ToString();
    private static readonly byte[] FixedStringData = Encoding.ASCII.GetBytes(FixedStringDataString);

    private static FixedString10 CreateFixedString(string? strValue = null)
    {
        strValue ??= FixedStringDataString;

        strValue.Should().HaveLength(10);

        return new FixedString10
        {
            Data1 = (byte)strValue[0],
            Data2 = (byte)strValue[1],
            Data3 = (byte)strValue[2],
            Data4 = (byte)strValue[3],
            Data5 = (byte)strValue[4],
            Data6 = (byte)strValue[5],
            Data7 = (byte)strValue[6],
            Data8 = (byte)strValue[7],
            Data9 = (byte)strValue[8],
            Data10 = (byte)strValue[9]
        };
    }

    [Test]
    public void Test_AsByteArray()
    {
        var fs = CreateFixedString();

        fs.AsByteArray().Should().Equal(FixedStringData);
    }

    [Test]
    public void Test_AsString()
    {
        var fs = CreateFixedString();

        fs.ToString().Should().Be(FixedStringDataString);
    }

    [Test]
    [TestCase("0000000001", 1)]
    [TestCase("1000000000", 1000000000)]
    [TestCase("1234567890", 1234567890)]
    public void Test_DecodeStringAsLong_decimal(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(false).Should().Be(lngValue);
    }

    [Test]
    [TestCase("0000000001", 1)]
    [TestCase("003B9ACA00", 1000000000)]
    [TestCase("00499602D2", 1234567890)]
    public void Test_DecodeStringAsLong_hex(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(true).Should().Be(lngValue);
    }
}
