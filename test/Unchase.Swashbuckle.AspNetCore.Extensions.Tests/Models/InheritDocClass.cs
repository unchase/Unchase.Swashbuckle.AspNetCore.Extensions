namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <inheritdoc cref="IInheritDocClass"/>
    public class InheritDocClass : IInheritDocClass
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc cref="IInheritDocCommon.Common"/>
        public string Common { get; set; }

        /// <inheritdoc/>
        public InheritEnum InheritEnum { get; set; }
    }
}
