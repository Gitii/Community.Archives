using System.Buffers.Binary;
using System.ComponentModel;
using System.Reflection;

namespace Community.Archives.Core;

public static class EndiannessExtensions
{
    /// <summary>
    /// Converts the byte order of the fields in the given struct so that it matches the host endianness.
    /// If <seealso cref="Endianness"/> is set on a struct, that byte order will be assumed for all fields.
    /// </summary>
    /// <typeparam name="T">Type of the struct.</typeparam>
    /// <param name="obj">The struct of type <typeparamref name="T"/>. It's passed in by-ref.</param>
    /// <param name="overrideHostSystemEndianness">Overrides the byte order of the host system. Mainly used for testing.</param>
    /// <returns>The same struct that has been passed in (by-ref). The byte order has been reversed if necessary.</returns>
    public static ref T ConvertByteOrder<T>(
        ref this T obj,
        ByteOrder? overrideHostSystemEndianness = null
    ) where T : struct
    {
        var hostSystemEndianness = overrideHostSystemEndianness.GetValueOrDefault(
            GetHostSystemEndiness()
        );
        var classAttr = typeof(T).GetCustomAttribute<Endianness>();
        if (classAttr != null)
        {
            ConvertByteOrderOnFieldSeparately(ref obj, classAttr, hostSystemEndianness);
        }
        else
        {
            ConvertByteOrderOnClass(ref obj, hostSystemEndianness);
        }

        return ref obj;
    }

    private static ByteOrder GetHostSystemEndiness()
    {
        return BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
    }

    private static void ConvertByteOrderOnClass<T>(ref T obj, ByteOrder hostSystemEndianness) where T : struct
    {
        foreach (var fieldInfo in typeof(T).GetFields())
        {
            var endianness = fieldInfo.GetCustomAttribute<Endianness>();

            if (endianness != null)
            {
                if (endianness.ByteOrder != hostSystemEndianness)
                {
                    if (fieldInfo.FieldType == typeof(short))
                    {
                        var typedRef = __makeref(obj);
                        short number = (short)fieldInfo.GetValueDirect(typedRef)!;
                        fieldInfo.SetValueDirect(
                            typedRef,
                            BinaryPrimitives.ReverseEndianness(number)
                        );
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        var typedRef = __makeref(obj);
                        int number = (int)fieldInfo.GetValueDirect(typedRef)!;
                        fieldInfo.SetValueDirect(
                            typedRef,
                            BinaryPrimitives.ReverseEndianness(number)
                        );
                    }
                }
            }
        }
    }

    private static void ConvertByteOrderOnFieldSeparately<T>(
        ref T obj,
        Endianness classAttr,
        ByteOrder hostSystemEndianness
    ) where T : struct
    {
        foreach (var fieldInfo in typeof(T).GetFields())
        {
            if (classAttr.ByteOrder != hostSystemEndianness)
            {
                if (fieldInfo.FieldType == typeof(short))
                {
                    var typedRef = __makeref(obj);
                    short number = (short)fieldInfo.GetValueDirect(typedRef)!;
                    fieldInfo.SetValueDirect(typedRef, BinaryPrimitives.ReverseEndianness(number));
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    var typedRef = __makeref(obj);
                    int number = (int)fieldInfo.GetValueDirect(typedRef)!;
                    fieldInfo.SetValueDirect(typedRef, BinaryPrimitives.ReverseEndianness(number));
                }
            }
        }
    }
}
