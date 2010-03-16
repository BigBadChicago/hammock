using System;
using System.IO;
using System.Net;
#if !NETCF
using System.Runtime.Serialization;
using System.Text;

#endif

namespace Hammock.Web.Mocks
{
    public class MockHttpWebResponse : WebResponse
    {
        private readonly Uri _requestUri;
        private string _contentType;

        public string Content { get; private set; }

        public override Stream GetResponseStream()
        {
            var bytes = Encoding.UTF8.GetBytes(Content);
            var stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public override void Close()
        {
            
        }

        public override long ContentLength
        {
            get { return Content.Length; }
        }

        public override string ContentType
        {
            get { return _contentType; }
        }

        public override Uri ResponseUri
        {
            get { return _requestUri; }
        }

        public MockHttpWebResponse(Uri origin, string content, string contentType)
        {
            _requestUri = origin;
            Content = content;
            _contentType = contentType;
        }
    }
}