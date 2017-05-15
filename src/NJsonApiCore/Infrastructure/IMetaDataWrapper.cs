using System;
using System.Collections.Generic;

namespace NJsonApi.Infrastructure
{
    //implemented by action result inner content ??

    /// <summary>
    /// Defines the type needed to be implemented to return custom "meta"data in the final JSON-API result.
    /// Example of this occur in list where the meta object contains next, prev among other links
    /// </summary>
    public interface IMetaDataWrapper
    {
        Dictionary<string, object> MetaData { get; }
        object GetValue();
    }
}