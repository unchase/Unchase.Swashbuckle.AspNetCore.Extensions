using System;
using System.Reflection;
using System.Xml.XPath;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Inject human-friendly remarks to descriptions for Schemas based on XML Comment files.
    /// </summary>
    public class XmlCommentsWithRemarksSchemaFilter : ISchemaFilter
    {
        #region Fields

        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private readonly XPathNavigator _xmlNavigator;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="xmlDoc"><see cref="XPathDocument"/></param>
        public XmlCommentsWithRemarksSchemaFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
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
            ApplyTypeTags(schema, context.Type);

            // If it's for a C# field/property and it's NOT bound to a request parameter (e.g. via [FromRoute], [FromQuery] etc.),
            // then the field/property tags can be applied here. If it is bound to a request parameter, the field/property tags
            // will be applied a level up on the corresponding OpenApiParameter (see XmlCommentsParameterFilter).

            if (context.MemberInfo != null && context.ParameterInfo == null)
            {
                ApplyFieldOrPropertyTags(schema, context.MemberInfo);
            }
        }

        private void ApplyTypeTags(OpenApiSchema schema, Type type)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
            var typeSummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/{SummaryTag}");

            if (typeSummaryNode != null)
            {
                var typeRemarksNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{typeMemberName}']/{RemarksTag}");
                if (typeRemarksNode != null && !string.IsNullOrWhiteSpace(typeRemarksNode.InnerXml))
                {
                    schema.Description +=
                        $" ({XmlCommentsTextHelper.Humanize(typeRemarksNode.InnerXml)})";
                }
            }
        }

        private void ApplyFieldOrPropertyTags(OpenApiSchema schema, MemberInfo fieldOrPropertyInfo)
        {
            var fieldOrPropertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(fieldOrPropertyInfo);
            var fieldOrPropertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{fieldOrPropertyMemberName}']");

            var summaryNode = fieldOrPropertyNode?.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                var remarksNode = fieldOrPropertyNode.SelectSingleNode(RemarksTag);
                if (remarksNode != null
                    && !string.IsNullOrWhiteSpace(remarksNode.InnerXml)
                    && !schema.Description.Contains(XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)))
                {
                    schema.Description +=
                        $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                }
            }
        }

        #endregion
    }
}
