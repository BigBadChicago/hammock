using System;
using System.Collections.Generic;

namespace Hammock.Server
{
    public interface IHttpContext : IDisposable 
    {
        IDictionary<object, object> Items { get; }
        IHttpContext Current { get; }
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
    }
}