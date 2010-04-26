using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Silverlight.GZip;
using ICSharpCode.SharpZipLib.Silverlight.Zip;

namespace Hammock.Silverlight.Compat
{
    public class GzipHttpWebResponse : WebResponse
    {
        private const int ChunkSize = 2048;

        private readonly HttpWebResponse _response;

        public GzipHttpWebResponse(HttpWebResponse response)
        {
            _response = response;
        }

        public override void Close()
        {
            _response.Close();
        }

        public override Stream GetResponseStream()
        {
            Stream compressed = null;
            if (_response.Headers["Accept-Encoding"].Contains("gzip"))
            {
                compressed = new GZipInputStream(_response.GetResponseStream());
            }
            else if (_response.Headers["Accept-Encoding"].Contains("deflate"))
            {
                compressed = new ZipInputStream(_response.GetResponseStream());
            }
            if (compressed != null)
            {
                var decompressed = new MemoryStream();
                var size = ChunkSize;
                var buffer = new byte[ChunkSize];
                while (true)
                {
                    size = compressed.Read(buffer, 0, size);
                    if (size > 0)
                    {
                        decompressed.Write(buffer, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                decompressed.Seek(0, SeekOrigin.Begin);
                return decompressed;
            }

            return _response.GetResponseStream();
        }

        public override long ContentLength
        {
            get { return _response.ContentLength; }
        }

        public override string ContentType
        {
            get { return _response.ContentType; }
        }

        public override WebHeaderCollection Headers
        {
            get { return _response.Headers; }
        }

        public override System.Uri ResponseUri
        {
            get { return _response.ResponseUri; }
        }
    }
}
