using System;
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
    }
}