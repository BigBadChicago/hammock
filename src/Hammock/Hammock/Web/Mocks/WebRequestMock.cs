using System;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class WebRequestMock : WebRequest
    {
        private readonly Uri _origin;
        private readonly string _response;

        public WebRequestMock(Uri origin, string response)
        {
            _origin = origin;
            _response = response;
        }

        public override WebResponse GetResponse()
        {
            var response = new WebResponseMock(_origin, _response);
            return response;
        }
    }
}