using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class FixedString12Tests
{
    private static long FixedStringDataLong = 123456789012;
    private static string FixedStringDataString = FixedStringDataLong.ToString();
    private static readonly byte[] FixedStringData = Encoding.ASCII.GetBytes(FixedStringDataString);

    private static FixedString12 CreateFixedString(string? strValue = null)
    {
        strValue ??= FixedStringDataString;

        strValue.Should().HaveLength(12);

        return new FixedString12
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
            Data10 = (byte)strValue[9],
            Data11 = (byte)strValue[10],
            Data12 = (byte)strValue[11]
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
    [TestCase("000000000001", 1)]
    [TestCase("100000000000", 100000000000)]
    [TestCase("123456789012", 123456789012)]
    public void Test_DecodeStringAsLong_decimal(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(false).Should().Be(lngValue);
    }

    [Test]
    [TestCase("000000000001", 1)]
    [TestCase("00174876E800", 100000000000)]
    [TestCase("001CBE991A14", 123456789012)]
    public void Test_DecodeStringAsLong_hex(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(true).Should().Be(lngValue);
    }

    [Test]
    [TestCase("000000000001", 1)]
    [TestCase("112402762000", 10000000000)]
    [TestCase("133767016065", 12345678901)]
    public void Test_DecodeStringAsLong_octal(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsOctalLong().Should().Be(lngValue);
    }
}
