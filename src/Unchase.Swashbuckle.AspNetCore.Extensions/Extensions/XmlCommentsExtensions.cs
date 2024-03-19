﻿using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    internal static class XmlCommentsExtensions
    {
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ExampleTag = "example";

        internal static Type GetTargetRecursive(this Type type, Dictionary<string, (string Cref, string Path)> inheritedDocs, string cref)
        {
            var targets = GetTargets(type, cref);

            if (!targets.Any())
            {
                return null;
            }

            foreach (var target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                string targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(target);
                if (!string.IsNullOrWhiteSpace(targetMemberName))
                {
                    if (inheritedDocs.ContainsKey(targetMemberName))
                    {
                        return GetTargetRecursive(target, inheritedDocs, inheritedDocs[targetMemberName].Cref);
                    }
                    else
                    {
                        return target;
                    }
                }
            }

            return null;
        }

        private static Type[] GetTargets(Type type, string cref)
        {
            var targets = type.GetInterfaces();
            if (type.BaseType != typeof(object))
            {
                targets = targets.Append(type.BaseType).ToArray();
            }

            // Try to find the target, if one is declared.
            if (!string.IsNullOrWhiteSpace(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForType(t) == cref);

                if (crefTarget != null)
                {
                    return new[] { crefTarget };
                }
            }

            return targets.ToArray();
        }

        internal static MemberInfo GetTargetRecursive(this MemberInfo memberInfo, Dictionary<string, (string Cref, string Path)> inheritedDocs, string cref)
        {
            var targets = GetTargets(memberInfo, cref);

            if (!targets.Any())
            {
                return null;
            }

            foreach (var target in targets)
            {
                if (target == null)
                {
                    continue;
                }

                string targetMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target);
                if (!string.IsNullOrWhiteSpace(targetMemberName))
                {
                    if (inheritedDocs.ContainsKey(targetMemberName))
                    {
                        return GetTargetRecursive(target, inheritedDocs, inheritedDocs[targetMemberName].Cref);
                    }
                    else
                    {
                        return target;
                    }
                }
            }

            return null;
        }

        private static MemberInfo[] GetTargets(MemberInfo memberInfo, string cref)
        {
            var type = memberInfo.DeclaringType ?? memberInfo.ReflectedType;

            if (type == null)
            {
                return null;
            }

            // Find all matching members in all interfaces and the base class.
            var targets = type.GetInterfaces().Append(type.BaseType).SelectMany(x => x.FindMembers(
                memberInfo.MemberType,
                BindingFlags.Instance | BindingFlags.Public,
                (info, _) => info.Name == memberInfo.Name,
                null)).ToList();

            // Try to find the target, if one is declared.
            if (!string.IsNullOrWhiteSpace(cref))
            {
                var crefTarget = targets.SingleOrDefault(t => XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(t) == cref);

                if (crefTarget != null)
                {
                    return new[] { crefTarget };
                }
            }

            return targets.ToArray();
        }

        internal static void ApplyPropertyComments(
            this OpenApiSchema schema,
            MemberInfo memberInfo,
            List<XPathDocument> documents,
            Dictionary<string, (string Cref, string Path)> inheritedDocs,
            bool includeRemarks = false,
            params Type[] excludedTypes)
        {
            if (memberInfo == null)
            {
                return;
            }

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

            string cref = inheritedDocs[memberName].Cref;
            XPathNavigator targetXmlNode;
            if (string.IsNullOrWhiteSpace(cref))
            {
                var target = GetTargetRecursive(memberInfo, inheritedDocs, cref);
                if (target == null)
                {
                    return;
                }

                targetXmlNode = GetMemberXmlNode(XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(target), documents);
            }
            else
            {
                targetXmlNode = GetMemberXmlNode(cref, documents);
            }

            if (targetXmlNode == null)
            {
                return;
            }

            var summaryNode = targetXmlNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null && string.IsNullOrWhiteSpace(schema.Description))
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
                schema.Example = GetExampleValue(memberInfo, exampleNode);
            }
        }

        private static IOpenApiAny GetExampleValue(MemberInfo memberInfo, XPathNavigator exampleNode)
        {
            var type = GetUnderlyingType(memberInfo);
            var exampleValue = exampleNode.InnerXml;

            return string.IsNullOrEmpty(exampleValue)
                ? new OpenApiNull()
                : GetExampleValueAsIOpenApiAny(type, XmlCommentsTextHelper.Humanize(exampleValue));
        }

        private static IOpenApiAny GetExampleValueAsIOpenApiAny(Type type, string exampleString)
        {
            return type == typeof(string)
                ? new OpenApiString(exampleString)
                : OpenApiAnyFactory.CreateFromJson(exampleString);
        }

        private static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    var propType = ((PropertyInfo)member).PropertyType;

                    if (!propType.Name.ToLower().Contains("nullable"))
                    {
                        return propType;
                    }

                    return propType.GenericTypeArguments?.Length > 0 ? propType.GenericTypeArguments[0] : propType;

                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
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