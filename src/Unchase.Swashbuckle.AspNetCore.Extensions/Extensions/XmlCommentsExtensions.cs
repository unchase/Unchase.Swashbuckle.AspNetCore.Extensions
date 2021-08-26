using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    internal static class XmlCommentsExtensions
    {
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";

        internal static Type GetTargetRecursive(this Type type, Dictionary<string, string> inheritedDocs, string cref)
        {
            var target = GetTarget(type, cref);

            if (target == null)
            {
                return null;
            }

            string targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(target);

            if (inheritedDocs.ContainsKey(targetMemberName))
            {
                return GetTarget(target, inheritedDocs[targetMemberName]);
            }

            return target;
        }

        private static Type GetTarget(Type type, string cref)
        {
            var targets = type.GetInterfaces();
            if (type.BaseType != typeof(object))
            {
                targets = targets.Append(type.BaseType).ToArray();
            }

            // Try to find the target, if one is declared.
            if (!string.IsNullOrEmpty(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForType(t) == cref);

                if (crefTarget != null)
                {
                    return crefTarget;
                }
            }

            // We use the last since that will be our base class or the "nearest" implemented interface.
            return targets.LastOrDefault();
        }

        internal static MemberInfo GetTargetRecursive(this MemberInfo memberInfo, Dictionary<string, string> inheritedDocs, string cref)
        {
            var target = GetTarget(memberInfo, cref);

            if (target == null)
            {
                return null;
            }

            string targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target);

            if (inheritedDocs.ContainsKey(targetMemberName))
            {
                return GetTarget(target, inheritedDocs[targetMemberName]);
            }

            return target;
        }

        private static MemberInfo GetTarget(MemberInfo memberInfo, string cref)
        {
            var type = memberInfo.DeclaringType ?? memberInfo.ReflectedType;

            if (type == null)
            {
                return null;
            }

            // Find all matching members in all interfaces and the base class.
            var targets = type.GetInterfaces()
                .Append(type.BaseType)
                .SelectMany(
                    x => x.FindMembers(
                        memberInfo.MemberType,
                        BindingFlags.Instance | BindingFlags.Public,
                        (info, _) => info.Name == memberInfo.Name,
                        null))
                .ToList();

            // Try to find the target, if one is declared.
            if (!string.IsNullOrEmpty(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(t) == cref);

                if (crefTarget != null)
                {
                    return crefTarget;
                }
            }

            // We use the last since that will be our base class or the "nearest" implemented interface.
            return targets.LastOrDefault();
        }

        internal static void ApplyPropertyComments(
            this OpenApiSchema schema, 
            MemberInfo memberInfo, 
            List<XPathDocument> documents, 
            Dictionary<string, string> inheritedDocs, 
            bool includeRemarks = false, 
            params Type[] excludedTypes)
        {
            string memberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(memberInfo);

            if (!inheritedDocs.ContainsKey(memberName))
            {
                return;
            }

            if (excludedTypes.Any() && excludedTypes.ToList()
                .Contains(((PropertyInfo)memberInfo).PropertyType))
            {
                return;
            }

            string cref = inheritedDocs[memberName];
            var target = memberInfo.GetTargetRecursive(inheritedDocs, cref);
            if (target == null)
            {
                return;
            }

            var targetXmlNode = GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target), documents);

            if (targetXmlNode == null)
            {
                return;
            }

            var summaryNode = targetXmlNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

                if (includeRemarks)
                {
                    var remarksNode = targetXmlNode.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        schema.Description += $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                    }
                }
            }

            var exampleNode = targetXmlNode.SelectSingleNode(ExampleTag);
            if (exampleNode != null)
            {
                schema.Example = new OpenApiString(XmlCommentsTextHelper.Humanize(exampleNode.InnerXml));
            }
        }

        internal static XPathNavigator GetMemberXmlNode(string memberName, List<XPathDocument> documents)
        {
            string path = $"/doc/members/member[@name='{memberName}']";

            foreach (var document in documents)
            {
                var node = document.CreateNavigator().SelectSingleNode(path);

                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
