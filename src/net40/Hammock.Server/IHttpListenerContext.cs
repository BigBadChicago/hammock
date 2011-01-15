using System;
using System.Security.Principal;

namespace Hammock.Server
{
    public interface IHttpListenerContext : IDisposable
    {
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
        IPrincipal User { get; }
    }
}