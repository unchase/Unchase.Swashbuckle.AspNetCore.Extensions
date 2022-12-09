using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Adds documentation to requests body that is provided by the &lt;inhertidoc /&gt; tag.
    /// </summary>
    /// <seealso cref="IRequestBodyFilter" />
    internal class InheritDocRequestBodyFilter :
        IRequestBodyFilter
    {
        #region Fields

        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";
        private readonly bool _includeRemarks;
        private readonly List<XPathDocument> _documents;
        private readonly Dictionary<string, string> _inheritedDocs;
        private readonly Type[] _excludedTypes;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocRequestBodyFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        public InheritDocRequestBodyFilter(
            List<XPathDocument> documents,
            Dictionary<string, string> inheritedDocs,
            bool includeRemarks = false)
            : this(documents, inheritedDocs, includeRemarks, Array.Empty<Type>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocRequestBodyFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        /// <param name="excludedTypes">Excluded types.</param>
        public InheritDocRequestBodyFilter(
            List<XPathDocument> documents,
            Dictionary<string, string> inheritedDocs,
            bool includeRemarks = false,
            params Type[] excludedTypes)
        {
            _includeRemarks = includeRemarks;
            _excludedTypes = excludedTypes;
            _documents = documents;
            _inheritedDocs = inheritedDocs;
        }

        #endregion

        #region Methods

        public void Apply(
            OpenApiRequestBody requestBody,
            RequestBodyFilterContext context)
        {
            ApplyForType(requestBody, context, context.BodyParameterDescription?.Type);

            if (context.FormParameterDescriptions?.Any() == true)
            {
                foreach (var formParameterDescription in context.FormParameterDescriptions)
                {
                    ApplyForType(requestBody, context, formParameterDescription?.Type);
                }
            }
        }

        private void ApplyForType(
            OpenApiRequestBody requestBody,
            RequestBodyFilterContext context, Type type)
        {
            if (type == null)
            {
                return;
            }

            if (_excludedTypes.Any() && _excludedTypes.ToList().Contains(type))
            {
                return;
            }

            // Try to apply a description for inherited types.
            string parameterMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            if (string.IsNullOrWhiteSpace(requestBody.Description) && _inheritedDocs.ContainsKey(parameterMemberName))
            {
                string cref = _inheritedDocs[parameterMemberName];
                XPathNavigator targetXmlNode;
                if (string.IsNullOrWhiteSpace(cref))
                {
                    var target = type.GetTargetRecursive(_inheritedDocs, cref);
                    if (target == null)
                    {
                        return;
                    }

                    targetXmlNode = XmlCommentsExtensions.GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForType(target), _documents);
                }
                else
                {
                    targetXmlNode = XmlCommentsExtensions.GetMemberXmlNode(cref, _documents);
                }

                var summaryNode = targetXmlNode?.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                {
                    requestBody.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                    if (_includeRemarks)
                    {
                        var remarksNode = targetXmlNode.SelectSingleNode(RemarksTag);
                        if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                        {
                            requestBody.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                        }
                    }
                }
            }

            if (context.SchemaRepository.Schemas.ContainsKey(type.Name))
            {
                var schema = context.SchemaRepository.Schemas[type.Name];
                if (schema?.Properties?.Any() != true)
                {
                    return;
                }

                // Add the summary and examples for the properties.
                foreach (var entry in schema.Properties)
                {
                    var members = type.GetMembers();
                    var memberInfo = members.FirstOrDefault(p =>
                        p.Name.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
                    if (memberInfo != null)
                    {
                        entry.Value.ApplyPropertyComments(memberInfo, _documents, _inheritedDocs, _includeRemarks, _excludedTypes);
                    }
                }
            }
        }

        #endregion
    }
}
