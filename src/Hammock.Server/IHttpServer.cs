using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Hammock.Server
{
    public interface IHttpServer : IDisposable 
    {
        ICollection<IHttpModule> Modules { get; }

        void Start(IPAddress address);
        void Start(IPAddress address, int port);
        void Start(IPAddress address, X509Certificate certificate);
        void Start(IPAddress address, int port, X509Certificate certificate);
    }
}