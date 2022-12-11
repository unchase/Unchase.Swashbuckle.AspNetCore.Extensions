using System;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Inject human-friendly remarks to descriptions for RequestBodies based on XML Comment files.
    /// </summary>
    internal class XmlCommentsWithRemarksRequestBodyFilter :
        IRequestBodyFilter
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
        public XmlCommentsWithRemarksRequestBodyFilter(
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
        /// <param name="requestBody"><see cref="OpenApiRequestBody"/>.</param>
        /// <param name="context"><see cref="RequestBodyFilterContext"/>.</param>
        public void Apply(
            OpenApiRequestBody requestBody,
            RequestBodyFilterContext context)
        {
            var bodyParameterDescription = context.BodyParameterDescription;

            if (bodyParameterDescription == null)
            {
                return;
            }

            var propertyInfo = bodyParameterDescription.PropertyInfo();
            if (propertyInfo != null)
            {
                ApplyPropertyTags(requestBody, propertyInfo);
                return;
            }
        }

        private void ApplyPropertyTags(OpenApiRequestBody requestBody, PropertyInfo propertyInfo)
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
            var propertySummaryNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/{SummaryTag}");

            if (propertySummaryNode != null)
            {
                var propertyRemarksNode = _xmlNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/{RemarksTag}");
                if (propertyRemarksNode != null
                    && !string.IsNullOrWhiteSpace(propertyRemarksNode.InnerXml)
                    && !requestBody.Description.Contains(XmlCommentsTextHelper.Humanize(propertyRemarksNode.InnerXml)))
                {
                    requestBody.Description +=
                        $" ({XmlCommentsTextHelper.Humanize(propertyRemarksNode.InnerXml)})";
                }
            }
        }

        #endregion
    }
}