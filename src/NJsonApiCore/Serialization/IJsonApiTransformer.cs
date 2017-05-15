using NJsonApi.Infrastructure;
using NJsonApi.Serialization.Documents;
using System;

namespace NJsonApi.Serialization
{
    /// <summary>
    /// Interface that defines how the transformation from an Action result into a JSON-API document is done
    /// </summary>
    public interface IJsonApiTransformer
    {
        CompoundDocument Transform(Exception e, int httpStatus);

        /// <summary>
        /// Transform a successful action result into a JSON-API document <see cref="CompoundDocument"/>
        /// </summary>
        /// <param name="objectGraph">Generated result</param>
        /// <param name="context">General context to aid the transformation</param>
        /// <returns>First level JSON-API document ready to be serialized and returned</returns>
        CompoundDocument Transform(object objectGraph, Context context);

        IDelta TransformBack(UpdateDocument updateDocument, Type type, Context context);
    }
}