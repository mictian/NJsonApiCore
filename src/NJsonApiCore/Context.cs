using System;

namespace NJsonApi
{
    /// <summary>
    /// Context used in the Serialization of Actions result into JSON-API JSON string.
    /// This class can be seen as a DTO as it does not contains logic
    /// </summary>
    public class Context
    {
        #region Constructors

        /// <summary>
        /// Instanciate a new JSON-API transformation context with only a request URI
        /// </summary>
        /// <param name="requestUri">URI that initiate the request</param>
        public Context(Uri requestUri)
        {
            IncludedResources = new string[0];
            RequestUri = requestUri;
        }

        /// <summary>
        /// Instantiate a new JSON-API transformation context where a list of related resources is also requested
        /// </summary>
        /// <param name="requestUri">URI that initiate the request</param>
        /// <param name="includedResources">List of included resources also requested in the initial URL.
        /// <example>http://localhost:60101/articles/1?include=comments.author</example>
        /// </param>
        public Context(Uri requestUri, string[] includedResources)
        {
            RequestUri = requestUri;
            IncludedResources = includedResources;
        }
        #endregion

        /// <summary>
        /// Requested URL. Endpoint the client pinged
        /// </summary>
        public Uri RequestUri { get; private set; }

        /// <summary>
        /// List of included resources.
        /// The following example generates two included resources: comments and author
        /// <example>http://localhost:60101/articles/1?include=comments.author</example>
        /// </summary>
        public string[] IncludedResources { get; set; }

        public Uri BaseUri
        {
            get
            {
                var authority = (UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port);
                return new Uri(RequestUri.GetComponents(authority, UriFormat.SafeUnescaped));
            }
        }
    }
}