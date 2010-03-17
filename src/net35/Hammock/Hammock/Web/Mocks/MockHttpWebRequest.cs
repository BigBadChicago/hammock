using System;
using System.IO;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockHttpWebRequest : WebRequest
    {
        private readonly Uri _requestUri;

        public virtual HttpStatusCode ExpectStatusCode { get; protected internal set; }
        public virtual string ExpectStatusDescription { get; protected internal set; }
        public virtual System.Net.WebHeaderCollection ExpectHeaders { get; protected internal set; }

        public virtual string Content { get; set; }

#if !SILVERLIGHT
        public override long ContentLength { get; set; }
#else
        public long ContentLength { get; set; }
#endif
        public override string ContentType { get; set; }

        public MockHttpWebRequest(Uri requestUri)
        {
            _requestUri = requestUri;
            Headers = new System.Net.WebHeaderCollection();
            ExpectHeaders = new System.Net.WebHeaderCollection();
        }

#if !SILVERLIGHT
        public override WebResponse GetResponse()
        {
            var response = new MockHttpWebResponse(_requestUri, ContentType)
                               {
                                   StatusCode = ExpectStatusCode,
                                   StatusDescription = ExpectStatusDescription,
                                   Content = Content
                               };
            foreach(var key in ExpectHeaders.AllKeys)
            {
                response.Headers.Add(key, ExpectHeaders[key]);
            }
            return response;
        }
#endif      
        public override void Abort()
        {
            
        }

#if !SILVERLIGHT
        public override Stream GetRequestStream()
        {
            return new MemoryStream();
        }
#endif
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

        public override System.Net.WebHeaderCollection Headers { get; set; }
        public override string Method { get; set; }

        public override Uri RequestUri
        {
            get { return _requestUri; }
        }
    }
}