using System;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthParameterHandling
    {
        HttpAuthorizationHeader,
        UrlOrPostParameters
    }
}