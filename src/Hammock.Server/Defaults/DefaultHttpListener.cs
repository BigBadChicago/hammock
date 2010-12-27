using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Hammock.Server.Defaults
{
    [Export(typeof(IHttpListener))]
    public class DefaultHttpListener : IHttpListener
    {
        private ITcpListener _listener;
        private X509Certificate _certificate;

        public DefaultHttpListener(IPAddress address, int port)
        {
            _listener = new DefaultTcpListener(address, port);
        }

        public DefaultHttpListener(IPAddress address, int port, X509Certificate certificate) : this(address, port)
        {
            _certificate = certificate;
        }

        public virtual void Dispose()
        {
            
        }

        public IAsyncResult BeginGetContext(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IHttpListenerContext EndGetContext(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
    }
}