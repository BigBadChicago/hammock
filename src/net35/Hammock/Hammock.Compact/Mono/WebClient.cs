#region License

// TweetSharp
// Copyright (c) 2010 Daniel Crenna and Jason Diller
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace System.Net
{
    [ComVisible(true)]
    public partial class WebClient : Component
    {
        private const string urlEncodedCType = "application/x-www-form-urlencoded";
        private static readonly byte[] hexBytes;
        private bool async;
        private Thread async_thread;
        private Uri baseAddress;
        private string baseString;
        private ICredentials credentials;
        private Encoding encoding = Encoding.Default;
        private Mono.Net.WebHeaderCollection headers;
        private NameValueCollection queryString;

        static WebClient()
        {
            hexBytes = new byte[16];
            var index = 0;
            for (var i = (int) '0'; i <= '9'; i++, index++)
                hexBytes[index] = (byte) i;

            for (var i = (int) 'A'; i <= 'F'; i++, index++)
                hexBytes[index] = (byte) i;
        }

        public string BaseAddress
        {
            get
            {
                if (baseString == null)
                {
                    if (baseAddress == null)
                        return "";
                }

                baseString = baseAddress.ToString();
                return baseString;
            }

            set { baseAddress = string.IsNullOrEmpty(value) ? null : new Uri(value); }
        }

        public ICredentials Credentials
        {
            get { return credentials; }
            set { credentials = value; }
        }

        public Mono.Net.WebHeaderCollection Headers
        {
            get
            {
                if (headers == null)
                    headers = new Mono.Net.WebHeaderCollection();

                return headers;
            }
            set { headers = value; }
        }

        public NameValueCollection QueryString
        {
            get
            {
                if (queryString == null)
                    queryString = new NameValueCollection();

                return queryString;
            }
            set { queryString = value; }
        }

        public WebHeaderCollection ResponseHeaders { get; private set; }

        public Encoding Encoding
        {
            get { return encoding; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                encoding = value;
            }
        }

        public IWebProxy Proxy { get; set; }

        public bool IsBusy { get; private set; }

        private void CheckBusy()
        {
            if (IsBusy)
                throw new NotSupportedException("WebClient does not support concurrent I/O operations.");
        }

        private void SetBusy()
        {
            lock (this)
            {
                CheckBusy();
                IsBusy = true;
            }
        }

        //   DownloadData

        public byte[] DownloadData(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return DownloadData(CreateUri(address));
        }

        public byte[] DownloadData(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            try
            {
                SetBusy();
                async = false;
                return DownloadDataCore(address, null);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private byte[] DownloadDataCore(Uri address, object userToken)
        {
            WebRequest request = null;

            try
            {
                request = SetupRequest(address);
                var response = request.GetResponse();
                var st = ProcessResponse(response);
                return ReadAll(st, (int) response.ContentLength, userToken);
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
        }

        public void DownloadFile(string address, string fileName)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            DownloadFile(CreateUri(address), fileName);
        }

        public void DownloadFile(Uri address, string fileName)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            try
            {
                SetBusy();
                async = false;
                DownloadFileCore(address, fileName, null);
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DownloadFileCore(Uri address, string fileName, object userToken)
        {
            WebRequest request = null;

            using (var f = new FileStream(fileName, FileMode.Create))
            {
                try
                {
                    request = SetupRequest(address);
                    var response = request.GetResponse();
                    var st = ProcessResponse(response);

                    var cLength = (int) response.ContentLength;
                    var length = (cLength <= -1 || cLength > 32*1024) ? 32*1024 : cLength;
                    var buffer = new byte[length];

                    int nread;
                    long notify_total = 0;

                    while ((nread = st.Read(buffer, 0, length)) != 0)
                    {
                        if (async)
                        {
                            notify_total += nread;
                            OnDownloadProgressChanged(
                                                         new DownloadProgressChangedEventArgs(notify_total,
                                                                                              response.ContentLength,
                                                                                              userToken));
                        }
                        f.Write(buffer, 0, nread);
                    }
                }
                catch (ThreadInterruptedException)
                {
                    if (request != null)
                        request.Abort();
                    throw;
                }
            }
        }

        public Stream OpenRead(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return OpenRead(CreateUri(address));
        }

        public
            Stream OpenRead(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            WebRequest request;
            try
            {
                SetBusy();
                async = false;
                request = SetupRequest(address);
                var response = request.GetResponse();
                return ProcessResponse(response);
            }
            catch (Exception ex)
            {
                //JD - Changed this to just rethrow the web exception instead of wrapping
                //it in another exception for consistency with the native .net implementation
                if (ex is WebException)
                {
                    throw;
                }
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Stream OpenWrite(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return OpenWrite(CreateUri(address));
        }

        public Stream OpenWrite(string address, string method)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return OpenWrite(CreateUri(address), method);
        }

        public Stream OpenWrite(Uri address)
        {
            return OpenWrite(address, null);
        }

        public Stream OpenWrite(Uri address, string method)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            try
            {
                SetBusy();
                async = false;
                var request = SetupRequest(address, method, true);
                return request.GetRequestStream();
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string DetermineMethod(Uri address, string method, bool is_upload)
        {
            if (method != null)
                return method;

            if (address.Scheme == Uri.UriSchemeFtp)
                return (is_upload) ? "STOR" : "RETR";

            return (is_upload) ? "POST" : "GET";
        }

        public byte[] UploadData(string address, byte[] data)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return UploadData(CreateUri(address), data);
        }

        public byte[] UploadData(string address, string method, byte[] data)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return UploadData(CreateUri(address), method, data);
        }

        public byte[] UploadData(Uri address, byte[] data)
        {
            return UploadData(address, null, data);
        }

        public byte[] UploadData(Uri address, string method, byte[] data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            try
            {
                SetBusy();
                async = false;
                return UploadDataCore(address, method, data, null);
            }
            catch (WebException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private byte[] UploadDataCore(Uri address, string method, byte[] data, object userToken)
        {
            var request = SetupRequest(address, method, true);
            try
            {
                var contentLength = data.Length;
                request.ContentLength = contentLength;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, contentLength);
                }

                var response = request.GetResponse();
                var st = ProcessResponse(response);
                return ReadAll(st, (int) response.ContentLength, userToken);
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
        }

        //   UploadFile

        public byte[] UploadFile(string address, string fileName)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return UploadFile(CreateUri(address), fileName);
        }

        public byte[] UploadFile(Uri address, string fileName)
        {
            return UploadFile(address, null, fileName);
        }

        public byte[] UploadFile(string address, string method, string fileName)
        {
            return UploadFile(CreateUri(address), method, fileName);
        }

        public byte[] UploadFile(Uri address, string method, string fileName)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            try
            {
                SetBusy();
                async = false;
                return UploadFileCore(address, method, fileName, null);
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private byte[] UploadFileCore(Uri address, string method, string fileName, object userToken)
        {
            var fileCType = Headers["Content-Type"];
            if (fileCType != null)
            {
                var lower = fileCType.ToLower();
                if (lower.StartsWith("multipart/"))
                    throw new WebException("Content-Type cannot be set to a multipart" +
                                           " type for this request.");
            }
            else
            {
                fileCType = "application/octet-stream";
            }

            var boundary = "------------" + DateTime.Now.Ticks.ToString("x");
            Headers["Content-Type"] = String.Format("multipart/form-data; boundary={0}", boundary);
            Stream reqStream = null;
            Stream fStream = null;
            byte[] resultBytes;

            fileName = Path.GetFullPath(fileName);

            WebRequest request = null;
            try
            {
                fStream = File.OpenRead(fileName);
                request = SetupRequest(address, method, true);
                reqStream = request.GetRequestStream();
                var realBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
                reqStream.Write(realBoundary, 0, realBoundary.Length);
                var partHeaders = String.Format("Content-Disposition: form-data; " +
                                                "name=\"file\"; filename=\"{0}\"\r\n" +
                                                "Content-Type: {1}\r\n\r\n",
                                                Path.GetFileName(fileName), fileCType);

                var partHeadersBytes = Encoding.UTF8.GetBytes(partHeaders);
                reqStream.Write(partHeadersBytes, 0, partHeadersBytes.Length);
                int nread;
                var buffer = new byte[4096];
                while ((nread = fStream.Read(buffer, 0, 4096)) != 0)
                    reqStream.Write(buffer, 0, nread);

                reqStream.WriteByte((byte) '\r');
                reqStream.WriteByte((byte) '\n');
                reqStream.Write(realBoundary, 0, realBoundary.Length);
                reqStream.Close();
                reqStream = null;
                var response = request.GetResponse();
                var st = ProcessResponse(response);
                resultBytes = ReadAll(st, (int) response.ContentLength, userToken);
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
            finally
            {
                if (fStream != null)
                    fStream.Close();

                if (reqStream != null)
                    reqStream.Close();
            }

            return resultBytes;
        }

        public byte[] UploadValues(string address, NameValueCollection data)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return UploadValues(CreateUri(address), data);
        }

        public byte[] UploadValues(string address, string method, NameValueCollection data)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            return UploadValues(CreateUri(address), method, data);
        }

        public byte[] UploadValues(Uri address, NameValueCollection data)
        {
            return UploadValues(address, null, data);
        }

        public byte[] UploadValues(Uri address, string method, NameValueCollection data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            try
            {
                SetBusy();
                async = false;
                return UploadValuesCore(address, method, data, null);
            }
            catch (Exception ex)
            {
                throw new WebException("An error occurred " +
                                       "performing a WebClient request.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private byte[] UploadValuesCore(Uri uri, string method, NameValueCollection data, object userToken)
        {
            var cType = Headers["Content-Type"];
            if (cType != null && String.Compare(cType, urlEncodedCType, true) != 0)
                throw new WebException("Content-Type header cannot be changed from its default " +
                                       "value for this request.");

            Headers["Content-Type"] = urlEncodedCType;
            var request = SetupRequest(uri, method, true);
            try
            {
                var rqStream = request.GetRequestStream();
                var tmpStream = new MemoryStream();
                foreach (string key in data)
                {
                    var bytes = Encoding.ASCII.GetBytes(key);
                    UrlEncodeAndWrite(tmpStream, bytes);
                    tmpStream.WriteByte((byte) '=');
                    bytes = Encoding.ASCII.GetBytes(data[key]);
                    UrlEncodeAndWrite(tmpStream, bytes);
                    tmpStream.WriteByte((byte) '&');
                }

                var length = (int) tmpStream.Length;
                if (length > 0)
                    tmpStream.SetLength(--length); // remove trailing '&'

                var buf = tmpStream.GetBuffer();
                rqStream.Write(buf, 0, length);
                rqStream.Close();
                tmpStream.Close();

                var response = request.GetResponse();
                var st = ProcessResponse(response);
                return ReadAll(st, (int) response.ContentLength, userToken);
            }
            catch (ThreadInterruptedException)
            {
                request.Abort();
                throw;
            }
        }

        public string DownloadString(string address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var bytes = DownloadData(CreateUri(address));
            return encoding.GetString(bytes, 0, bytes.Length);
        }

        public string DownloadString(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            var bytes = DownloadData(CreateUri(address));
            return encoding.GetString(bytes, 0, bytes.Length);
        }

        public string UploadString(string address, string data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            var resp = UploadData(address, encoding.GetBytes(data));
            return encoding.GetString(resp, 0, resp.Length);
        }

        public string UploadString(string address, string method, string data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            var resp = UploadData(address, method, encoding.GetBytes(data));
            return encoding.GetString(resp, 0, resp.Length);
        }

        public string UploadString(Uri address, string data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            var resp = UploadData(address, encoding.GetBytes(data));
            return encoding.GetString(resp, 0, resp.Length);
        }

        public string UploadString(Uri address, string method, string data)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (data == null)
                throw new ArgumentNullException("data");

            var resp = UploadData(address, method, encoding.GetBytes(data));
            return encoding.GetString(resp, 0, resp.Length);
        }

        public event DownloadDataCompletedEventHandler DownloadDataCompleted;
        public event AsyncCompletedEventHandler DownloadFileCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event DownloadStringCompletedEventHandler DownloadStringCompleted;
        public event OpenReadCompletedEventHandler OpenReadCompleted;
        public event OpenWriteCompletedEventHandler OpenWriteCompleted;
        public event UploadDataCompletedEventHandler UploadDataCompleted;
        public event UploadFileCompletedEventHandler UploadFileCompleted;
        public event UploadProgressChangedEventHandler UploadProgressChanged;
        public event UploadStringCompletedEventHandler UploadStringCompleted;
        public event UploadValuesCompletedEventHandler UploadValuesCompleted;

        private Uri CreateUri(string address)
        {
            return MakeUri(address);
        }

        private Uri CreateUri(Uri address)
        {
            var query = address.Query;
            if (String.IsNullOrEmpty(query))
                query = GetQueryString(true);

            if (baseAddress == null && query == null)
                return address;

            if (baseAddress == null)
                return new Uri(address + query);

            return query == null ? new Uri(baseAddress, address.ToString()) : new Uri(baseAddress, address + query);
        }

        private string GetQueryString(bool add_qmark)
        {
            if (queryString == null || queryString.Count == 0)
                return null;

            var sb = new StringBuilder();
            if (add_qmark)
                sb.Append('?');

            foreach (string key in queryString)
                sb.AppendFormat("{0}={1}&", key, UrlEncode(queryString[key]));

            if (sb.Length != 0)
                sb.Length--; // removes last '&' or the '?' if empty.

            if (sb.Length == 0)
                return null;

            return sb.ToString();
        }

        private Uri MakeUri(string path)
        {
            var query = GetQueryString(true);
            if (baseAddress == null && query == null)
            {
                try
                {
                    return new Uri(path);
                }
                catch (ArgumentNullException)
                {
                    path = Path.GetFullPath(path);
                    return new Uri("file://" + path);
                }
                catch (UriFormatException)
                {
                    path = Path.GetFullPath(path);
                    return new Uri("file://" + path);
                }
            }

            if (baseAddress == null)
            {
                return new Uri(path + query);
            }

            return query == null ? new Uri(baseAddress, path) : new Uri(baseAddress, path + query);
        }

        private WebRequest SetupRequest(Uri uri)
        {
            var request = GetWebRequest(uri);
            if (Proxy != null)
                request.Proxy = Proxy;

            request.Credentials = credentials;

            // Special headers. These are properties of HttpWebRequest.
            // What do we do with other requests different from HttpWebRequest?
            if (headers != null && headers.Count != 0 && (request is HttpWebRequest))
            {
                var req = (HttpWebRequest) request;
                var expect = headers["Expect"];
                var contentType = headers["Content-Type"];
                var accept = headers["Accept"];
                var connection = headers["Connection"];
                var userAgent = headers["User-Agent"];
                var referer = headers["Referer"];

                headers.RemoveInternal("Expect");
                headers.RemoveInternal("Content-Type");
                headers.RemoveInternal("Accept");
                headers.RemoveInternal("Connection");
                headers.RemoveInternal("Referer");
                headers.RemoveInternal("User-Agent");

                request.Headers = new WebHeaderCollection();
                foreach (NameValueCollection header in headers)
                {
                    request.Headers.Add(header);
                }

                if (!string.IsNullOrEmpty(expect))
                    req.Expect = expect;

                if (!string.IsNullOrEmpty(accept))
                    req.Accept = accept;

                if (!string.IsNullOrEmpty(contentType))
                    req.ContentType = contentType;

                if (!string.IsNullOrEmpty(connection))
                    req.Connection = connection;

                if (!string.IsNullOrEmpty(userAgent))
                    req.UserAgent = userAgent;

                if (!string.IsNullOrEmpty(referer))
                    req.Referer = referer;
            }

            ResponseHeaders = null;
            return request;
        }

        private WebRequest SetupRequest(Uri uri, string method, bool is_upload)
        {
            var request = SetupRequest(uri);
            request.Method = DetermineMethod(uri, method, is_upload);
            return request;
        }

        private Stream ProcessResponse(WebResponse response)
        {
            ResponseHeaders = response.Headers;
            return response.GetResponseStream();
        }

        private byte[] ReadAll(Stream stream, int length, object userToken)
        {
            MemoryStream ms = null;

            var nolength = (length == -1);
            var size = ((nolength) ? 8192 : length);
            if (nolength)
                ms = new MemoryStream();

            //			long total = 0;
            int nread;
            var offset = 0;
            var buffer = new byte[size];
            while ((nread = stream.Read(buffer, offset, size)) != 0)
            {
                if (nolength)
                {
                    ms.Write(buffer, 0, nread);
                }
                else
                {
                    offset += nread;
                    size -= nread;
                }

                if (async)
                {
//					total += nread;
                    OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(nread, length, userToken));
                }
            }

            return nolength ? ms.ToArray() : buffer;
        }

        private static string UrlEncode(string str)
        {
            var result = new StringBuilder();

            var len = str.Length;
            for (var i = 0; i < len; i++)
            {
                var c = str[i];
                if (c == ' ')
                    result.Append('+');
                else if ((c < '0' && c != '-' && c != '.') ||
                         (c < 'A' && c > '9') ||
                         (c > 'Z' && c < 'a' && c != '_') ||
                         (c > 'z'))
                {
                    result.Append('%');
                    var idx = c >> 4;
                    result.Append((char) hexBytes[idx]);
                    idx = c & 0x0F;
                    result.Append((char) hexBytes[idx]);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private static void UrlEncodeAndWrite(Stream stream, byte[] bytes)
        {
            if (bytes == null)
                return;

            var len = bytes.Length;
            if (len == 0)
                return;

            for (var i = 0; i < len; i++)
            {
                var c = (char) bytes[i];
                if (c == ' ')
                    stream.WriteByte((byte) '+');
                else if ((c < '0' && c != '-' && c != '.') ||
                         (c < 'A' && c > '9') ||
                         (c > 'Z' && c < 'a' && c != '_') ||
                         (c > 'z'))
                {
                    stream.WriteByte((byte) '%');
                    var idx = c >> 4;
                    stream.WriteByte(hexBytes[idx]);
                    idx = c & 0x0F;
                    stream.WriteByte(hexBytes[idx]);
                }
                else
                {
                    stream.WriteByte((byte) c);
                }
            }
        }

        protected virtual void OnDownloadDataCompleted(DownloadDataCompletedEventArgs args)
        {
            CompleteAsync();
            if (DownloadDataCompleted != null)
                DownloadDataCompleted(this, args);
        }

        protected virtual void OnDownloadFileCompleted(AsyncCompletedEventArgs args)
        {
            CompleteAsync();
            if (DownloadFileCompleted != null)
                DownloadFileCompleted(this, args);
        }

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
        {
            if (DownloadProgressChanged != null)
                DownloadProgressChanged(this, e);
        }

        protected virtual void OnDownloadStringCompleted(DownloadStringCompletedEventArgs args)
        {
            CompleteAsync();
            if (DownloadStringCompleted != null)
                DownloadStringCompleted(this, args);
        }

        protected virtual void OnOpenReadCompleted(OpenReadCompletedEventArgs args)
        {
            CompleteAsync();
            if (OpenReadCompleted != null)
                OpenReadCompleted(this, args);
        }

        protected virtual void OnOpenWriteCompleted(OpenWriteCompletedEventArgs args)
        {
            CompleteAsync();
            if (OpenWriteCompleted != null)
                OpenWriteCompleted(this, args);
        }

        protected virtual void OnUploadDataCompleted(UploadDataCompletedEventArgs args)
        {
            CompleteAsync();
            if (UploadDataCompleted != null)
                UploadDataCompleted(this, args);
        }

        protected virtual void OnUploadFileCompleted(UploadFileCompletedEventArgs args)
        {
            CompleteAsync();
            if (UploadFileCompleted != null)
                UploadFileCompleted(this, args);
        }

        protected virtual void OnUploadProgressChanged(UploadProgressChangedEventArgs e)
        {
            if (UploadProgressChanged != null)
                UploadProgressChanged(this, e);
        }

        protected virtual void OnUploadStringCompleted(UploadStringCompletedEventArgs args)
        {
            CompleteAsync();
            if (UploadStringCompleted != null)
                UploadStringCompleted(this, args);
        }

        protected virtual void OnUploadValuesCompleted(UploadValuesCompletedEventArgs args)
        {
            CompleteAsync();
            if (UploadValuesCompleted != null)
                UploadValuesCompleted(this, args);
        }

        protected virtual WebRequest GetWebRequest(Uri address)
        {
            return WebRequest.Create(address);
        }

        protected virtual WebResponse GetWebResponse(WebRequest request)
        {
            return request.GetResponse();
        }

        protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            return request.EndGetResponse(result);
        }

        public void UploadFileAsync(Uri address, string method, string fileName)
        {
            UploadFileAsync(address, method, fileName, null);
        }
    }
}