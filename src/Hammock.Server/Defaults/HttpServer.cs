using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Hammock.Server.Defaults
{
    public class HttpServer : IHttpServer
    {
        private const int Delay = 500;
        private IEndpoint _endpoint;
        private bool _stopping;
        private int _backlog;

        public void Dispose()
        {
            if (_endpoint == null)
            {
                return;
            }
            _endpoint.Close();
            _endpoint.Dispose();
        }

        public ICollection<IHttpModule> Modules
        {
            get { throw new NotImplementedException(); }
        }

        public void Start(IAddress address)
        {
            Start(address, 80);
        }

        public void Start(IAddress address, int port)
        {
            _stopping = false;
            _endpoint = Endpoint.Create(address, port);

            new Action(EventLoop).BeginInvoke(null, null);
        }

        private void EventLoop()
        {
            while (!_stopping)
            {
                try
                {
                    new Action(ConnectionHandler)
                        .BeginInvoke(null, null);
                }
                catch
                {
                    Thread.Sleep(Delay);
                }
            }

            while (true)
            {
                if (_backlog <= 0)
                {
                    break;
                }

                Thread.Sleep(Delay);
            }
        }

        private void ConnectionHandler()
        {
            var endpoint = _endpoint.Accept();
            HandleConnection(endpoint);

            /* When on a background thread, another async primitive is twice as slow
             return _endpoint.BeginAccept(
                ar =>
                    {
                        var endpoint = _endpoint.EndAccept(ar);
                        HandleConnection(endpoint);
                    }
                , null); 
             */
        }

        private void HandleConnection(IEndpoint endpoint)
        {
            if (!_stopping)
            {
                WithConnection(new HttpConnection { Endpoint = endpoint });
            }
        }

        public void Start(IAddress address, X509Certificate certificate)
        {
            throw new NotImplementedException();
        }

        public void Start(IAddress address, int port, X509Certificate certificate)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            _stopping = true;
        }

        public void WithConnection(IHttpConnection connection)
        {
            WithPendingLock(
                ()=>
                    {
                        var request = new byte[64];

                        connection.Endpoint.BeginReceive(request, 0, request.Length,
                                                         ar =>
                                                             {
                                                                 SendOK(connection);
                                                             }, null);
                    }
                );
        }

        private void SendOK(IHttpConnection connection)
        {
            var response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");

            connection.Endpoint.BeginSend(response, 0, response.Length,
                                          ar =>
                                              {
                                                  //var read = connection.Endpoint.EndSend(ar);
                                                  connection.Endpoint.Close();
                                              }, null);
        }

        private int _peak;

        public int GetPeak()
        {
            return _peak;
        }

        public void WithPendingLock(Action action)
        {
            Interlocked.Increment(ref _backlog);
            if(_peak < _backlog)
            {
                _peak = _backlog;
            }

            try
            {
                action.Invoke();
            }
            finally
            {
                Interlocked.Decrement(ref _backlog);
            }
        }
    }
}
