using System;
using System.Collections.Generic;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// Complicated class for testing enums
    /// </summary>
    /// <remarks>
    /// Complicated class remarks - class
    /// </remarks>
    public class ComplicatedClass
    {
        /// <summary>
        /// Tag
        /// </summary>
        /// <remarks>
        /// Tag remarks - property
        /// </remarks>
        public Tag Tag { get; set; }

        /// <summary>
        /// Some enum without xml-comments for one of values.
        /// </summary>
        public EnumWithoutXmlComments Enum { get; set; }

        /// <summary>
        /// Inner class
        /// </summary>
        /// <remarks>
        /// InnerClass remarks - property
        /// </remarks>
        public InnerClass InnerClass { get; set; }
    }

    /// <summary>
    /// Inner class
    /// </summary>
    /// <remarks>
    /// Inner class remarks - class
    /// </remarks>
    public class InnerClass
    {
        /// <summary>
        /// List of inner enums
        /// </summary>
        /// <remarks>
        /// List of inner enums remarks - property
        /// </remarks>
        public List<InnerEnum> InnerEnum { get; set; }

        /// <summary>
        /// Second inner class
        /// </summary>
        /// <remarks>
        /// Second inner class remarks - property
        /// </remarks>
        public SecondInnerClass<SecondInnerEnum> SecondInnerClass { get; set; }
    }

    /// <summary>
    /// Inner enum
    /// </summary>
    /// <remarks>
    /// Inner enum remarks - enum
    /// </remarks>
    public enum InnerEnum
    {
        /// <summary>
        /// Inner enum value
        /// </summary>
        /// <remarks>
        /// Inner enum value remarks
        /// </remarks>
        Value = 1
    }

    /// <summary>
    /// Second inner class
    /// </summary>
    /// <remarks>
    /// Second inner class remarks - class
    /// </remarks>
    public class SecondInnerClass<T> where T : Enum
    {
        /// <summary>
        /// Second inner enum
        /// </summary>
        /// <remarks>
        /// Second inner enum remarks - property
        /// </remarks>
        public T InnerEnum { get; set; }
    }

    /// <summary>
    /// Second inner enum
    /// </summary>
    /// <remarks>
    /// Second inner enum remarks - enum
    /// </remarks>
    public enum SecondInnerEnum
    {
        /// <summary>
        /// Second inner enum value
        /// </summary>
        /// <remarks>
        /// Second inner enum value remarks
        /// </remarks>
        Value = 0
    }
}
