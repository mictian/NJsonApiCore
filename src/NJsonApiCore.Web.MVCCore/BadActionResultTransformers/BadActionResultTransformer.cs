using NJsonApi.Serialization.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace NJsonApi.Web.MVCCore.BadActionResultTransformers
{
    /// <summary>
    /// Transform a "Bad" result into a JSON API format.
    /// This class use a pre-defined list of bad response transformers
    /// </summary>
    public static class BadActionResultTransformer
    {
        private static readonly List<ICanTransformBadActions> badActionRegistry = new List<ICanTransformBadActions>();

        static BadActionResultTransformer()
        {
            badActionRegistry.Add(new TransformBadRequestObjectResult());
            badActionRegistry.Add(new TransformBadRequestResult());
            badActionRegistry.Add(new TransformHttpNotFoundObjectResult());
            badActionRegistry.Add(new TransformHttpNotFoundResult());
            badActionRegistry.Add(new TransformHttpUnauthorizedResult());
        }

        private static ICanTransformBadActions FindTransformer(IActionResult badActionResult)
        {
            return badActionRegistry.Single(x => x.Accepts(badActionResult));
        }

        public static bool IsBadAction(IActionResult potentiallyBadsAction)
        {
            return badActionRegistry.Any(x => x.Accepts(potentiallyBadsAction));
        }

        public static CompoundDocument Transform(IActionResult badActionResult)
        {
            return FindTransformer(badActionResult).Transform(badActionResult);
        }
    }
}