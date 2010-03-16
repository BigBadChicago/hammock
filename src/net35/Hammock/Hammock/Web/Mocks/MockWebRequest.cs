using System;
using System.IO;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockWebRequest : WebRequest
    {
        private readonly Uri _origin;
        private readonly string _response;

        public MockWebRequest(Uri origin, string response)
        {
            _origin = origin;
            _response = response;
        }
#if !SILVERLIGHT
        public override WebResponse GetResponse()
        {
            var response = new MockWebResponse(_origin, _response);
            return response;
        }
#endif

        // Asynchronous overrides
        public override void Abort()
        {
            throw new NotImplementedException();
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

        public override string ContentType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override System.Net.WebHeaderCollection Headers
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override string Method
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override Uri RequestUri
        {
            get { throw new NotImplementedException(); }
        }
    }
}