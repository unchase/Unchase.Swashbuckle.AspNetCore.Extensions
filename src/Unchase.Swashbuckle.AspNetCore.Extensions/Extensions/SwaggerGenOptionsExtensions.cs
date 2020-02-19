using System.ComponentModel;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    public static class SwaggerGenOptionsExtensions
    {
        #region Extensions

        /// <summary>
        /// Change all responses by specific http status codes in OpenApi document.
        /// </summary>
        /// <typeparam name="T">Type of response example.</typeparam>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="httpStatusCode">HTTP status code.</param>
        /// <param name="responseDescription">Response description.</param>
        /// <param name="responseExampleOption"><see cref="ResponseExampleOptions"/>.</param>
        /// <param name="responseExample">New example for response.</param>
        /// <returns>
        /// Returns <see cref="SwaggerGenOptions"/>.
        /// </returns>
        public static SwaggerGenOptions ChangeAllResponsesByHttpStatusCode<T>(
            this SwaggerGenOptions swaggerGenOptions, 
            int httpStatusCode,
            string responseDescription = null,
            ResponseExampleOptions responseExampleOption = ResponseExampleOptions.None,
            T responseExample = default) where T : class
        {
            swaggerGenOptions.DocumentFilter<ChangeResponseByHttpStatusCodeDocumentFilter<T>>(httpStatusCode, responseDescription, responseExampleOption, responseExample);
            return swaggerGenOptions;
        }

        /// <summary>
        /// Change all responses by specific http status codes in OpenApi document.
        /// </summary>
        /// <typeparam name="T">Type of response example.</typeparam>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="httpStatusCode">HTTP status code.</param>
        /// <param name="responseDescription">Response description.</param>
        /// <param name="responseExampleOption"><see cref="ResponseExampleOptions"/>.</param>
        /// <param name="responseExample">New example for response.</param>
        /// <returns>
        /// Returns <see cref="SwaggerGenOptions"/>.
        /// </returns>
        public static SwaggerGenOptions ChangeAllResponsesByHttpStatusCode<T>(
            this SwaggerGenOptions swaggerGenOptions,
            HttpStatusCode httpStatusCode,
            string responseDescription = null,
            ResponseExampleOptions responseExampleOption = ResponseExampleOptions.None,
            T responseExample = default) where T : class
        {
            return swaggerGenOptions.ChangeAllResponsesByHttpStatusCode((int)httpStatusCode, responseDescription, responseExampleOption, responseExample);
        }

        /// <summary>
        /// Add filters to fix enums in OpenApi document.
        /// </summary>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="includeDescriptionFromAttribute">If true - add extensions (descriptions) from <see cref="DescriptionAttribute"/>.</param>
        /// <returns>
        /// Returns <see cref="SwaggerGenOptions"/>.
        /// </returns>
        public static SwaggerGenOptions AddEnumsWithValuesFixFilters(this SwaggerGenOptions swaggerGenOptions, bool includeDescriptionFromAttribute = false)
        {
            swaggerGenOptions.SchemaFilter<XEnumNamesSchemaFilter>(includeDescriptionFromAttribute);
            swaggerGenOptions.ParameterFilter<XEnumNamesParameterFilter>(includeDescriptionFromAttribute);
            swaggerGenOptions.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(includeDescriptionFromAttribute);
            return swaggerGenOptions;
        }

        #endregion
    }
}
