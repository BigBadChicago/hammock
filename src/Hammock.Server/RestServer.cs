using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Hammock.Server
{
    public class RestServer : IHttpServer
    {
        private readonly IHttpServer _server;

        public ICollection<IHttpModule> Modules { get; private set; }

        public RestServer()
        {
            _server = new Defaults.HttpServer();
            Modules = new List<IHttpModule>(0);
        }

        public virtual void Start(IAddress address)
        {
            _server.Start(address, 80);
        }

        public virtual void Start(IAddress address, int port)
        {
            _server.Start(address, port);
        }

        public virtual void Start(IAddress address, X509Certificate certificate)
        {
            _server.Start(address, 443, certificate);
        }

        public virtual void Start(IAddress address, int port, X509Certificate certificate)
        {
            _server.Start(address, port, certificate);
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void WithConnection(IHttpConnection connection)
        {
            _server.WithConnection(connection);
        }

        public virtual void Dispose()
        {
            _server.Dispose();
        }
    }
}
