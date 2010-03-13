using System;
using System.Runtime.Serialization;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthType
    {
#if !SILVERLIGHT && !Smartphone
        [EnumMember] RequestToken,
        [EnumMember] AccessToken,
        [EnumMember] ProtectedResource,
        [EnumMember] ClientAuthentication
#else
        RequestToken,
        AccessToken,
        ProtectedResource,
        ClientAuthentication
#endif
    }
}