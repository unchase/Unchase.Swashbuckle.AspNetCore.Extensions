using System.Collections.Generic;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <inheritdoc cref="IInheritDocClass"/>
    public class InheritDocClass : IInheritDocClass
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public int? Age { get; set; }

        /// <inheritdoc/>
        public float? Weight { get; set; }

        /// <inheritdoc/>
        public int? NumberOfFeet { get; set; }

        /// <inheritdoc/>
        public byte? AByte { get; set; }

        /// <inheritdoc/>
        public short? AShort { get; set; }

        /// <inheritdoc/>
        public long? ALong { get; set; }

        /// <inheritdoc/>
        public string[] AnArray { get; set; }

        /// <inheritdoc/>
        public IEnumerable<FreeText> SomeFreeTexts { get; set; }

        /// <inheritdoc cref="IInheritDocCommon.Common"/>
        public string Common { get; set; }

        /// <inheritdoc/>
        public InheritEnum InheritEnum { get; set; }
    }
}
