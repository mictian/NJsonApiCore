using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing.Template;

namespace NJsonApi.Web.MVCCore
{
    internal static class ApiExplorerExtensions
    {
        /// <summary>
        /// Extension method to find the action group (representat of the invoked controller)
        /// </summary>
        /// <param name="provider">This</param>
        /// <param name="controller">Executed controller type</param>
        /// <returns>Controller representation on the .NET core way</returns>
        public static ApiDescriptionGroup From(this IApiDescriptionGroupCollectionProvider provider, Type controller)
        {
            foreach (var actionGroup in provider.ApiDescriptionGroups.Items)
            {
                foreach (var action in actionGroup.Items)
                {
                    var controllerAction = action.ActionDescriptor as ControllerActionDescriptor;
                    if (controllerAction == null)
                    {
                        continue;
                    }

                    if (controllerAction.ControllerTypeInfo.FullName == controller.FullName)
                    {
                        return actionGroup;
                    }
                }
            }
            return null;
        }

        public static string ToPlaceholder(this TemplatePart part)
        {
            if (part.IsParameter)
            {
                return "{" + (part.IsCatchAll ? "*" : string.Empty) + part.Name + (part.IsOptional ? "?" : string.Empty) + "}";
            }
            else
            {
                return part.Text;
            }
        }
    }
}