using System;
using System.Collections.Specialized;

namespace Hammock.Server
{
    public interface IHttpRequest : IDisposable 
    {
        NameValueCollection Headers { get; }
    }
}