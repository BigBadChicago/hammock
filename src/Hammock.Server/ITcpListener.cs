using System;
using System.Net.Sockets;

namespace Hammock.Server
{
    public interface ITcpListener
    {
        IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state);
        Socket EndAcceptSocket(IAsyncResult asyncResult);
    }
}