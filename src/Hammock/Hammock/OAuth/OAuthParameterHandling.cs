using System;

namespace Hammock.OAuth
{
#if !SILVERLIGHT
    /// <summary>
    /// Methods of including OAuth parameters in a signed request.
    /// </summary>
    /// <seealso cref="http://oauth.net/core/1.0#signing_process"/>
    [Serializable]
#endif
    public enum OAuthParameterHandling
    {
        /// <summary>
        /// OAuth parameters are served in the HTTP Authorization header.
        /// </summary>
        HttpAuthorizationHeader,
        /// <summary>
        /// OAuth parameters are part of the URI, if using GET, or sent
        /// as POST parameters, if using POST.
        /// </summary>
        UrlOrPostParameters
    }
}