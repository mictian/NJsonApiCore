using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NJsonApi.Serialization;
using NJsonApi.Web.MVCCore.BadActionResultTransformers;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NJsonApi.Infrastructure;

namespace NJsonApi.Web
{
    public class JsonApiActionFilter : ActionFilterAttribute
    {
        public bool AllowMultiple => false;
        private readonly IJsonApiTransformer jsonApiTransformer;
        private readonly IConfiguration configuration;
        private readonly JsonSerializer serializer;

        public JsonApiActionFilter(
            IJsonApiTransformer jsonApiTransformer,
            IConfiguration configuration,
            JsonSerializer serializer)
        {
            this.jsonApiTransformer = jsonApiTransformer;
            this.configuration = configuration;
            this.serializer = serializer;
        }

        /// <summary>
        /// Filter invalid JSON API calls 
        /// </summary>
        /// <param name="context">MVC Context of the action that will be executed</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isValidContentType = context.HttpContext.Request.ContentType == configuration.DefaultJsonApiMediaType;
            var controllerType = context.Controller.GetType();
            var isControllerRegistered = configuration.IsControllerRegistered(controllerType);
            var actionDescriptorForBody = context.ActionDescriptor.Parameters.SingleOrDefault(
                    x => x.BindingInfo != null && x.BindingInfo.BindingSource == BindingSource.Body);

            if (isControllerRegistered)
            {
                //Validate that the call not tries to "mutate the state"/ Method <> "GET" while the content type is not JSON API (that is not supported)
                if (!isValidContentType && context.HttpContext.Request.Method != "GET")
                {
                    context.Result = new UnsupportedMediaTypeResult();
                    return;
                }

                //Validate that the GET requests define a valid (*/* or JSON API) Accept header 
                if (!ValidateAcceptHeader(context.HttpContext.Request.Headers))
                {
                    context.Result = new StatusCodeResult(406);
                    return;
                }

                // No sure (force load the content of the body or parse it from a JSONAPI doc?)
                if (actionDescriptorForBody != null)
                {
                    var json = (string)context.ActionDescriptor.Properties[actionDescriptorForBody.Name];
                    using (var stringReader = new StringReader(json))
                    {
                        using (var jsonReader = new JsonTextReader(stringReader))
                        {
                            var updateDocument = serializer.Deserialize(jsonReader, typeof(UpdateDocument)) as UpdateDocument;
                            if (updateDocument != null)
                            {
                                var typeInsideDeltaGeneric = actionDescriptorForBody
                                    .ParameterType
                                    .GenericTypeArguments
                                    .Single();

                                var jsonApiContext = new Context(new Uri(context.HttpContext.Request.Host.Value, UriKind.Absolute));
                                var transformed = jsonApiTransformer.TransformBack(updateDocument, typeInsideDeltaGeneric, jsonApiContext);
                                if (context.ActionArguments.ContainsKey(actionDescriptorForBody.Name))
                                {
                                    context.ActionArguments[actionDescriptorForBody.Name] = transformed;
                                }
                                else
                                {
                                    context.ActionArguments.Add(actionDescriptorForBody.Name, transformed);
                                }

                                context.ModelState.Clear();
                            }
                        }
                    }
                }
            }
            else
            {
                if (actionDescriptorForBody != null)
                {
                    var type = (context.Controller as Controller).ControllerContext.ActionDescriptor.Parameters.First().ParameterType;
                    var json = (string)context.ActionDescriptor.Properties[actionDescriptorForBody.Name];
                    using (var stringReader = new StringReader(json))
                    {
                        using (var jsonReader = new JsonTextReader(stringReader))
                        {
                            var obj = serializer.Deserialize(jsonReader, type);
                            if (context.ActionArguments.ContainsKey(actionDescriptorForBody.Name))
                            {
                                context.ActionArguments[actionDescriptorForBody.Name] = obj;
                            }
                            else
                            {
                                context.ActionArguments.Add(actionDescriptorForBody.Name, obj);
                            }
                        }
                    }
                }

                //When the requested controller is not register and the content type is do valid a 406 is returned (NOT ACCEPTABLE)
                // As there is not contorller althought the content type is valid we cannot generate the content requested
                if (isValidContentType)
                {
                    context.Result = new ContentResult()
                    {
                        StatusCode = 406,
                        Content = $"The Content-Type provided was {context.HttpContext.Request.ContentType} but there was no NJsonApiCore configuration mapping for {controllerType.FullName}"
                    };
                }
            }
        }

        /// <summary>
        /// Serialize the generated result in case it is valid and for a register Controller 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result == null || context.Result is NoContentResult)
            {
                return;
            }

            if (BadActionResultTransformer.IsBadAction(context.Result))
            {
                var transformed = BadActionResultTransformer.Transform(context.Result);

                context.Result = new ObjectResult(transformed)
                {
                    StatusCode = transformed.Errors.First().Status
                };
                return;
            }

            var controllerType = context.Controller.GetType();
            var isControllerRegistered = configuration.IsControllerRegistered(controllerType);

            if (isControllerRegistered)
            {
                var responseResult = (ObjectResult)context.Result;
                var relationshipPaths = FindRelationshipPathsToInclude(context.HttpContext.Request);

                if (!configuration.ValidateIncludedRelationshipPaths(relationshipPaths, responseResult.Value))
                {
                    context.Result = new StatusCodeResult(400);
                    return;
                }

                //Once all validation passed the result is transformed
                var jsonApiContext = new Context(
                    new Uri(context.HttpContext.Request.GetDisplayUrl()),
                    relationshipPaths);
                responseResult.Value = jsonApiTransformer.Transform(responseResult.Value, jsonApiContext);
            }
        }

        private string[] FindRelationshipPathsToInclude(HttpRequest request)
        {
            var includeQueryParameter = request.Query["include"].FirstOrDefault();

            return configuration.FindRelationshipPathsToInclude(includeQueryParameter);
        }

        private bool ValidateAcceptHeader(IHeaderDictionary headers)
        {
            return configuration.ValidateAcceptHeader(headers["Accept"].FirstOrDefault());
        }
    }
}