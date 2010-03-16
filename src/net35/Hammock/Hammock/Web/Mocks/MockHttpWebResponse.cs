using System;
using System.IO;
using System.Net;
using System.Text;
using Hammock.Extensions;

#if !NETCF
using System.Text;
#endif

namespace Hammock.Web.Mocks
{
    public class MockHttpWebResponse : WebResponse
    {
        private readonly Uri _requestUri;
        private readonly string _contentType;

        public virtual string Content { get; protected internal set; }
        public virtual HttpStatusCode StatusCode { get; protected internal set; }
        public virtual string StatusDescription { get; protected internal set; }
        public virtual new System.Net.WebHeaderCollection Headers { get; private set; }

        public override Stream GetResponseStream()
        {
            if(Content.IsNullOrBlank())
            {
                return new MemoryStream();
            }
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
            get { return Content != null ? Content.Length : 0; }
        }

        public override string ContentType
        {
            get { return _contentType; }
        }

        public override Uri ResponseUri
        {
            get { return _requestUri; }
        }

        public MockHttpWebResponse(Uri requestUri, 
                                   string contentType)
        {
            _requestUri = requestUri;
            _contentType = contentType;

            Headers = new System.Net.WebHeaderCollection();
        }
    }
}