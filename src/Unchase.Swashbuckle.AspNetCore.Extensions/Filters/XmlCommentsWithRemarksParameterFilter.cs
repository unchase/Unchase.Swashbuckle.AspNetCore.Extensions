using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Inject human-friendly remarks to descriptions for Parameters based on XML Comment files.
    /// </summary>
    internal class XmlCommentsWithRemarksParameterFilter :
        IParameterFilter
    {
        #region Fields

        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private readonly XPathNavigator _xmlNavigator;
        private readonly Type[] _excludedTypes;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="xmlDoc"><see cref="XPathDocument"/></param>
        /// <param name="excludedTypes">Excluded types.</param>
        public XmlCommentsWithRemarksParameterFilter(
            XPathDocument xmlDoc,
            params Type[] excludedTypes)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
            _excludedTypes = excludedTypes;
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
            if (context.PropertyInfo != null)
            {
                ApplyPropertyTags(parameter, context.PropertyInfo);
            }
        }

        private void ApplyPropertyTags(OpenApiParameter parameter, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return;
            }

            if (propertyInfo.DeclaringType != null && _excludedTypes.Any() && _excludedTypes.ToList().Contains(propertyInfo.DeclaringType))
            {
                return;
            }

            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
            var propertyNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']");

            var summaryNode = propertyNode?.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
            {
                var remarksNode = propertyNode.SelectSingleNode(RemarksTag);
                if (remarksNode != null
                    && !string.IsNullOrWhiteSpace(remarksNode.InnerXml)
                    && !parameter.Description.Contains(XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)))
                {
                    parameter.Description +=
                        $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                }
            }
        }

        #endregion
    }
}