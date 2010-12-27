using System;

namespace Hammock.Server
{
    public interface IHttpModule : IDisposable
    {
        IAsyncResult BeginProcess(IHttpContext context);
        IHttpContext EndProcess(IAsyncResult result);
    }
}