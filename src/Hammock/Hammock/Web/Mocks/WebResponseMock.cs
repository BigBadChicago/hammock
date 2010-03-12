using System;
using System.IO;
using System.Net;
using System.Text;

namespace Hammock.Web.Mocks
{
    public class WebResponseMock : WebResponse
    {
        private readonly Uri _origin;
        public string Content { get; private set; }

        public override Uri ResponseUri
        {
            get { return _origin; }
        }

        public WebResponseMock(Uri origin, string content)
        {
            _origin = origin;
            Content = content;
        }
    }
}