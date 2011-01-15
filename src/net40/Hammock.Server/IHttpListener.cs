using System;

namespace Hammock.Server
{
    public interface IHttpListener : IDisposable
    {
        IAsyncResult BeginGetContext(AsyncCallback callback, object state);
        IHttpListenerContext EndGetContext(IAsyncResult asyncResult);
    }
}