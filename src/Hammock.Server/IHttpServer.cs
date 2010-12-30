using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Hammock.Server
{
    public interface IHttpServer : IDisposable 
    {
        ICollection<IHttpModule> Modules { get; }

        void Start(IAddress address);
        void Start(IAddress address, int port);
        void Start(IAddress address, X509Certificate certificate);
        void Start(IAddress address, int port, X509Certificate certificate);

        void Stop();
        
        void WithConnection(IHttpConnection connection);
    }
}