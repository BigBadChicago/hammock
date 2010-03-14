using System;
using System.Runtime.Serialization;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthParameterHandling
    {
#if !SILVERLIGHT && !Smartphone
        [EnumMember] HttpAuthorizationHeader,
        [EnumMember] UrlOrPostParameters
#else
        HttpAuthorizationHeader,
        UrlOrPostParameters
#endif
    }
}