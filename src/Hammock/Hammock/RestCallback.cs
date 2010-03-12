using System;

namespace Hammock
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public delegate void RestCallback(RestRequest request, RestResponse response);

#if !SILVERLIGHT
    [Serializable]
#endif     
    public delegate void RestCallback<T>(RestRequest request, RestResponse<T> response);
}