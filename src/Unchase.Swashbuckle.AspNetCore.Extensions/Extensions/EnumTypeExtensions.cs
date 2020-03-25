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
    internal static class EnumTypeExtensions
    {
        private static string GetDescriptionFromEnumOption(Type enumOptionType, object enumOption)
        {
            return enumOptionType.GetFieldAttributeDescription(enumOption, 0);
        }

        private static string GetFieldAttributeDescription(this Type enumType, object enumField, int attributeNumber)
        {
            if (!enumType.IsEnum)
                return string.Empty;
            var memInfo = enumType.GetMember(enumField.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return (attributes[attributeNumber] as DescriptionAttribute)?.Description ?? string.Empty;
            return string.Empty;
        }

        internal static List<OpenApiString> GetEnumValuesDescription(Type enumType, IEnumerable<XPathNavigator> xmlNavigators = null)
        {
            var enumsDescriptions = new List<OpenApiString>();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var enumDescription = GetDescriptionFromEnumOption(enumType, enumValue);
                if (string.IsNullOrWhiteSpace(enumDescription))
                {
                    var memberInfo = enumType.GetMembers().FirstOrDefault(m => m.Name.Equals(enumValue.ToString(), StringComparison.InvariantCultureIgnoreCase));
                    enumDescription = TryGetMemberComments(memberInfo, xmlNavigators);
                }
                enumsDescriptions.Add(new OpenApiString(enumDescription));
            }
            return enumsDescriptions;
        }

        private static string TryGetMemberComments(MemberInfo memberInfo, IEnumerable<XPathNavigator> xmlNavigators)
        {
            if (xmlNavigators == null)
                return string.Empty;

            try
            {
                foreach (var xmlNavigator in xmlNavigators)
                {
                    var xpathNavigator1 = xmlNavigator.SelectSingleNode(
                        $"/doc/members/member[@name='{XmlCommentsNodeNameHelper.GetNodeNameForMember(memberInfo)}']");
                    var xpathNavigator2 = xpathNavigator1?.SelectSingleNode("summary");
                    return xpathNavigator2 != null ? XmlCommentsTextHelper.Humanize(xpathNavigator2.InnerXml) : string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }

            return string.Empty;
        }

        internal static string AddEnumValuesDescription(this OpenApiSchema schema, bool includeDescriptionFromAttribute = false)
        {
            if (schema.Enum == null || schema.Enum.Count == 0)
                return null;

            if (!schema.Extensions.ContainsKey("x-enumNames") || ((OpenApiArray)schema.Extensions["x-enumNames"]).Count != schema.Enum.Count)
                return null;

            var sb = new StringBuilder();
            for (var i = 0; i < schema.Enum.Count; i++)
            {
                if (schema.Enum[i] is OpenApiInteger schemaEnumInt)
                {
                    var value = schemaEnumInt.Value;
                    var name = ((OpenApiString)((OpenApiArray)schema.Extensions["x-enumNames"])[i]).Value;
                    sb.Append($"{Environment.NewLine}{Environment.NewLine}{value} = {name}");
                }
                else if (schema.Enum[i] is OpenApiString schemaEnumString)
                {
                    var value = schemaEnumString.Value;
                    sb.Append($"{Environment.NewLine}{Environment.NewLine}{value}");
                }

                // add description from DescriptionAttribute
                if (includeDescriptionFromAttribute)
                {
                    if (!schema.Extensions.ContainsKey("x-enumDescriptions"))
                        continue;

                    var xEnumDescriptions = (OpenApiArray)schema.Extensions["x-enumDescriptions"];
                    if (xEnumDescriptions?.Count == schema.Enum.Count)
                    {
                        var description = ((OpenApiString)((OpenApiArray)schema.Extensions["x-enumDescriptions"])[i]).Value;
                        sb.Append($" ({description})");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
