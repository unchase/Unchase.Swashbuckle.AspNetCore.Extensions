using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.XPath;
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
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.DocumentFilter<ChangeResponseByHttpStatusCodeDocumentFilter<T>>(swaggerGenOptions, httpStatusCode, responseDescription, responseExampleOption, responseExample);
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
            return ChangeAllResponsesByHttpStatusCode(swaggerGenOptions, (int)httpStatusCode, responseDescription, responseExampleOption, responseExample);
        }

        /// <summary>
        /// Add filters to fix enums in OpenApi document.
        /// </summary>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="configureOptions">An <see cref="Action{FixEnumsOptions}"/> to configure options for filters.</param>
        public static SwaggerGenOptions AddEnumsWithValuesFixFilters(
            this SwaggerGenOptions swaggerGenOptions,
            Action<FixEnumsOptions> configureOptions = null)
        {
            // local function
            void EmptyAction(FixEnumsOptions x) { }

            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.SchemaFilter<XEnumNamesSchemaFilter>(swaggerGenOptions, configureOptions ?? EmptyAction);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.ParameterFilter<XEnumNamesParameterFilter>(swaggerGenOptions, configureOptions ?? EmptyAction);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.DocumentFilter<DisplayEnumsWithValuesDocumentFilter>(swaggerGenOptions);
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
        /// <param name="excludedTypes">Types for which remarks will be excluded.</param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false,
            params Type[] excludedTypes)
        {
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.IncludeXmlComments(swaggerGenOptions, xmlDocFactory, includeControllerXmlComments);

            var distinctExcludedTypes = excludedTypes?.Distinct().ToArray() ?? new Type[] { };

            var xmlDoc = xmlDocFactory();
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.ParameterFilter<XmlCommentsWithRemarksParameterFilter>(swaggerGenOptions, xmlDoc, distinctExcludedTypes);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.RequestBodyFilter<XmlCommentsWithRemarksRequestBodyFilter>(swaggerGenOptions, xmlDoc, distinctExcludedTypes);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.SchemaFilter<XmlCommentsWithRemarksSchemaFilter>(swaggerGenOptions, xmlDoc, distinctExcludedTypes);

            if (includeControllerXmlComments)
            {
                Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.DocumentFilter<XmlCommentsWithRemarksDocumentFilter>(swaggerGenOptions, xmlDoc, distinctExcludedTypes);
            }

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
        /// <param name="excludedTypes">Types for which remarks will be excluded.</param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            string filePath,
            bool includeControllerXmlComments = false,
            params Type[] excludedTypes)
        {
            return IncludeXmlCommentsWithRemarks(swaggerGenOptions, () => new XPathDocument(filePath), includeControllerXmlComments, excludedTypes);
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
        /// <param name="excludedTypesFunc">Func for excluding types.</param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false,
            Func<Type[]> excludedTypesFunc = default)
        {
            return IncludeXmlCommentsWithRemarks(swaggerGenOptions, xmlDocFactory, includeControllerXmlComments, excludedTypesFunc?.Invoke());
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
        /// <param name="excludedTypesFunc">Func for excluding types.</param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            string filePath,
            bool includeControllerXmlComments = false,
            Func<Type[]> excludedTypesFunc = default)
        {
            return IncludeXmlCommentsWithRemarks(swaggerGenOptions, () => new XPathDocument(filePath), includeControllerXmlComments, excludedTypesFunc?.Invoke());
        }

        /// <summary>
        /// Inject human-friendly descriptions for Schemas and it's Parameters based on &lt;inheritdoc/&gt; XML Comments (from summary and remarks).
        /// </summary>
        /// <param name="swaggerGenOptions"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="includeRemarks">
        /// Flag to indicate to include remarks from XML comments.
        /// </param>
        /// <param name="excludedTypes">Types for which remarks will be excluded.</param>
        public static SwaggerGenOptions IncludeXmlCommentsFromInheritDocs(
            this SwaggerGenOptions swaggerGenOptions,
            bool includeRemarks = false,
            params Type[] excludedTypes)
        {
            var documents = swaggerGenOptions.SchemaFilterDescriptors.Where(x => x.Type == typeof(XmlCommentsSchemaFilter)).Select(x => x.Arguments.Single()).Cast<XPathDocument>().ToList();

            var inheritedDocs = documents.SelectMany(doc =>
            {
                var inheritedElements = new List<(string Name, string Cref, string Path)>();
                foreach (XPathNavigator member in doc.CreateNavigator().Select("doc/members/member/inheritdoc"))
                {
                    string cref = member.GetAttribute("cref", string.Empty);
                    string path = member.GetAttribute("path", string.Empty);
                    member.MoveToParent();
                    string parentCref = member.GetAttribute("cref", string.Empty);
                    string parentPath = member.GetAttribute("path", string.Empty);
                    if (!string.IsNullOrWhiteSpace(parentCref))
                    {
                        cref = parentCref;
                        path = parentPath;
                    }

                    inheritedElements.Add((member.GetAttribute("name", string.Empty), cref, path));
                }

                return inheritedElements;
            }).GroupBy(x => x.Name).ToDictionary(x => x.Key, x => (x.First().Cref, x.First().Path));

            var distinctExcludedTypes = excludedTypes?.Distinct().ToArray() ?? new Type[] { };
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.ParameterFilter<InheritDocParameterFilter>(swaggerGenOptions, documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.RequestBodyFilter<InheritDocRequestBodyFilter>(swaggerGenOptions, documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.SchemaFilter<InheritDocSchemaFilter>(swaggerGenOptions, documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            Microsoft.Extensions.DependencyInjection.SwaggerGenOptionsExtensions.OperationFilter<InheritDocOperationFilter>(swaggerGenOptions, documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            return swaggerGenOptions;
        }

        #endregion
    }
}