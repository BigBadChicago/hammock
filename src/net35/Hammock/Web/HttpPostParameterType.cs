using System;
using System.Runtime.Serialization;

namespace Hammock.Web
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum HttpPostParameterType
    {
#if !SILVERLIGHT && !Smartphone && !ClientProfiles
        [EnumMember] Field,
        [EnumMember] File
#else
        Field,
        File
#endif
    }
}