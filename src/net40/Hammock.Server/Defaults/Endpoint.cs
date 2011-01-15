using System;
using System.Net;
using System.Net.Sockets;

namespace Hammock.Server.Defaults
{
    public class Endpoint : IEndpoint
    {
        private readonly Socket _socket;

        private Endpoint(Socket socket)
        {
            _socket = socket;
        }

        public static IEndpoint Create(IAddress address, int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("port");
            }

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(new IPAddress(address.Value), port));
            socket.Listen(2147483647);
            return new Endpoint(socket);
        }

        public IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _socket.BeginSend(buffer, offset, count, SocketFlags.None, callback, state);
        }

        public IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _socket.BeginReceive(buffer, offset, count, SocketFlags.None, callback, state);
        }

        public int EndSend(IAsyncResult result)
        {
            SocketError error;
            return _socket.EndSend(result, out error);
        }

        public int EndReceive(IAsyncResult result)
        {
            SocketError error;
            return _socket.EndReceive(result, out error);
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return _socket.BeginAccept(callback, state);
        }

        public IEndpoint EndAccept(IAsyncResult result)
        {
            return new Endpoint(_socket.EndAccept(result));
        }

        public IEndpoint Accept()
        {
            return new Endpoint(_socket.Accept());
        }

        public void Close()
        {
            if(_socket != null)
            {
                _socket.Close();
            }
        }

        public void Dispose()
        {
#if NET40
            if(_socket != null)
            {
                _socket.Dispose();
            }
#else
            if(_socket != null)
            {
                (_socket as IDisposable).Dispose();
            }
#endif
        }
    }
}