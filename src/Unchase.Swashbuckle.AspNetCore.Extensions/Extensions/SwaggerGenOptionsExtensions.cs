﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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
        #region Fields

        private static readonly FieldInfo _xmlDocMembers =
            typeof(XmlCommentsSchemaFilter).GetField("_xmlDocMembers", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

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
        /// <param name="configureOptions">An <see cref="Action{FixEnumsOptions}"/> to configure options for filters.</param>
        public static SwaggerGenOptions AddEnumsWithValuesFixFilters(
            this SwaggerGenOptions swaggerGenOptions,
            Action<FixEnumsOptions> configureOptions = null)
        {
            // local function
            void EmptyAction(FixEnumsOptions x) { }

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
        /// <param name="excludedTypes">Types for which remarks will be excluded.</param>
        public static SwaggerGenOptions IncludeXmlCommentsWithRemarks(
            this SwaggerGenOptions swaggerGenOptions,
            Func<XPathDocument> xmlDocFactory,
            bool includeControllerXmlComments = false,
            params Type[] excludedTypes)
        {
            swaggerGenOptions.IncludeXmlComments(xmlDocFactory, includeControllerXmlComments);

            var distinctExcludedTypes = excludedTypes?.Distinct().ToArray() ?? new Type[] { };

            var xmlDoc = xmlDocFactory();
            swaggerGenOptions.ParameterFilter<XmlCommentsWithRemarksParameterFilter>(xmlDoc, distinctExcludedTypes);
            swaggerGenOptions.RequestBodyFilter<XmlCommentsWithRemarksRequestBodyFilter>(xmlDoc, distinctExcludedTypes);
            swaggerGenOptions.SchemaFilter<XmlCommentsWithRemarksSchemaFilter>(xmlDoc, distinctExcludedTypes);

            if (includeControllerXmlComments)
            {
                swaggerGenOptions.DocumentFilter<XmlCommentsWithRemarksDocumentFilter>(xmlDoc, distinctExcludedTypes);
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
            return swaggerGenOptions.IncludeXmlCommentsWithRemarks(() => new XPathDocument(filePath), includeControllerXmlComments, excludedTypes);
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
            return swaggerGenOptions.IncludeXmlCommentsWithRemarks(xmlDocFactory, includeControllerXmlComments, excludedTypesFunc?.Invoke());
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
            return swaggerGenOptions.IncludeXmlCommentsWithRemarks(() => new XPathDocument(filePath), includeControllerXmlComments, excludedTypesFunc?.Invoke());
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
            if (_xmlDocMembers is null)
            {
                throw new NotSupportedException("Unsupported version of " + typeof(XmlCommentsSchemaFilter).Assembly.GetName().Name);
            }

            var inheritedElements = new List<(string Name, string Cref, string Path)>();
            var documentUris = new HashSet<string>();
            foreach (var descriptor in swaggerGenOptions.SchemaFilterDescriptors)
            {
                if (descriptor.Type == typeof(XmlCommentsSchemaFilter))
                {
                    var navigators = _xmlDocMembers.GetValue(descriptor.FilterInstance) as IEnumerable<KeyValuePair<string, XPathNavigator>>;
                    if (navigators is null)
                    {
                        continue;
                    }

                    foreach (var pair in navigators)
                    {
                        var navigator = pair.Value;
                        var baseUri = navigator.BaseURI;
                        if (!string.IsNullOrEmpty(baseUri))
                        {
                            // baseUri is empty string when creating XPathDocument via something like:
                            // new XPathDocument(new StringReader(File.ReadAllText(text)))
                            documentUris.Add(baseUri);
                        }

                        foreach (XPathNavigator member in navigator.Select("inheritdoc"))
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
                    }
                }
            }

            var documents = documentUris
                .Select(x => new XPathDocument(x))
                .ToList();

            var inheritedDocs = inheritedElements
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => (x.First().Cref, x.First().Path));

            var distinctExcludedTypes = excludedTypes?.Distinct().ToArray() ?? new Type[] { };
            swaggerGenOptions.ParameterFilter<InheritDocParameterFilter>(documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            swaggerGenOptions.RequestBodyFilter<InheritDocRequestBodyFilter>(documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            swaggerGenOptions.SchemaFilter<InheritDocSchemaFilter>(documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            swaggerGenOptions.OperationFilter<InheritDocOperationFilter>(documents, inheritedDocs, includeRemarks, distinctExcludedTypes);
            return swaggerGenOptions;
        }

        #endregion
    }
}