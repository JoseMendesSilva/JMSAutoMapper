using System;
using System.Collections.Concurrent;

namespace JMSAutoMapper.Conversion
{
    internal static class NumericConversionTable
    {
        public static readonly ConcurrentDictionary<(Type Source, Type Target), Func<object, object>> Table = new();

        static NumericConversionTable()
        {
            Table.TryAdd((typeof(double), typeof(decimal)), v => System.Convert.ToDecimal((double)v));
            Table.TryAdd((typeof(decimal), typeof(double)), v => System.Convert.ToDouble((decimal)v));
            Table.TryAdd((typeof(float), typeof(decimal)), v => System.Convert.ToDecimal((float)v));
            Table.TryAdd((typeof(decimal), typeof(float)), v => System.Convert.ToSingle((decimal)v));
            Table.TryAdd((typeof(double), typeof(float)), v => System.Convert.ToSingle((double)v));
            Table.TryAdd((typeof(float), typeof(double)), v => System.Convert.ToDouble((float)v));

            Type dec = typeof(decimal);
            Table.TryAdd((typeof(int), dec), v => System.Convert.ToDecimal((int)v));
            Table.TryAdd((typeof(long), dec), v => System.Convert.ToDecimal((long)v));
            Table.TryAdd((typeof(short), dec), v => System.Convert.ToDecimal((short)v));
            Table.TryAdd((typeof(byte), dec), v => System.Convert.ToDecimal((byte)v));

            Table.TryAdd((dec, typeof(int)), v => System.Convert.ToInt32((decimal)v));
            Table.TryAdd((dec, typeof(long)), v => System.Convert.ToInt64((decimal)v));
            Table.TryAdd((dec, typeof(short)), v => System.Convert.ToInt16((decimal)v));
            Table.TryAdd((dec, typeof(byte)), v => System.Convert.ToByte((decimal)v));
        }

        public static object? Convert(object value, Type targetType)
        {
            if (Table.TryGetValue((value.GetType(), targetType), out var converter))
                return converter(value);
            return null;
        }
    }
}