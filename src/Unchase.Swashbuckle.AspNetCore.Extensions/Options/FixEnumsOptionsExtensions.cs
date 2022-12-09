namespace Unchase.Swashbuckle.AspNetCore.Extensions.Options
{
    /// <summary>
    /// Extensions for <see cref="FixEnumsOptions"/>.
    /// </summary>
    public static class FixEnumsOptionsExtensions
    {
        #region Extension methods

        /// <summary>
        /// Include XML comments from <paramref name="fullPath"/>.
        /// </summary>
        /// <param name="options"><see cref="FixEnumsOptions"/>.</param>
        /// <param name="fullPath">Full file path with XML comments.</param>
        /// <returns>
        /// Returns <see cref="FixEnumsOptions"/>.
        /// </returns>
        public static FixEnumsOptions IncludeXmlCommentsFrom(
            this FixEnumsOptions options,
            string fullPath)
        {
            if (!options.IncludedXmlCommentsPaths.Contains(fullPath))
            {
                options.IncludedXmlCommentsPaths.Add(fullPath);
            }

            return options;
        }

        #endregion
    }
}
