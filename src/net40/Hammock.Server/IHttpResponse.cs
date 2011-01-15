using System;
using System.Collections.Specialized;

namespace Hammock.Server
{
    public interface IHttpResponse : IDisposable 
    {
        NameValueCollection Headers { get; }
    }
}