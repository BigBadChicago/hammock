using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Hammock.Server.Defaults
{
    public class HttpListener : IHttpListener
    {
        //private ITcpListener _listener;
        private X509Certificate _certificate;

        public HttpListener(IPAddress address, int port)
        {
            //_listener = new DefaultTcpListener(address, port);
        }

        public HttpListener(IPAddress address, int port, X509Certificate certificate) : this(address, port)
        {
            _certificate = certificate;
        }

        public virtual void Dispose()
        {
            
        }

        public virtual IAsyncResult BeginGetContext(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public virtual IHttpListenerContext EndGetContext(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}