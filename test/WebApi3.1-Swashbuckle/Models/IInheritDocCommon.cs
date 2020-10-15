namespace WebApi3._1_Swashbuckle.Models
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
        public string Common { get; set; }

        /// <summary>
        /// InheritEnum - inheritdoc (inner)
        /// </summary>
        public InheritEnum InheritEnum { get; set; }
    }
}
