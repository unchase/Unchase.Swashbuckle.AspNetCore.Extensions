using System;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    /// <summary>
    /// Extension methods for <see cref=" object"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Check if an object is a number
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True or False</returns>
        internal static bool IsNumber(this Type type)
        {
            return type == typeof(sbyte)
                   || type == typeof(byte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   //|| type == typeof(Int128 - Only in .NET 7, 8, 9)
                   //|| type == typeof(UInt128  - Only in .NET 7, 8, 9)
                   //|| type == typeof(nint - Only in .NET 7, 8, 9)
                   //|| type == typeof(nuint - Only in .NET 7, 8, 9)
                   //|| type == typeof(Half - Only in .NET 5, 6, 7, 8, 9)
                   || type == typeof(float)
                   || type == typeof(double)
                   || type == typeof(decimal);
        }
    }
}