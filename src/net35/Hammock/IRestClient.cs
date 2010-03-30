using System;

namespace Hammock
{
    public interface IRestClient
    {
#if !Silverlight
        RestResponse Request(RestRequest request);
        RestResponse Request();
        
        RestResponse<T> Request<T>(RestRequest request);
        RestResponse<T> Request<T>();
#endif
        IAsyncResult BeginRequest(RestRequest request, RestCallback callback);
        IAsyncResult BeginRequest<T>(RestRequest request, RestCallback<T> callback);
        IAsyncResult BeginRequest(RestCallback callback);

        IAsyncResult BeginRequest(RestRequest request);
        IAsyncResult BeginRequest<T>(RestRequest request);
        IAsyncResult BeginRequest<T>(RestCallback<T> callback);

        RestResponse EndRequest(IAsyncResult result);
        RestResponse<T> EndRequest<T>(IAsyncResult result);
    }
}