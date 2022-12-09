using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    /// <summary>
    /// Description sources.
    /// </summary>
    public enum DescriptionSources
    {
        /// <summary>
        /// <see cref="DescriptionAttribute"/>.
        /// </summary>
        DescriptionAttributes = 0,

        /// <summary>
        /// Xml comments.
        /// </summary>
        XmlComments = 1,

        /// <summary>
        /// <see cref="DescriptionAttribute"/> then xml comments.
        /// </summary>
        DescriptionAttributesThenXmlComments = 2
    }

    internal static class EnumTypeExtensions
    {
        #region Methods

        private static string GetDescriptionFromEnumOption(
            Type enumOptionType,
            object enumOption)
        {
            return enumOptionType
                .GetFieldAttributeDescription(enumOption, 0);
        }

        private static string GetFieldAttributeDescription(
            this Type enumType,
            object enumField,
            int attributeNumber)
        {
            if (!enumType.IsEnum)
            {
                return string.Empty;
            }

            var memInfo = enumType.GetMember(enumField.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attributes.Length > 0)
            {
                return (attributes[attributeNumber] as DescriptionAttribute)?.Description ?? string.Empty;
            }

            return string.Empty;
        }

        internal static List<OpenApiString> GetEnumValuesDescription(
            Type enumType,
            DescriptionSources descriptionSource,
            IEnumerable<XPathNavigator> xmlNavigators,
            bool includeRemarks = false)
        {
            var enumsDescriptions = new List<OpenApiString>();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var enumDescription = string.Empty;
                try
                {
                    switch (descriptionSource)
                    {
                        case DescriptionSources.DescriptionAttributes:
                            enumDescription = GetDescriptionFromEnumOption(enumType, enumValue);
                            break;
                        case DescriptionSources.XmlComments:
                            var memberInfo = enumType.GetMembers().FirstOrDefault(m =>
                                m.Name.Equals(enumValue.ToString(), StringComparison.InvariantCultureIgnoreCase));
                            enumDescription = TryGetMemberComments(memberInfo, xmlNavigators, includeRemarks);
                            break;
                        case DescriptionSources.DescriptionAttributesThenXmlComments:
                            enumDescription = GetDescriptionFromEnumOption(enumType, enumValue);
                            if (string.IsNullOrWhiteSpace(enumDescription))
                            {
                                var memberInfo2 = enumType.GetMembers().FirstOrDefault(m =>
                                    m.Name.Equals(enumValue.ToString(), StringComparison.InvariantCultureIgnoreCase));
                                enumDescription = TryGetMemberComments(memberInfo2, xmlNavigators, includeRemarks);
                            }

                            break;
                    }
                }
                catch
                {
                }
                finally
                {
                    if (!string.IsNullOrWhiteSpace(enumDescription))
                    {
                        enumsDescriptions.Add(new OpenApiString(enumDescription));
                    }
                    else
                    {
                        enumsDescriptions.Add(new OpenApiString(string.Empty));
                    }
                }
            }

            return enumsDescriptions;
        }

        private static string TryGetMemberComments(
            MemberInfo memberInfo,
            IEnumerable<XPathNavigator> xmlNavigators,
            bool includeRemarks = false)
        {
            var commentsBuilder = new StringBuilder();
            if (xmlNavigators == null)
            {
                return string.Empty;
            }

            foreach (var xmlNavigator in xmlNavigators)
            {
                var nodeNameForMember = GetNodeNameForMember(memberInfo);
                var xpathMemberNavigator = xmlNavigator.SelectSingleNode(
                    $"/doc/members/member[@name='{nodeNameForMember}']");
                var xpathSummaryNavigator = xpathMemberNavigator?.SelectSingleNode("summary");
                if (xpathSummaryNavigator != null)
                {
                    commentsBuilder.Append(XmlCommentsTextHelper.Humanize(xpathSummaryNavigator.InnerXml));
                    if (includeRemarks)
                    {
                        var xpathRemarksNavigator = xpathMemberNavigator?.SelectSingleNode("remarks");
                        if (xpathRemarksNavigator != null && !string.IsNullOrWhiteSpace(xpathRemarksNavigator.InnerXml))
                        {
                            commentsBuilder.Append($" ({XmlCommentsTextHelper.Humanize(xpathRemarksNavigator.InnerXml)})");
                        }
                    }

                    return commentsBuilder.ToString();
                }
            }

            return string.Empty;
        }

        private static string GetNodeNameForMember(
            MemberInfo memberInfo)
        {
            var stringBuilder = new StringBuilder((memberInfo.MemberType & MemberTypes.Field) != 0 ? "F:" : "P:");
            stringBuilder.Append(QualifiedNameFor(memberInfo.DeclaringType, false));
            stringBuilder.Append("." + memberInfo.Name);
            return stringBuilder.ToString();
        }

        private static string QualifiedNameFor(
            Type type,
            bool expandGenericArgs = false)
        {
            if (type.IsArray)
            {
                return QualifiedNameFor(type.GetElementType(), expandGenericArgs) + "[]";
            }

            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(type.Namespace))
            {
                stringBuilder.Append(type.Namespace + ".");
            }

            if (type.IsNested)
            {
                stringBuilder.Append(type.DeclaringType?.Name + ".");
            }

            if (type.IsConstructedGenericType & expandGenericArgs)
            {
                var str = type.Name.Split('`').First();
                stringBuilder.Append(str);
                var values = type.GetGenericArguments()
                    .Select(t => !t.IsGenericParameter ? QualifiedNameFor(t, true) : string.Format("`{0}", t.GenericParameterPosition));
                stringBuilder.Append("{" + string.Join(",", values) + "}");
            }
            else
            {
                stringBuilder.Append(type.Name);
            }

            return stringBuilder.ToString();
        }

        internal static string AddEnumValuesDescription(
            this OpenApiSchema schema,
            string xEnumNamesAlias,
            string xEnumDescriptionsAlias,
            bool includeDescriptionFromAttribute = false,
            string newLine = "\n")
        {
            if (schema.Enum == null || schema.Enum.Count == 0)
            {
                return null;
            }

            if (!schema.Extensions.ContainsKey(xEnumNamesAlias) ||
                ((OpenApiArray)schema.Extensions[xEnumNamesAlias]).Count != schema.Enum.Count)
            {
                return null;
            }

            var sb = new StringBuilder();
            for (var i = 0; i < schema.Enum.Count; i++)
            {
                if (schema.Enum[i] is OpenApiInteger schemaEnumInt)
                {
                    var value = schemaEnumInt.Value;
                    var name = ((OpenApiString)((OpenApiArray)schema.Extensions[xEnumNamesAlias])[i]).Value;
                    sb.Append($"{newLine}{newLine}{value} = {name}");
                }
                else if (schema.Enum[i] is OpenApiString schemaEnumString)
                {
                    var value = schemaEnumString.Value;
                    sb.Append($"{newLine}{newLine}{value}");
                }

                // add description from DescriptionAttribute
                if (includeDescriptionFromAttribute)
                {
                    if (!schema.Extensions.ContainsKey(xEnumDescriptionsAlias))
                    {
                        continue;
                    }

                    var xEnumDescriptions = (OpenApiArray)schema.Extensions[xEnumDescriptionsAlias];
                    if (xEnumDescriptions?.Count == schema.Enum.Count)
                    {
                        var description = ((OpenApiString)((OpenApiArray)schema.Extensions[xEnumDescriptionsAlias])[i]).Value;
                        if (!string.IsNullOrWhiteSpace(description))
                        {
                            sb.Append($" ({description})");
                        }
                    }
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
