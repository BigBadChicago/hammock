using System;
using System.ComponentModel.Composition;
using System.Security.Principal;

namespace Hammock.Server.Defaults
{
    [Export(typeof(IHttpListenerContext))]
    public class DefaultHttpListenerContext : IHttpListenerContext
    {
        public IHttpRequest Request
        {
            get { throw new NotImplementedException(); }
        }

        public IHttpResponse Response
        {
            get { throw new NotImplementedException(); }
        }

        public IPrincipal User
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}