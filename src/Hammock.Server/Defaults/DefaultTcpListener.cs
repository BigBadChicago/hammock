using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Net.Sockets;

namespace Hammock.Server.Defaults
{
    [Export(typeof(ITcpListener))]
    public class DefaultTcpListener : ITcpListener
    {
        private readonly TcpListener _listener;

        public DefaultTcpListener(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
        }

        public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
        {
            return _listener.BeginAcceptSocket(callback, state);
        }

        public Socket EndAcceptSocket(IAsyncResult asyncResult)
        {
            return _listener.EndAcceptSocket(asyncResult);
        }
    }
}
