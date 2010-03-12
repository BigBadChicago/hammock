using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Hammock.Extensions;
using Hammock.Model;

namespace Hammock.Web.Mocks
{
    internal class WebQueryClientMock : IWebQueryClient
    {
        private readonly IEnumerable<IMockable> _graph;

        public WebQueryClientMock(IEnumerable<IMockable> graph)
        {
            _graph = graph;
        }

        public WebFormat WebFormat { get; set; }
        public ICredentials Credentials { get; set; }

        #region IWebQueryClient Members

        public WebResponse Response { get; private set; }
        public WebRequest Request { get; private set; }
        public WebCredentials WebCredentials { get; set; }
        public WebException Exception { get; set; }
               
        public string SourceUrl { get; set; }
        public bool UseCompression { get; set; }
        public TimeSpan? RequestTimeout { get; set; }
        public string ProxyValue { get; set; }

        public void SetWebProxy(WebRequest request)
        {
            // No-op
        }

        public WebRequest GetWebRequestShim(Uri address)
        {
            var request = new WebRequestMock(address, _graph.ToJson());
            Request = request;

            return request;
        }

        public WebResponse GetWebResponseShim(WebRequest request)
        {
            var response = new WebResponseMock(request.RequestUri, _graph.ToJson());
            Response = response;

            return response;
        }

        public WebResponse GetWebResponseShim(WebRequest request, IAsyncResult result)
        {
            var response = new WebResponseMock(request.RequestUri, _graph.ToJson());
            Response = response;

            return response;
        }

        public event OpenReadCompletedEventHandler OpenReadCompleted;

        public void OpenReadAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public void OpenReadAsync(Uri uri, object state)
        {
            throw new NotImplementedException();
        }

        public void CancelAsync()
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(string url)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_graph.ToJson()));
        }

        public bool KeepAlive { get; set; }

        #endregion

        protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs e)
        {
            if (OpenReadCompleted != null)
            {
                OpenReadCompleted(this, e);
            }
        }
    }
}