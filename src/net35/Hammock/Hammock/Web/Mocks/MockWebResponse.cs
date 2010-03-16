using System;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockWebResponse : WebResponse
    {
        private readonly Uri _origin;
        public string Content { get; private set; }

        public override Uri ResponseUri
        {
            get { return _origin; }
        }

        public MockWebResponse(Uri origin, string content)
        {
            _origin = origin;
            Content = content;
        }
    }
}