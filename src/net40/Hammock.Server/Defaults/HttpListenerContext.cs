using System;
using System.Security.Principal;

namespace Hammock.Server.Defaults
{
    public class HttpListenerContext : IHttpListenerContext
    {
        public virtual IHttpRequest Request
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IHttpResponse Response
        {
            get { throw new NotImplementedException(); }
        }

        public virtual IPrincipal User
        {
            get { throw new NotImplementedException(); }
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}