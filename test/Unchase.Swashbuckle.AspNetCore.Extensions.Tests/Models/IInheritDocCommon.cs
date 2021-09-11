namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// IInheritDocCommon interface
    /// </summary>
    /// <remarks>
    /// IInheritDocCommon interface remarks
    /// </remarks>
    public interface IInheritDocCommon
    {
        /// <summary>
        /// Common - inheritdoc (inner)
        /// </summary>
        /// <remarks>
        /// Common remarks - inheritdoc (inner)
        /// </remarks>
        string Common { get; set; }

        /// <summary>
        /// InheritEnum - inheritdoc (inner)
        /// </summary>
        InheritEnum InheritEnum { get; set; }
    }
}
