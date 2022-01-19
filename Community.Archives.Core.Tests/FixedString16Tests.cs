using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests;

public class FixedString16Tests
{
    private static long FixedStringDataLong = 12345678;
    private static string FixedStringDataString = "0000000" + FixedStringDataLong.ToString();

    private static FixedString16 CreateFixedString(string? strValue = null)
    {
        strValue ??= FixedStringDataString;

        strValue.Should().HaveLength(16);

        return new FixedString16
        {
            Data1 = new FixedString8
            {
                Data1 = (byte)strValue[0],
                Data2 = (byte)strValue[1],
                Data3 = (byte)strValue[2],
                Data4 = (byte)strValue[3],
                Data5 = (byte)strValue[4],
                Data6 = (byte)strValue[5],
                Data7 = (byte)strValue[6],
                Data8 = (byte)strValue[7]
            },
            Data2 = new FixedString8
            {
                Data1 = (byte)strValue[8],
                Data2 = (byte)strValue[9],
                Data3 = (byte)strValue[10],
                Data4 = (byte)strValue[11],
                Data5 = (byte)strValue[12],
                Data6 = (byte)strValue[13],
                Data7 = (byte)strValue[14],
                Data8 = (byte)strValue[15]
            }
        };
    }

    [Test]
    public void Test_AsByteArray()
    {
        const string strValue = "1234567812345678";
        var fs = CreateFixedString(strValue);

        fs.AsByteArray().Should().Equal(Encoding.ASCII.GetBytes(strValue));
    }

    [Test]
    public void Test_AsString()
    {
        const string strValue = "1234567812345678";
        var fs = CreateFixedString(strValue);

        fs.ToString().Should().Be(strValue);
    }

    [Test]
    [TestCase("0000000000000001", 1)]
    [TestCase("0000000010000000", 10000000)]
    [TestCase("0000000012345678", 12345678)]
    public void Test_DecodeStringAsLong_decimal(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(false).Should().Be(lngValue);
    }

    [Test]
    [TestCase("0000000000000001", 1)]
    [TestCase("0000000000989680", 10000000)]
    [TestCase("0000000000BC614E", 12345678)]
    public void Test_DecodeStringAsLong_hex(string strValue, long lngValue)
    {
        var fs = CreateFixedString(strValue);

        fs.DecodeStringAsLong(true).Should().Be(lngValue);
    }
}
