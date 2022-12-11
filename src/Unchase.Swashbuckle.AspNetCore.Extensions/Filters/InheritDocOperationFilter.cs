using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    internal class InheritDocOperationFilter :
        IOperationFilter
    {
        #region Fields

        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";
        private const string ParamTag = "param";
        private const string ParamXPath = "/doc/members/member/param[@name='{0}']";
        private readonly bool _includeRemarks;
        private readonly List<XPathDocument> _documents;
        private readonly Dictionary<string, (string Cref, string Path)> _inheritedDocs;
        private readonly Type[] _excludedTypes;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocOperationFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        public InheritDocOperationFilter(
            List<XPathDocument> documents,
            Dictionary<string, (string Cref, string Path)> inheritedDocs,
            bool includeRemarks = false)
            : this(documents, inheritedDocs, includeRemarks, Array.Empty<Type>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritDocOperationFilter" /> class.
        /// </summary>
        /// <param name="inheritedDocs">Dictionary with inheritdoc in form of name-cref.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="excludedTypes">Excluded types.</param>
        /// <param name="documents">List of <see cref="XPathDocument"/>.</param>
        public InheritDocOperationFilter(
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
        /// <param name="operation"><see cref="OpenApiOperation"/>.</param>
        /// <param name="context"><see cref="OperationFilterContext"/>.</param>
        public void Apply(
            OpenApiOperation operation,
            OperationFilterContext context)
        {
            string memberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(context.MethodInfo);
            if (_inheritedDocs.ContainsKey(memberName))
            {
                string cref = _inheritedDocs[memberName].Cref;
                XPathNavigator targetXmlNode;
                var originXmlNode = XmlCommentsExtensions.GetMemberXmlNode(memberName, _documents);
                if (string.IsNullOrWhiteSpace(cref))
                {
                    var target = context.MethodInfo.GetTargetRecursive(_inheritedDocs, cref);
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

                var pathes = _inheritedDocs[memberName].Path.Split('|');
                var summaryNode = targetXmlNode?.SelectSingleNode(SummaryTag);
                if (summaryNode != null && (!pathes.Any() || pathes.Contains(SummaryTag)))
                {
                    if (string.IsNullOrWhiteSpace(operation.Description))
                    {
                        operation.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                    }
                    else
                    {
                        operation.Description += $" ({XmlCommentsTextHelper.Humanize(summaryNode.InnerXml)})";
                    }
                }
                else
                {
                    summaryNode = originXmlNode?.SelectSingleNode(SummaryTag);
                    if (summaryNode != null)
                    {
                        operation.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                    }
                }

                var remarksNode = targetXmlNode?.SelectSingleNode(RemarksTag);
                if (_includeRemarks && remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml) && (!pathes.Any() || pathes.Contains(RemarksTag)))
                {
                    if (string.IsNullOrWhiteSpace(operation.Description))
                    {
                        operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
                    }
                    else
                    {
                        operation.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                    }
                }
                else if (_includeRemarks)
                {
                    remarksNode = originXmlNode?.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        if (string.IsNullOrWhiteSpace(operation.Description))
                        {
                            operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
                        }
                        else
                        {
                            operation.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                        }
                    }
                }

                // inherit from params
                if (pathes.Contains(ParamTag))
                {
                    foreach (var parameterInfo in context.MethodInfo.GetParameters())
                    {
                        var parameterType = parameterInfo.ParameterType;
                        if (_excludedTypes.Any() && _excludedTypes.ToList().Contains(parameterType))
                        {
                            continue;
                        }

                        var paramNode = targetXmlNode?.SelectSingleNode(string.Format(ParamXPath, parameterInfo.Name));
                        var parameter = operation.Parameters.FirstOrDefault(x => x.Name.Equals(parameterInfo.Name));
                        if (parameter != null)
                        {
                            if (paramNode != null)
                            {
                                if (parameter.Description == null)
                                {
                                    parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
                                }
                                else if (!parameter.Description.Contains(paramNode.InnerXml))
                                {
                                    parameter.Description += XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
                                }

                                continue;
                            }

                            OpenApiSchema schema = null;
                            if (parameter.Schema?.Reference == null)
                            {
                                if (parameter.Schema?.AllOf?.Count > 0)
                                {
                                    schema = context.SchemaRepository.Schemas.FirstOrDefault(s => parameter.Schema.AllOf.FirstOrDefault(a => a.Reference.Id == s.Key) != null).Value;
                                }
                                else
                                {
                                    if (parameter.Description == null)
                                    {
                                        parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
                                    }
                                    else if (!parameter.Description.Contains(paramNode.InnerXml))
                                    {
                                        parameter.Description += XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
                                    }

                                    continue;
                                }
                            }
                            else
                            {
                                var componentReference = parameter.Schema?.Reference?.Id;
                                if (!string.IsNullOrWhiteSpace(componentReference))
                                {
                                    schema = context.SchemaRepository.Schemas[componentReference];
                                }
                            }

                            if (schema != null)
                            {
                                if (schema.Description != null)
                                {
                                    if (parameter.Description == null)
                                    {
                                        parameter.Description = schema.Description;
                                    }
                                    else if (!parameter.Description.Contains(schema.Description))
                                    {
                                        parameter.Description += schema.Description;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}