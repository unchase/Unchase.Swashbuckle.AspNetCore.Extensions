// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

using System.Collections.Generic;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Tests.Models
{
    /// <summary>
    /// InheritDocClass - inheritdoc
    /// </summary>
    /// <remarks>
    /// InheritDocClass remarks - inheritdoc
    /// </remarks>
    public interface IInheritDocClass : IInheritDocCommon
    {
        /// <summary>
        /// Name - inheritdoc
        /// </summary>
        /// <remarks>
        /// Name remarks - inheritdoc
        /// </remarks>
        /// <example>Donald Duck</example>
        string Name { get; set; }

        /// <summary>
        /// Age - inheritdoc
        /// </summary>
        /// <remarks>
        /// Age remarks - inheritdoc
        /// </remarks>
        /// <example>75</example>
        int? Age { get; set; }

        /// <summary>
        /// Weight - inheritdoc
        /// </summary>
        /// <remarks>
        /// Weight remarks - inheritdoc
        /// </remarks>
        /// <example>0.174</example>
        float? Weight { get; set; }

        /// <summary>
        /// NumberOfFeet - inheritdoc
        /// </summary>
        /// <remarks>
        /// NumberOfFeet remarks - inheritdoc
        /// </remarks>
        /// <example>2</example>
        int? NumberOfFeet { get; set; }

        /// <summary>
        /// AByte - inheritdoc
        /// </summary>
        /// <remarks>
        /// AByte remarks - inheritdoc
        /// </remarks>
        /// <example>1</example>
        byte? AByte { get; set; }

        /// <summary>
        /// AShort - inheritdoc
        /// </summary>
        /// <remarks>
        /// AShort remarks - inheritdoc
        /// </remarks>
        /// <example>1234</example>
        short? AShort { get; set; }

        /// <summary>
        /// ALong - inheritdoc
        /// </summary>
        /// <remarks>
        /// ALong remarks - inheritdoc
        /// </remarks>
        /// <example>1234</example>
        long? ALong { get; set; }

        /// <summary>
        /// AnArray - inheritdoc
        /// </summary>
        /// <remarks>
        /// AnArray remarks - inheritdoc
        /// </remarks>
        /// <example>[ "One", "Two", "Three" ]</example>
        string[] AnArray { get; set; }

        /// <summary>
        /// SomeFreeTexts - inheritdoc
        /// </summary>
        /// <remarks>
        /// SomeFreeTexts remarks - inheritdoc
        /// </remarks>
        /// <example>[{"text": "KAROSSERI OFÖRÄNDRAT, FLAK TÄCK MED KÅPA SOM HAR ALLA LUCKOR FASTLÅSTA.B"}, {"text": "AKLÄM FASTLÅST. INGEN MÖJLIGHET TILL LAST FINNS.BAKDÖRRAR FASTSVETSADE"}]</example>
        IEnumerable<FreeText> SomeFreeTexts { get; set; }

    }
}
