using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Filters
{
    /// <summary>
    /// Filter for removing Paths and Components from OpenApi documentation without accepted roles.
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
        public HidePathsAndDefinitionsByRolesDocumentFilter(
            IEnumerable<string> acceptedRoles)
        {
            _acceptedRoles = acceptedRoles;
        }

        #endregion

        #region Methods

        #region GetRequiredDefinitions

        private static List<string> GetRequiredDefinitions(
            IDictionary<string, OpenApiSchema> schemas,
            OpenApiReference reference)
        {
            var result = new List<string>();

            if (reference?.Id != null)
            {
                if (!result.Contains(reference?.Id))
                {
                    result.Add(reference.Id);
                }

                var responseSchema = schemas[reference?.Id];
                if (responseSchema != null)
                {
                    if (responseSchema.Properties?.Count > 0)
                    {
                        foreach (var schemaProperty in responseSchema.Properties)
                        {
                            if (schemaProperty.Value?.Reference?.Id != null)
                            {
                                result.Add(schemaProperty.Value?.Reference?.Id);
                                var responseSchemaPropertySchema = schemas[schemaProperty.Value?.Reference?.Id];
                                if (responseSchemaPropertySchema?.Reference != null)
                                {
                                    result.AddRange(GetRequiredDefinitions(schemas, responseSchemaPropertySchema.Reference));
                                    if (responseSchemaPropertySchema.Items?.Reference?.Id != null)
                                    {
                                        var itemsSchema = schemas[responseSchemaPropertySchema.Items?.Reference?.Id];
                                        if (itemsSchema?.Reference?.Id != null)
                                        {
                                            result.AddRange(GetRequiredDefinitions(schemas, itemsSchema.Reference));
                                        }
                                    }
                                }
                                else if (responseSchemaPropertySchema != null)
                                {
                                    result.AddRange(GetRequiredDefinitions(schemas, schemaProperty.Value?.Reference));
                                }
                            }

                            if (schemaProperty.Value?.Items?.Reference?.Id != null)
                            {
                                result.Add(schemaProperty.Value?.Items?.Reference?.Id);
                                var itemsSchema = schemas[schemaProperty.Value?.Items?.Reference?.Id];
                                if (itemsSchema?.Reference?.Id != null)
                                {
                                    result.AddRange(GetRequiredDefinitions(schemas, itemsSchema.Reference));
                                }

                                if (itemsSchema?.Properties?.Count > 0)
                                {
                                    foreach (var itemsSchemaProperty in itemsSchema.Properties)
                                    {
                                        if (itemsSchemaProperty.Value?.Reference?.Id != null)
                                        {
                                            result.AddRange(GetRequiredDefinitions(schemas, itemsSchemaProperty.Value?.Reference));
                                            result.Add(itemsSchemaProperty.Value?.Reference?.Id);
                                        }

                                        if (itemsSchemaProperty.Value?.Items?.Reference?.Id != null)
                                        {
                                            result.AddRange(GetRequiredDefinitions(schemas, itemsSchemaProperty.Value?.Items?.Reference));
                                        }

                                        if (itemsSchemaProperty.Value?.AllOf?.Count > 0)
                                        {
                                            foreach (var itemsSchemaPropertyAllOf in itemsSchemaProperty.Value?.AllOf)
                                            {
                                                if (itemsSchemaPropertyAllOf?.Reference?.Id != null)
                                                {
                                                    result.AddRange(GetRequiredDefinitions(schemas, itemsSchemaPropertyAllOf?.Reference));
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
                                        result.AddRange(GetRequiredDefinitions(schemas, schemaPropertyAllOf.Reference));
                                    }
                                }
                            }
                        }
                    }

                    if (responseSchema?.Items?.Reference?.Id != null)
                    {
                        result.Add(responseSchema?.Items?.Reference?.Id);
                        var itemsSchema = schemas[responseSchema?.Items?.Reference?.Id];
                        if (itemsSchema?.Reference?.Id != null)
                        {
                            result.AddRange(GetRequiredDefinitions(schemas, itemsSchema?.Reference));
                        }
                    }
                }
            }

            result = result.Distinct().ToList();

            return result;
        }

        #endregion

        #region RemovePathsAndComponents

        /// <summary>
        /// Remove Paths and Components from OpenApi documentation without accepted roles.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="paths">Dictionary of openApi paths with <see cref="MethodInfo"/> keys.</param>
        /// <param name="schemas">Dictionary with openApi schemas with scheme name keys.</param>
        /// <param name="acceptedRoles">Collection of accepted roles.</param>
        internal static void RemovePathsAndComponents(
            OpenApiDocument openApiDoc,
            IDictionary<(MethodInfo ActionMethodInfo, Type ControllerType), string> paths,
            IDictionary<string, OpenApiSchema> schemas,
            IReadOnlyList<string> acceptedRoles)
        {
            var keysForRemove = new List<string>();
            var requiredRefs = new List<string>();

            #region Remove Paths

            foreach (var path in paths)
            {
                var authorizeAttributes = path.Key.ActionMethodInfo.GetCustomAttributes<AuthorizeAttribute>(true);
                foreach (var authorizeAttribute in authorizeAttributes)
                {
                    var authorizeAttributeRoles = authorizeAttribute?.Roles?.Split(',').Select(r => r.Trim()).ToList();
                    var intersect = authorizeAttributeRoles?.Intersect(acceptedRoles);
                    if (intersect == null || !intersect.Any())
                    {
                        keysForRemove.Add($"/{path.Value}");
                    }
                }

                var controllerAuthorizeAttributes = path.Key.ControllerType.GetCustomAttributes<AuthorizeAttribute>(true);
                foreach (var controllerAuthorizeAttribute in controllerAuthorizeAttributes)
                {
                    var authorizeAttributeRoles = controllerAuthorizeAttribute?.Roles?.Split(',').Select(r => r.Trim()).ToList();
                    var intersect = authorizeAttributeRoles?.Intersect(acceptedRoles);
                    if (intersect == null || !intersect.Any())
                    {
                        keysForRemove.Add($"/{path.Value}");
                    }
                }
            }

            foreach (var keyForRemove in keysForRemove)
            {
                openApiDoc.Paths.Remove(keyForRemove);
            }

            #endregion

            #region Remove Components

            foreach (var operation in openApiDoc?.Paths?.Where(p => p.Value?.Operations != null)?.SelectMany(p => p.Value?.Operations))
            {
                if (operation.Value?.Responses != null)
                {
                    foreach (var response in operation.Value?.Responses)
                    {
                        if (response.Value?.Reference?.Id != null && !requiredRefs.Contains(response.Value?.Reference?.Id))
                        {
                            requiredRefs.AddRange(GetRequiredDefinitions(schemas, response.Value?.Reference));
                            requiredRefs = requiredRefs.Distinct().ToList();
                        }

                        foreach (var responseContentSchema in response.Value?.Content?.Select(rc => rc.Value?.Schema))
                        {
                            foreach (var responseContentSchemaAllOfItemSchema in responseContentSchema.AllOf.Where(s => s.Reference?.Id != null))
                            {
                                if (responseContentSchemaAllOfItemSchema?.Reference?.Id != null && !requiredRefs.Contains(responseContentSchemaAllOfItemSchema?.Reference?.Id))
                                {
                                    requiredRefs.AddRange(GetRequiredDefinitions(schemas, responseContentSchemaAllOfItemSchema?.Reference));
                                    requiredRefs = requiredRefs.Distinct().ToList();
                                }
                            }

                            if (responseContentSchema?.Reference?.Id != null && !requiredRefs.Contains(responseContentSchema?.Reference?.Id))
                            {
                                requiredRefs.AddRange(GetRequiredDefinitions(schemas, responseContentSchema?.Reference));
                                requiredRefs = requiredRefs.Distinct().ToList();
                            }
                        }
                    }
                }

                if (operation.Value?.RequestBody != null)
                {
                    var operationRequestBody = operation.Value?.RequestBody;
                    foreach (var operationRequestBodyContentSchema in operationRequestBody?.Content?.Select(rc => rc.Value?.Schema))
                    {
                        foreach (var operationRequestBodyContentSchemaAllOfItemSchema in operationRequestBodyContentSchema.AllOf.Where(s => s.Reference?.Id != null))
                        {
                            if (operationRequestBodyContentSchemaAllOfItemSchema?.Reference?.Id != null && !requiredRefs.Contains(operationRequestBodyContentSchemaAllOfItemSchema?.Reference?.Id))
                            {
                                requiredRefs.AddRange(GetRequiredDefinitions(schemas, operationRequestBodyContentSchemaAllOfItemSchema?.Reference));
                                requiredRefs = requiredRefs.Distinct().ToList();
                            }
                        }

                        if (operationRequestBodyContentSchema?.Reference?.Id != null && !requiredRefs.Contains(operationRequestBodyContentSchema?.Reference?.Id))
                        {
                            requiredRefs.AddRange(GetRequiredDefinitions(schemas, operationRequestBodyContentSchema?.Reference));
                            requiredRefs = requiredRefs.Distinct().ToList();
                        }
                    }
                }

                foreach (var parameter in openApiDoc.Paths?.Where(p => p.Value?.Parameters != null).SelectMany(p => p.Value?.Operations?.SelectMany(o => o.Value?.Parameters)))
                {
                    if (parameter?.Schema?.Reference?.Id != null && !requiredRefs.Contains(parameter.Schema.Reference.Id))
                    {
                        requiredRefs.AddRange(GetRequiredDefinitions(schemas, parameter.Schema.Reference));
                        requiredRefs = requiredRefs.Distinct().ToList();
                    }

                    foreach (var parameterContentSchema in parameter?.Content?.Select(rc => rc.Value?.Schema))
                    {
                        foreach (var parameterContentSchemaAllOfItemSchema in parameterContentSchema.AllOf.Where(s => s.Reference?.Id != null))
                        {
                            if (parameterContentSchemaAllOfItemSchema?.Reference?.Id != null && !requiredRefs.Contains(parameterContentSchemaAllOfItemSchema?.Reference?.Id))
                            {
                                requiredRefs.AddRange(GetRequiredDefinitions(schemas, parameterContentSchemaAllOfItemSchema?.Reference));
                                requiredRefs = requiredRefs.Distinct().ToList();
                            }
                        }

                        if (parameterContentSchema?.Reference?.Id != null && !requiredRefs.Contains(parameterContentSchema?.Reference?.Id))
                        {
                            requiredRefs.AddRange(GetRequiredDefinitions(schemas, parameterContentSchema?.Reference));
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
                {
                    tagsDict.Add(openApiDocTag.Name, 0);
                }
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
                    var swaggerTag = openApiDoc.Tags.FirstOrDefault(t => t.Name == tag);
                    if (swaggerTag != null)
                    {
                        openApiDoc.Tags.Remove(swaggerTag);
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Apply

        /// <summary>
        /// Apply filter.
        /// </summary>
        /// <param name="openApiDoc"><see cref="OpenApiDocument"/>.</param>
        /// <param name="context"><see cref="DocumentFilterContext"/>.</param>
        public void Apply(
            OpenApiDocument openApiDoc,
            DocumentFilterContext context)
        {
            if (!_acceptedRoles.Any())
            {
                return;
            }

            var apiDescriptions = context.ApiDescriptions;
            var schemas = context.SchemaRepository.Schemas;

            var paths = new Dictionary<(MethodInfo, Type), string>();
            foreach (var actionDescriptor in apiDescriptions.Select(ad => ad.ActionDescriptor))
            {
                var t = ((ControllerActionDescriptor) actionDescriptor).ControllerTypeInfo.AsType();
                paths.Add((((ControllerActionDescriptor)actionDescriptor).MethodInfo, t), actionDescriptor.AttributeRouteInfo.Template);
            }

            RemovePathsAndComponents(openApiDoc, paths, schemas, _acceptedRoles.ToList());
        }

        #endregion

        #endregion
    }
}