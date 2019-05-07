using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Swagger;

namespace Unchase.Swashbuckle.AspNetCore.Extensions.Extensions
{
    public static class SwaggerDocumentExtensions
    {
        public static void HidePathItemsWithoutAcceptedRoles(this SwaggerDocument swaggerDoc, IEnumerable<string> acceptedRoles)
        {
            var acceptedRolesList = acceptedRoles.ToList();
            foreach (var swaggerDocPath in swaggerDoc.Paths.Values)
            {
                if (!HasNecessaryRoles(swaggerDocPath.Get, acceptedRolesList))
                    swaggerDocPath.Get = null;

                if (!HasNecessaryRoles(swaggerDocPath.Post, acceptedRolesList))
                    swaggerDocPath.Post = null;

                if (!HasNecessaryRoles(swaggerDocPath.Patch, acceptedRolesList))
                    swaggerDocPath.Patch = null;

                if (!HasNecessaryRoles(swaggerDocPath.Delete, acceptedRolesList))
                    swaggerDocPath.Delete = null;

                if (!HasNecessaryRoles(swaggerDocPath.Put, acceptedRolesList))
                    swaggerDocPath.Put = null;
            }
        }

        internal static bool HasNecessaryRoles(Operation swaggerDocPathOperation, List<string> acceptedRoles)
        {
            var founded = false;
            if (swaggerDocPathOperation?.Security == null)
                return true;

            if (!acceptedRoles.Any())
                return true;

            foreach (var operationSecurity in swaggerDocPathOperation.Security)
            {
                if (founded)
                    break;

                if (operationSecurity != null)
                {
                    foreach (var operationSecurityValue in operationSecurity.Values)
                    {
                        if (founded)
                            break;

                        var operationSecurityValueRuntimeFields = operationSecurityValue?.GetType().GetRuntimeFields();
                        if (operationSecurityValueRuntimeFields != null)
                        {
                            foreach (var operationSecurityValueRuntimeField in operationSecurityValueRuntimeFields)
                            {
                                if (founded)
                                    break;

                                if (operationSecurityValueRuntimeField?.GetValue(operationSecurityValue) is List<AuthorizeAttribute> authorizeAttributes)
                                {
                                    foreach (var authorizeAttribute in authorizeAttributes)
                                    {
                                        var authorizeAttributeRoles = authorizeAttribute?.Roles?.Split(',').Select(r => r.Trim()).ToList();
                                        var intersect = authorizeAttributeRoles?.Intersect(acceptedRoles);
                                        if (intersect == null || !intersect.Any())
                                        {
                                            founded = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return !founded;
        }
    }
}
