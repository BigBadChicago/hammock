using System;
using System.Runtime.Serialization;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthType
    {
        [EnumMember] RequestToken,
        [EnumMember] AccessToken,
        [EnumMember] ProtectedResource,
        [EnumMember] ClientAuthentication
    }
}