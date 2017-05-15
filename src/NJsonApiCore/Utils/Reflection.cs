using NJsonApi.Infrastructure;
using System;
using System.Collections;
using System.Reflection;

namespace NJsonApi.Utils
{
    /// <summary>
    /// Utility class that provides reflection related functionality
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Given an object returns its type ready to be used for JSON transformer
        /// </summary>
        /// <param name="objectGraph">Action result to be serialized</param>
        /// <returns>Inner type for enumerables or object's type</returns>
        public static Type GetObjectType(object objectGraph)
        {
            Type objectType = objectGraph.GetType();

            //TODO: Handle the case when the object is not generic.
            //TODO: Why is really do this??
            if (objectGraph is IMetaDataWrapper && objectType.GetTypeInfo().IsGenericType)
            {
                objectType = objectGraph.GetType().GetGenericArguments()[0];
            }

            if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                if (objectType.IsArray)
                {
                    objectType = objectType.GetElementType();
                }
                else if (objectType.GetTypeInfo().IsGenericType)
                {
                    objectType = objectType.GetGenericArguments()[0];
                }
                
            }

            return objectType;
        }

        public static Type[] FromWithinGeneric(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsConstructedGenericType)
            {
                return type.GetGenericArguments();
            }
            else
            {
                return new Type[] { type };
            }
        }
    }
}
