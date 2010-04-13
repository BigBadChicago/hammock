using System;
using System.Runtime.Serialization;

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum OAuthSignatureMethod
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles
        [EnumMember] PlainText,
        [EnumMember] HmacSha1,
        [EnumMember] RsaSha1
#else
        PlainText,
        HmacSha1,
        RsaSha1
#endif
    }
}