﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Adds documentation to parameters that is provided by the &lt;inhertidoc /&gt; tag.
    /// </summary>
    /// <seealso cref="IParameterFilter" />
    internal class InheritDocParameterFilter :
        IParameterFilter
    {
        #region Fields

        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private readonly bool _includeRemarks;
        private readonly List<XPathDocument> _documents;
        private readonly Dictionary<string, (string Cref, string Path)> _inheritedDocs;
        private readonly Type[] _excludedTypes;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocParameterFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        public InheritDocParameterFilter(
            List<XPathDocument> documents,
            Dictionary<string, (string Cref, string Path)> inheritedDocs,
            bool includeRemarks = false)
            : this(documents, inheritedDocs, includeRemarks, Array.Empty<Type>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocParameterFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        /// <param name="excludedTypes">Excluded types.</param>
        public InheritDocParameterFilter(
            List<XPathDocument> documents,
            Dictionary<string, (string Cref, string Path)> inheritedDocs,
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

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="parameter"><see cref="OpenApiParameter"/>.</param>
        /// <param name="context"><see cref="ParameterFilterContext"/>.</param>
        public void Apply(
            OpenApiParameter parameter,
            ParameterFilterContext context)
        {
            if (context.ApiParameterDescription?.PropertyInfo() == null)
            {
                return;
            }

            if (_excludedTypes.Any() && _excludedTypes.ToList().Contains(context.ApiParameterDescription.PropertyInfo().DeclaringType))
            {
                return;
            }

            if (context.ApiParameterDescription.PropertyInfo() == null)
            {
                return;
            }

            // Try to apply a description for inherited types.
            string parameterMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(context.ApiParameterDescription.PropertyInfo());
            if (string.IsNullOrWhiteSpace(parameter.Description) && _inheritedDocs.ContainsKey(parameterMemberName))
            {
                string cref = _inheritedDocs[parameterMemberName].Cref;
                XPathNavigator targetXmlNode;
                if (string.IsNullOrWhiteSpace(cref))
                {
                    var target = context.ApiParameterDescription.PropertyInfo().GetTargetRecursive(_inheritedDocs, cref);
                    if (target == null)
                    {
                        return;
                    }

                    targetXmlNode = XmlCommentsExtensions.GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target), _documents);
                }
                else
                {
                    targetXmlNode = XmlCommentsExtensions.GetMemberXmlNode(cref, _documents);
                }

                var summaryNode = targetXmlNode?.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                {
                    parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                    if (_includeRemarks)
                    {
                        var remarksNode = targetXmlNode.SelectSingleNode(RemarksTag);
                        if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                        {
                            parameter.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                        }
                    }
                }
            }

            // TODO
            var type = context.ApiParameterDescription.PropertyInfo()?.DeclaringType;
            var typeName = type?.Name;
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                if (context.SchemaRepository.Schemas.ContainsKey(typeName))
                {
                    var schema = context.SchemaRepository.Schemas[typeName];
                    if (schema?.Properties?.Any() != true)
                    {
                        return;
                    }

                    // Add the summary and examples for the properties.
                    foreach (var entry in schema.Properties)
                    {
                        var members = ((TypeInfo)type).GetMembers();
                        var memberInfo = members.FirstOrDefault(p =>
                            p.Name.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
                        if (memberInfo != null)
                        {
                            entry.Value.ApplyPropertyComments(memberInfo, _documents, _inheritedDocs, _includeRemarks, _excludedTypes);
                        }
                    }
                }
            }
        }

        #endregion
    }
}