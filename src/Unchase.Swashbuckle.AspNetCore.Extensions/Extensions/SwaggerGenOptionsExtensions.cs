using System;
using System.Net;
using System.Xml.XPath;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Filters;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="SwaggerGenOptions"/>.
    /// </summary>
    public static class SwaggerGenOptionsExtensions
    {
        #region Extension methods

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
        /// <param name="services"><see cref="IServiceCollection"/>.</param>
        /// <param name="configureOptions">An <see cref="Action{FixEnumsOptions}"/> to configure options for filters.</param>
        /// <returns></returns>
        public static SwaggerGenOptions AddEnumsWithValuesFixFilters(
            this SwaggerGenOptions swaggerGenOptions,
            IServiceCollection services = null,
            Action<FixEnumsOptions> configureOptions = null)
        {
            // local function
            void EmptyAction(FixEnumsOptions x) { }

            if (configureOptions != null)
            {
                services?.Configure(configureOptions);
            }

            swaggerGenOptions.SchemaFilter<XEnumNamesSchemaFilter>(configureOptions ?? EmptyAction);
            swaggerGenOptions.ParameterFilter<XEnumNamesParameterFilter>(configureOptions ?? EmptyAction);
            swaggerGenOptions.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>();
            return swaggerGenOptions;
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files (from summary and remarks).
        /// </summary>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="xmlDocFactory">A factory method that returns XML Comments as an XPathDocument.</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false)
        {
            swaggerGenOptions.IncludeXmlComments(xmlDocFactory, includeControllerXmlComments);

            var xmlDoc = xmlDocFactory();
            swaggerGenOptions.ParameterFilter<XmlCommentsWithRemarksParameterFilter>(xmlDoc);
            swaggerGenOptions.RequestBodyFilter<XmlCommentsWithRemarksRequestBodyFilter>(xmlDoc);
            swaggerGenOptions.SchemaFilter<XmlCommentsWithRemarksSchemaFilter>(xmlDoc);

            if (includeControllerXmlComments)
                swaggerGenOptions.DocumentFilter<XmlCommentsWithRemarksDocumentFilter>(xmlDoc);

            return swaggerGenOptions;
        }

        /// <summary>
        /// Inject human-friendly descriptions for Operations, Parameters and Schemas based on XML Comment files (from summary and remarks).
        /// </summary>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="filePath">An absolute path to the file that contains XML Comments.</param>
        /// <param name="includeControllerXmlComments">
        /// Flag to indicate if controller XML comments (i.e. summary) should be used to assign Tag descriptions.
        /// Don't set this flag if you're customizing the default tag for operations via TagActionsBy.
        /// </param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            string filePath,
            bool includeControllerXmlComments = false)
        {
            return swaggerGenOptions.IncludeXmlCommentsWithRemarks(() => new XPathDocument(filePath), includeControllerXmlComments);
        }

        #endregion
    }
}