using System;
using System.Collections.Generic;

namespace WebApi3._1_Swashbuckle.Models
{
    /// <summary>
    /// Complicated class for testing enums.
    /// </summary>
    public class ComplicatedClass
    {
        /// <summary>
        /// Tag.
        /// </summary>
        public Tag Tag { get; set; }

        /// <summary>
        /// Inner class.
        /// </summary>
        public InnerClass InnerClass { get; set; }
    }

    /// <summary>
    /// Inner class.
    /// </summary>
    public class InnerClass
    {
        /// <summary>
        /// List of inner enums.
        /// </summary>
        public List<InnerEnum> InnerEnum { get; set; }

        /// <summary>
        /// Second inner class.
        /// </summary>
        public SecondInnerClass<SecondInnerEnum> SecondInnerClass { get; set; }
    }

    /// <summary>
    /// Inner enum.
    /// </summary>
    public enum InnerEnum
    {
        /// <summary>
        /// Inner enum value.
        /// </summary>
        Value = 1
    }

    /// <summary>
    /// Second inner class.
    /// </summary>
    public class SecondInnerClass<T> where T : Enum
    {
        /// <summary>
        /// Second inner enum.
        /// </summary>
        public T InnerEnum { get; set; }
    }

    /// <summary>
    /// Second inner enum.
    /// </summary>
    public enum SecondInnerEnum
    {
        /// <summary>
        /// Second inner enum value.
        /// </summary>
        Value = 0
    }
}
