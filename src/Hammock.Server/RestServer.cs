using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Hammock.Server.Defaults;

namespace Hammock.Server
{
    public class RestServer : IHttpServer
    {
        private IHttpListener _listener;

        public ICollection<IHttpModule> Modules { get; private set; }

        public RestServer()
        {
            Modules = new List<IHttpModule>(0);
        }

        public virtual void Start(IPAddress address)
        {
            _listener = new DefaultHttpListener(address, 80);
        }

        public virtual void Start(IPAddress address, int port)
        {
            _listener = new DefaultHttpListener(address, port);
        }

        public virtual void Start(IPAddress address, X509Certificate certificate)
        {
            _listener = new DefaultHttpListener(address, 443, certificate);
        }

        public virtual void Start(IPAddress address, int port, X509Certificate certificate)
        {
            _listener = new DefaultHttpListener(address, port, certificate);
        }

        public virtual void Dispose()
        {
            _listener.Dispose();
        }
    }
}
