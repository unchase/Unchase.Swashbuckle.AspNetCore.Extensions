using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Filter for removing Paths and Defenitions from OpenApi documentation without accepted roles.
    /// </summary>
    public class HidePathsAndDefinitionsByRolesDocumentFilter : IDocumentFilter
    {
        #region Fields

        private readonly IEnumerable<string> _acceptedRoles;

        #endregion

        #region Cosntructors

        /// <summary>
        /// Конструктор класса <see cref="HidePathsAndDefinitionsByRolesDocumentFilter"/>.
        /// </summary>
        /// <param name="acceptedRoles"></param>
        public HidePathsAndDefinitionsByRolesDocumentFilter(IEnumerable<string> acceptedRoles)
        {
            this._acceptedRoles = acceptedRoles;
        }

        #endregion

        #region Methods

        private List<string> GetRequiredDefinitions(DocumentFilterContext context, OpenApiReference reference)
        {
            var result = new List<string>();

            if (reference?.Id != null)
            {
                if (!result.Contains(reference?.Id))
                {
                    result.Add(reference.Id);
                }

                var responseSchema = context.SchemaRepository.Schemas[reference?.Id];
                if (responseSchema != null)
                {
                    if (responseSchema.Properties?.Count > 0)
                    {
                        foreach (var schemaProperty in responseSchema.Properties)
                        {
                            if (schemaProperty.Value?.Reference?.Id != null)
                            {
                                result.Add(schemaProperty.Value?.Reference?.Id);
                                var responseSchemaPropertySchema = context.SchemaRepository.Schemas[schemaProperty.Value?.Reference?.Id];
                                if (responseSchemaPropertySchema?.Reference != null)
                                {
                                    result.AddRange(GetRequiredDefinitions(context, responseSchemaPropertySchema.Reference));
                                    if (responseSchemaPropertySchema.Items?.Reference?.Id != null)
                                    {
                                        var itemsSchema = context.SchemaRepository.Schemas[responseSchemaPropertySchema.Items?.Reference?.Id];
                                        if (itemsSchema?.Reference?.Id != null)
                                        {
                                            result.AddRange(GetRequiredDefinitions(context, itemsSchema.Reference));
                                        }
                                    }
                                }
                            }

                            if (schemaProperty.Value?.Items?.Reference?.Id != null)
                            {
                                result.Add(schemaProperty.Value?.Items?.Reference?.Id);
                                var itemsSchema = context.SchemaRepository.Schemas[schemaProperty.Value?.Items?.Reference?.Id];
                                if (itemsSchema?.Reference?.Id != null)
                                {
                                    result.AddRange(GetRequiredDefinitions(context, itemsSchema.Reference));
                                }

                                if (itemsSchema?.Properties?.Count > 0)
                                {
                                    foreach (var itemsSchemaProperty in itemsSchema.Properties)
                                    {
                                        if (itemsSchemaProperty.Value?.Reference?.Id != null)
                                        {
                                            result.Add(itemsSchemaProperty.Value?.Reference?.Id);
                                        }

                                        if (itemsSchemaProperty.Value?.AllOf?.Count > 0)
                                        {
                                            foreach (var itemsSchemaPropertyAllOf in itemsSchemaProperty.Value?.AllOf)
                                            {
                                                if (itemsSchemaPropertyAllOf?.Reference?.Id != null)
                                                {
                                                    result.AddRange(GetRequiredDefinitions(context, itemsSchemaPropertyAllOf?.Reference));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (schemaProperty.Value?.AllOf?.Count > 0)
                            {
                                foreach (var schemaPropertyAllOf in schemaProperty.Value?.AllOf)
                                {
                                    if (schemaPropertyAllOf?.Reference?.Id != null)
                                    {
                                        result.AddRange(GetRequiredDefinitions(context, schemaPropertyAllOf.Reference));
                                    }
                                }
                            }
                        }
                    }

                    if (responseSchema?.Items?.Reference?.Id != null)
                    {
                        result.Add(responseSchema?.Items?.Reference?.Id);
                        var itemsSchema = context.SchemaRepository.Schemas[responseSchema?.Items?.Reference?.Id];
                        if (itemsSchema?.Reference?.Id != null)
                        {
                            result.AddRange(GetRequiredDefinitions(context, itemsSchema?.Reference));
                        }
                    }
                }
            }

            result = result.Distinct().ToList();

            return result;
        }

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/>.</param>
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {
            if (!this._acceptedRoles.Any())
                return;

            var keysForRemove = new List<string>();
            var refsForProbablyRemove = new List<string>();
            var requiredRefs = new List<string>();

            #region Remove Paths

            foreach (var apiDescriptionActioDescription in context.ApiDescriptions.Select(ad => ad.ActionDescriptor))
            {
                var methodInfo = ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)apiDescriptionActioDescription).MethodInfo;
                var authorizeAttributes = methodInfo.GetCustomAttributes<AuthorizeAttribute>(false);
                if (authorizeAttributes != null)
                {
                    foreach (var authorizeAttribute in authorizeAttributes)
                    {
                        var authorizeAttributeRoles = authorizeAttribute?.Roles?.Split(',').Select(r => r.Trim()).ToList();
                        var intersect = authorizeAttributeRoles?.Intersect(this._acceptedRoles);
                        if (intersect == null || !intersect.Any())
                        {
                            keysForRemove.Add($"/{apiDescriptionActioDescription.AttributeRouteInfo.Template}");
                        }
                    }
                }
            }

            foreach (var keyForRemove in keysForRemove)
            {
                openApiDoc.Paths.Remove(keyForRemove);
            }

            #endregion

            #region Remove components

            foreach (var operation in openApiDoc.Paths?.Where(p => p.Value?.Operations != null).SelectMany(p => p.Value?.Operations))
            {
                if (operation.Value?.Responses != null)
                    foreach (var response in operation.Value?.Responses)
                    {
                        if (response.Value?.Reference?.Id != null && !requiredRefs.Contains(response.Value?.Reference?.Id))
                        {
                            requiredRefs.AddRange(GetRequiredDefinitions(context, response.Value?.Reference));
                            requiredRefs = requiredRefs.Distinct().ToList();
                        }

                        foreach (var responseContentSchema in response.Value?.Content?.Select(rc => rc.Value?.Schema))
                        {
                            if (responseContentSchema?.Reference?.Id != null && !requiredRefs.Contains(responseContentSchema?.Reference?.Id))
                            {
                                requiredRefs.AddRange(GetRequiredDefinitions(context, responseContentSchema?.Reference));
                                requiredRefs = requiredRefs.Distinct().ToList();
                            }
                        }
                    }

                foreach (var parameter in openApiDoc.Paths?.Where(p => p.Value?.Parameters != null).SelectMany(p => p.Value?.Operations?.SelectMany(o => o.Value?.Parameters)))
                {
                    if (parameter?.Schema?.Reference?.Id != null && !requiredRefs.Contains(parameter.Schema.Reference.Id))
                    {
                        requiredRefs.AddRange(GetRequiredDefinitions(context, parameter.Schema.Reference));
                        requiredRefs = requiredRefs.Distinct().ToList();
                    }

                    foreach (var parameterContentSchema in parameter?.Content?.Select(rc => rc.Value?.Schema))
                    {
                        if (parameterContentSchema?.Reference?.Id != null && !requiredRefs.Contains(parameterContentSchema?.Reference?.Id))
                        {
                            requiredRefs.AddRange(GetRequiredDefinitions(context, parameterContentSchema?.Reference));
                            requiredRefs = requiredRefs.Distinct().ToList();
                        }
                    }
                }
            }

            var definitionsToDelete = openApiDoc.Components.Schemas.Select(d => d.Key).Except(requiredRefs).ToList();
            foreach (var definitionToDelete in definitionsToDelete)
            {
                openApiDoc.Components.Schemas.Remove(definitionToDelete);
            }

            #endregion

            #region Remove Tags

            var tagsDict = new Dictionary<string, int>();
            foreach (var openApiDocTag in openApiDoc.Tags)
            {
                if (!tagsDict.ContainsKey(openApiDocTag.Name))
                    tagsDict.Add(openApiDocTag.Name, 0);
            }

            foreach (var operation in openApiDoc.Paths?.Where(p => p.Value?.Operations != null).SelectMany(p => p.Value?.Operations))
            {
                if (operation.Value?.Tags?.Count > 0)
                {
                    foreach (var operationTag in operation.Value?.Tags)
                    {
                        if (operationTag != null && tagsDict.ContainsKey(operationTag.Name))
                        {
                            tagsDict[operationTag.Name]++;
                        }
                    }
                }
            }

            foreach (var tag in tagsDict.Keys)
            {
                if (tagsDict[tag] == 0)
                {
                    var seaggerTag = openApiDoc.Tags.FirstOrDefault(t => t.Name == tag);
                    if (seaggerTag != null)
                        openApiDoc.Tags.Remove(seaggerTag);
                }
            }

            #endregion
        }

        #endregion
    }
}
