using System;
using System.Collections.Generic;
using System.ComponentModel;

using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Options
{
    /// <summary>
    /// Various configuration properties for fixing enums.
    /// </summary>
    public class FixEnumsOptions
    {
        #region Properties

        /// <summary>
        /// Included file paths with XML comments.
        /// </summary>
        public HashSet<string> IncludedXmlCommentsPaths { get; } = new HashSet<string>();

        /// <summary>
        /// Include descriptions from <see cref="DescriptionAttribute"/> or xml comments. Default value is false.
        /// </summary>
        public bool IncludeDescriptions { get; set; } = false;

        /// <summary>
        /// Include remarks for descriptions from xml comments. Default value is false.
        /// </summary>
        public bool IncludeXEnumRemarks { get; set; } = false;

        /// <summary>
        /// Source to get descriptions. Default value is <see cref="DescriptionSources.DescriptionAttributes"/>.
        /// </summary>
        public DescriptionSources DescriptionSource { get; set; } = DescriptionSources.DescriptionAttributes;

        /// <summary>
        /// Apply fix enum filter to OpenApi schema. Default value is true.
        /// <para>
        /// Equivalent to "options.SchemaFilter&lt;XEnumNamesSchemaFilter&gt;();"
        /// </para>
        /// </summary>
        public bool ApplySchemaFilter { get; set; } = true;

        /// <summary>
        /// Apply fix enum filter to OpenApi parameters. Default value is true.
        /// <para>
        /// Equivalent to "options.ParameterFilter&lt;XEnumNamesParameterFilter&gt;();"
        /// </para>
        /// </summary>
        public bool ApplyParameterFilter { get; set; } = true;

        /// <summary>
        /// Apply fix enum filter to OpenApi document. Default value is true.
        /// <para>
        /// Equivalent to "options.DocumentFilter&lt;DisplayEnumsWithValuesDocumentFilter&gt;();"
        /// </para>
        /// </summary>
        public bool ApplyDocumentFilter { get; set; } = true;

        /// <summary>
        /// Alias for replacing "x-enumNames" in swagger documentation.
        /// </summary>
        public string XEnumNamesAlias { get; set; } = "x-enumNames";

        /// <summary>
        /// Alias for replacing "x-enumDescriptions" in swagger documentation.
        /// </summary>
        public string XEnumDescriptionsAlias { get; set; } = "x-enumDescriptions";

        /// <summary>
        /// New line for enum values descriptions.
        /// </summary>
        /// <remarks>
        /// For example, <see cref="Environment.NewLine"/> or "\n".
        /// </remarks>
        public string NewLine { get; set; } = "\n";

        #endregion
    }
}
