using System;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// Class with enum array
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClassWithEnumArray<T>
    {
        /// <summary>
        /// Enum array.
        /// </summary>
        /// <remarks>
        /// Enum array remarks - property
        /// </remarks>
        public T[] EnumArray { get; set; }
    }
}
