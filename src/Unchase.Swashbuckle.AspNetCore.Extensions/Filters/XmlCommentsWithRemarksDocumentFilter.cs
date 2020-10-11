using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Inject human-friendly remarks to descriptions for Document's Tags based on XML Comment files.
    /// </summary>
    public class XmlCommentsWithRemarksDocumentFilter : IDocumentFilter
    {
        #region Fields

        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";

        private readonly XPathNavigator _xmlNavigator;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="xmlDoc"><see cref="XPathDocument"/></param>
        public XmlCommentsWithRemarksDocumentFilter(XPathDocument xmlDoc)
        {
            _xmlNavigator = xmlDoc.CreateNavigator();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="swaggerDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/>.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Collect (unique) controller names and types in a dictionary
            var controllerNamesAndTypes = context.ApiDescriptions
                .Select(apiDesc => apiDesc.ActionDescriptor as ControllerActionDescriptor)
                .SkipWhile(actionDesc => actionDesc == null)
                .GroupBy(actionDesc => actionDesc.ControllerName)
                .Select(group => new KeyValuePair<string, Type>(group.Key, group.First().ControllerTypeInfo.AsType()));

            foreach (var nameAndType in controllerNamesAndTypes)
            {
                var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);
                var typeNode = _xmlNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));

                var summaryNode = typeNode?.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                {
                    var remarksNode = typeNode.SelectSingleNode(RemarksTag);
                    if (remarksNode != null && !string.IsNullOrWhiteSpace(remarksNode.InnerXml))
                    {
                        var tag = swaggerDoc.Tags.FirstOrDefault(t => t.Name.Equals(nameAndType.Key));
                        if (tag != null
                            && !tag.Description.Contains(XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)))
                        {
                            swaggerDoc.Tags.First(t => t.Name.Equals(nameAndType.Key)).Description +=
                                $" ({XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)})";
                        }
                    }
                }
            }
        }

        #endregion
    }
}
