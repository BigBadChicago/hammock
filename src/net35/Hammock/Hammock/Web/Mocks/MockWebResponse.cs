using System;
using System.IO;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockWebResponse : WebResponse
    {
        private readonly Uri _origin;
        public string Content { get; private set; }

        public override Stream GetResponseStream()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override long ContentLength
        {
            get { throw new NotImplementedException(); }
        }

        public override string ContentType
        {
            get { throw new NotImplementedException(); }
        }

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