using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Adds documentation that is provided by the &lt;inhertidoc /&gt; tag.
    /// </summary>
    /// <seealso cref="ISchemaFilter" />
    internal class InheritDocSchemaFilter : ISchemaFilter
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
        /// Initializes a new instance of the <see cref="InheritDocSchemaFilter" /> class.
        /// </summary>
        /// <param name="options"><see cref="SwaggerGenOptions"/>.</param>
        /// <param name="includeRemarks">Include remarks from inheritdoc XML comments.</param>
        /// <param name="excludedTypes">Excluded types.</param>
        public InheritDocSchemaFilter(SwaggerGenOptions options, bool includeRemarks = false, params Type[] excludedTypes)
        {
            _includeRemarks = includeRemarks;
            _excludedTypes = excludedTypes;
            _documents = options.SchemaFilterDescriptors.Where(x => x.Type == typeof(XmlCommentsSchemaFilter))
                .Select(x => x.Arguments.Single())
                .Cast<XPathDocument>()
                .ToList();

            _inheritedDocs = _documents.SelectMany(
                    doc =>
                    {
                        var inheritedElements = new List<(string, string)>();
                        foreach (XPathNavigator member in doc.CreateNavigator().Select("doc/members/member/inheritdoc"))
                        {
                            var cref = member.GetAttribute("cref", "");
                            member.MoveToParent();
                            var parentCref = member.GetAttribute("cref", "");
                            if (!string.IsNullOrWhiteSpace(parentCref))
                                cref = parentCref;
                            inheritedElements.Add((member.GetAttribute("name", ""), cref));
                        }

                        return inheritedElements;
                    })
                .ToDictionary(x => x.Item1, x => x.Item2);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="schema"><see cref="OpenApiSchema"/>.</param>
        /// <param name="context"><see cref="SchemaFilterContext"/>.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (_excludedTypes.Any() && _excludedTypes.ToList().Contains(context.Type))
            {
                return;
            }

            // Try to apply a description for inherited types.
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(context.Type);
            if (string.IsNullOrEmpty(schema.Description) && _inheritedDocs.ContainsKey(memberName))
            {
                var cref = _inheritedDocs[memberName];
                var target = GetTargetRecursive(context.Type, cref);

                var targetXmlNode = GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForType(target));
                var summaryNode = targetXmlNode?.SelectSingleNode(SummaryTag);

                if (summaryNode != null)
                {
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                    if (_includeRemarks)
                    {
                        var remarksNode = targetXmlNode.SelectSingleNode(RemarksTag);
                        if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                        {
                            schema.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                        }
                    }
                }
            }

            if (schema.Properties == null)
                return;

            // Add the summary and examples for the properties.
            foreach (var entry in schema.Properties)
            {
                var memberInfo = ((TypeInfo) context.Type).DeclaredMembers?.FirstOrDefault(p => p.Name.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
                if (memberInfo != null)
                {
                    ApplyPropertyComments(entry.Value, memberInfo);
                }
            }
        }

        private static MemberInfo GetTarget(MemberInfo memberInfo, string cref)
        {
            var type = memberInfo.DeclaringType ?? memberInfo.ReflectedType;

            if (type == null)
                return null;

            // Find all matching members in all interfaces and the base class.
            var targets = type.GetInterfaces()
                .Append(type.BaseType)
                .SelectMany(
                    x => x.FindMembers(
                        memberInfo.MemberType,
                        BindingFlags.Instance | BindingFlags.Public,
                        (info, criteria) => info.Name == memberInfo.Name,
                        null))
                .ToList();

            // Try to find the target, if one is declared.
            if (!string.IsNullOrEmpty(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(t) == cref);

                if (crefTarget != null)
                    return crefTarget;
            }

            // We use the last since that will be our base class or the "nearest" implemented interface.
            return targets.LastOrDefault();
        }

        private static Type GetTarget(Type type, string cref)
        {
            var targets = type.GetInterfaces();
            if (type.BaseType != typeof(object))
                targets = targets.Append(type.BaseType).ToArray();

            // Try to find the target, if one is declared.
            if (!string.IsNullOrEmpty(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForType(t) == cref);

                if (crefTarget != null)
                    return crefTarget;
            }

            // We use the last since that will be our base class or the "nearest" implemented interface.
            return targets.LastOrDefault();
        }

        private void ApplyPropertyComments(OpenApiSchema propertySchema, MemberInfo memberInfo)
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(memberInfo);

            if (!_inheritedDocs.ContainsKey(memberName))
                return;

            if (_excludedTypes.Any() && _excludedTypes.ToList()
                .Contains(((PropertyInfo)memberInfo).PropertyType))
            {
                return;
            }

            var cref = _inheritedDocs[memberName];
            var target = GetTargetRecursive(memberInfo, cref);

            var targetXmlNode = GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target));

            if (targetXmlNode == null)
                return;

            var summaryNode = targetXmlNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                propertySchema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                if (_includeRemarks)
                {
                    var remarksNode = targetXmlNode.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        propertySchema.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                    }
                }
            }

            var exampleNode = targetXmlNode.SelectSingleNode(ExampleTag);
            if (exampleNode != null)
                propertySchema.Example = new OpenApiString(XmlCommentsTextHelper.Humanize(exampleNode.InnerXml));
        }

        private XPathNavigator GetMemberXmlNode(string memberName)
        {
            var path = $"/doc/members/member[@name='{memberName}']";

            foreach (var document in _documents)
            {
                var node = document.CreateNavigator().SelectSingleNode(path);

                if (node != null)
                    return node;
            }

            return null;
        }

        private MemberInfo GetTargetRecursive(MemberInfo memberInfo, string cref)
        {
            var target = GetTarget(memberInfo, cref);

            if (target == null)
                return null;

            var targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target);

            if (_inheritedDocs.ContainsKey(targetMemberName))
                return GetTarget(target, _inheritedDocs[targetMemberName]);

            return target;
        }

        private Type GetTargetRecursive(Type type, string cref)
        {
            var target = GetTarget(type, cref);

            if (target == null)
                return null;

            var targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(target);

            if (_inheritedDocs.ContainsKey(targetMemberName))
                return GetTarget(target, _inheritedDocs[targetMemberName]);

            return target;
        }

        #endregion
    }
}
