using System;
using System.IO;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockHttpWebRequest : WebRequest
    {
        private readonly string _content;
        private readonly string _contentType;
        private readonly Uri _requestUri;

        public string ExpectStatusCode { get; set; }
        public string ExpectStatusDescription { get; set; }
        public string ExpectContent { get; set; }
        public string ExpectContentType { get; set; }

        public MockHttpWebRequest(Uri requestUri)
        {
            _requestUri = requestUri;
        }

#if !SILVERLIGHT
        public override WebResponse GetResponse()
        {
            var response = new MockHttpWebResponse(_requestUri, _content, _contentType);
            return response;
        }
#endif      
        public override void Abort()
        {
            
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override string ContentType { get; set; }
        public override System.Net.WebHeaderCollection Headers { get; set; }
        public override string Method { get; set; }

        public override Uri RequestUri
        {
            get { return _requestUri; }
        }
    }
}