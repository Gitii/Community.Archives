using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Core.Tests
{
    public class EndiannessExtensionsTests
    {
        public const short TestValueShortBigEndian = 0x0F00;
        public const short TestValueShortLittleEndian = 0xF;
        public const int TestValueIntegerBigEndian = 0xF0F0000;
        public const int TestValueIntegerLittleEndian = 0x0F0F;
        public const long TestValueLongLittleEndian = 0x0F0F0F000;
        public const long TestValueLongBigEndian = 0xF0F0F000000000;

        [Endianness(ByteOrder.BigEndian)]
        struct AttributeOnStruct
        {
            public short Test1;
            public int Test2;
            public long Test3;

            public static AttributeOnStruct AsBigEndian()
            {
                return new AttributeOnStruct()
                {
                    Test1 = TestValueShortBigEndian,
                    Test2 = TestValueIntegerBigEndian,
                    Test3 = TestValueLongBigEndian,
                };
            }

            public static AttributeOnStruct AsLittleEndian()
            {
                return new AttributeOnStruct()
                {
                    Test1 = TestValueShortLittleEndian,
                    Test2 = TestValueIntegerLittleEndian,
                    Test3 = TestValueLongLittleEndian,
                };
            }
        }

        struct AttributeOnFields
        {
            [Endianness(ByteOrder.BigEndian)] public short Test1;
            [Endianness(ByteOrder.BigEndian)] public int Test2;
            [Endianness(ByteOrder.BigEndian)] public long Test3;
            [Endianness(ByteOrder.LittleEndian)] public short Test4;

            public static AttributeOnFields AsMixedInput()
            {
                return new AttributeOnFields()
                {
                    Test1 = TestValueShortBigEndian,
                    Test2 = TestValueIntegerBigEndian,
                    Test3 = TestValueLongBigEndian,
                    Test4 = TestValueShortLittleEndian
                };
            }

            public static AttributeOnFields AsMixedOutput()
            {
                return new AttributeOnFields()
                {
                    Test1 = TestValueShortLittleEndian,
                    Test2 = TestValueIntegerLittleEndian,
                    Test3 = TestValueLongBigEndian, // that's not an error, long types aren't supported
                    Test4 = TestValueShortLittleEndian
                };
            }
        }

        [Test]
        public void Test_OnClassAttribute()
        {
            var actual = AttributeOnStruct.AsBigEndian();
            actual.ConvertByteOrder();

            var expected = AttributeOnStruct.AsLittleEndian();

            actual.Test1.Should().Be(expected.Test1);
            actual.Test2.Should().Be(expected.Test2);

            // long types aren't supported, so it's unchanged
            actual.Test3.Should().Be(AttributeOnStruct.AsBigEndian().Test3);
        }

        [Test]
        public void Test_OnAttributeOnFields()
        {
            var actual = AttributeOnFields.AsMixedInput();
            actual.ConvertByteOrder();

            var expected = AttributeOnFields.AsMixedOutput();

            actual.Test1.Should().Be(expected.Test1);
            actual.Test2.Should().Be(expected.Test2);
            actual.Test3.Should().Be(expected.Test3);
            actual.Test4.Should().Be(expected.Test4);
        }
    }
}
