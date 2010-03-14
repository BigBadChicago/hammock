using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Hammock.Attributes.Specialized;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Validation;
using Hammock.Web.Mocks;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Web
{
    public abstract partial class WebQuery
    {
        private static readonly object _sync = new object();

        public virtual IWebQueryInfo Info { get; protected set; }
        public virtual string UserAgent { get; protected internal set; }
        public virtual WebHeaderCollection Headers { get; protected set; }
        public virtual WebParameterCollection Parameters { get; protected set; }
        protected internal virtual WebEntity Entity { get; set; }
        public virtual WebMethod Method { get; set; }
        public virtual string Proxy { get; set; }
        public virtual bool MockWebQueryClient { get; set; }
        public virtual string AuthorizationHeader { get; protected set; }
        public virtual IEnumerable<IMockable> MockGraph { get; set; }
        public DecompressionMethods DecompressionMethods { get; set; }
        public virtual bool UseTransparentProxy { get; set; }
        public virtual TimeSpan? RequestTimeout { get; set; }
        public virtual WebQueryResult Result { get; internal set; }
        public virtual bool KeepAlive { get; set; }
        public virtual string SourceUrl { get; set; }
        
#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
#endif

        private WebResponse _webResponse;
        public virtual WebResponse WebResponse
        {
            get
            {
                lock (_sync)
                {
                    return _webResponse;
                }
            }
            set
            {
                lock (_sync)
                {
                    _webResponse = value;
                }
            }
        }

        protected WebQuery(IWebQueryInfo info)
        {
            SetQueryMeta(info);
            InitializeResult();
        }

        private void SetQueryMeta(IWebQueryInfo info)
        {
            if(info == null)
            {
                Headers = new WebHeaderCollection(0);
                Parameters = new WebParameterCollection(0);
                return;
            }

            Info = info;
            IEnumerable<PropertyInfo> properties;
            IDictionary<string, string> transforms;

            ParseTransforms(out properties, out transforms);
            Headers = ParseInfoHeaders(properties, transforms);
            Parameters = ParseInfoParameters(properties, transforms);
            ParseUserAgent(properties);
            ParseWebEntity(properties);
        }

        private void ParseTransforms(out IEnumerable<PropertyInfo> properties, 
                                     out IDictionary<string, string> transforms)
        {
            properties = Info.GetType().GetProperties();
            transforms = new Dictionary<string, string>(0);
            Info.ParseValidationAttributes(properties, transforms);
        }

        private void InitializeResult()
        {
            Result = new WebQueryResult();
            QueryRequest += (s, e) => SetRequestResults(e);
            QueryResponse += (s, e) => SetResponseResults(e);
        }

        private void SetResponseResults(WebQueryResponseEventArgs e)
        {
            Result.ResponseDate = DateTime.UtcNow;
            Result.Response = e.Response;
            Result.RequestHttpMethod = Method.ToUpper();

            var httpWebResponse = WebResponse != null && WebResponse is HttpWebResponse
                                      ? (HttpWebResponse) WebResponse
                                      : null;

            if(httpWebResponse == null)
            {
                return;
            }

            var statusCode = Convert.ToInt32(httpWebResponse.StatusCode, CultureInfo.InvariantCulture);
            var statusDescription = httpWebResponse.StatusDescription;

#if TRACE
            Trace.WriteLine(String.Concat("RESPONSE: ", statusCode, " ", statusDescription));
            Trace.WriteLine("HEADERS:");
            foreach (var trace in httpWebResponse.Headers.AllKeys.Select(
                key => String.Concat("\t", key, ": ", httpWebResponse.Headers[key])))
            {
                Trace.WriteLine(trace);
            }
            Trace.WriteLine("BODY: " + e.Response);
#endif

            Result.ResponseHttpStatusCode = statusCode;
            Result.ResponseHttpStatusDescription = statusDescription;
            Result.ResponseType = httpWebResponse.ContentType;
            Result.ResponseLength = httpWebResponse.ContentLength;
            Result.ResponseUri = httpWebResponse.ResponseUri;
        }

        private void SetRequestResults(WebQueryRequestEventArgs e)
        {
            Result.RequestDate = DateTime.UtcNow;
            Result.RequestUri = new Uri(e.Request);
        }

#if !SILVERLIGHT
        protected virtual void SetWebProxy(WebRequest request)
        {
#if !Smartphone
            var proxyUriBuilder = new UriBuilder(Proxy);
            request.Proxy = new WebProxy(proxyUriBuilder.Host,
                                         proxyUriBuilder.Port);

            if (!proxyUriBuilder.UserName.IsNullOrBlank())
            {
                request.Headers["Proxy-Authorization"] = WebExtensions.ToBasicAuthorizationHeader(proxyUriBuilder.UserName,
                                                                                             proxyUriBuilder.Password);
            }
#else
            var uri = new Uri(Proxy);
            request.Proxy = new WebProxy(uri.Host, uri.Port);
            var userParts = uri.UserInfo.Split(new[] { ':' }).Where(ui => !ui.IsNullOrBlank()).ToArray();
            if (userParts.Length == 2)
            {
                request.Proxy.Credentials = new NetworkCredential(userParts[0], userParts[1]);
            }
#endif
        }
#endif
        protected virtual WebRequest BuildPostOrPutWebRequest(PostOrPut method, string url, out byte[] content)
        {
            return Entity == null
                       ? BuildPostOrPutFormWebRequest(method, url, out content)
                       : BuildPostOrPutEntityWebRequest(method, url, out content);
        }

        protected virtual WebRequest BuildPostOrPutFormWebRequest(PostOrPut method, string url, out byte[] content)
        {
            var parameters = AppendParameters(url).Replace(url + "?", "");
#if TRACE
            Trace.WriteLine(method.ToUpper() + ": " + url);
            Trace.WriteLine("BODY: " + parameters);
#endif

            var request = (HttpWebRequest) WebRequest.Create(url);
            AuthenticateRequest(request);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
            request.ContentType = "application/x-www-form-urlencoded";
            
            SetRequestMeta(request);

#if !SILVERLIGHT
            content = Encoding.ASCII.GetBytes(parameters);
            request.ContentLength = content.Length;
#else       
            content = Encoding.UTF8.GetBytes(parameters);
#endif
            return request;
        }

        private WebRequest BuildPostOrPutEntityWebRequest(PostOrPut method, string url, out byte[] content)
        {
            url = AppendParameters(url);

            var request = (HttpWebRequest)WebRequest.Create(url);
            AuthenticateRequest(request);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
            request.ContentType = Entity.ContentType;

            SetRequestMeta(request);

            var entity = Entity.Content.ToString();
#if TRACE
            Trace.WriteLine("BODY: " + entity);
            Trace.WriteLine(method.ToUpper() + ": " + url);
#endif

            content = Entity.ContentEncoding.GetBytes(entity);
#if !Silverlight
            // [DC]: This is set by Silverlight
            request.ContentLength = content.Length;
#endif

            return request;
        }

        protected virtual WebRequest BuildGetOrDeleteWebRequest(GetOrDelete method, string url)
        {
            url = AppendParameters(url);
#if TRACE
            Trace.WriteLine(method.ToUpper() + ": " + url);
#endif

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method == GetOrDelete.Get ? "GET" : "DELETE";
            AuthenticateRequest(request);
            SetRequestMeta(request);

            return request;
        }

        protected virtual void SetRequestMeta(HttpWebRequest request)
        {
            AppendHeaders(request);

#if !SILVERLIGHT
            if (ServicePoint != null)
            {
#if !Smartphone
                request.ServicePoint.ConnectionLeaseTimeout = ServicePoint.ConnectionLeaseTimeout;
                request.ServicePoint.ReceiveBufferSize = ServicePoint.ReceiveBufferSize;
                request.ServicePoint.UseNagleAlgorithm = ServicePoint.UseNagleAlgorithm;
                request.ServicePoint.BindIPEndPointDelegate = ServicePoint.BindIPEndPointDelegate;
#endif
                request.ServicePoint.ConnectionLimit = ServicePoint.ConnectionLimit;
                request.ServicePoint.Expect100Continue = ServicePoint.Expect100Continue;
                request.ServicePoint.MaxIdleTime = ServicePoint.MaxIdleTime;
            }
#endif

#if !SILVERLIGHT
            if (!Proxy.IsNullOrBlank())
            {
                SetWebProxy(request);
            }
#endif

            if (!UserAgent.IsNullOrBlank())
            {
#if !SILVERLIGHT
                request.UserAgent = UserAgent;
#else
                request.Headers["User-Agent"] = UserAgent;
#endif
            }

            if (DecompressionMethods != DecompressionMethods.None)
            {
#if !SILVERLIGHT
                request.AutomaticDecompression = DecompressionMethods;
#else
                // TODO: Implement decompression on HttpWebResponse
                // TODO: Implement decompression on HttpWebResponse
                switch(DecompressionMethods)
                {
                    case DecompressionMethods.GZip:
                        request.Accept = "gzip";
                        break;
                    case DecompressionMethods.Deflate:
                        request.Accept = "deflate";
                        break;
                    case DecompressionMethods.GZip | DecompressionMethods.Deflate:
                        request.Accept = "gzip,deflate";
                        break;
                }
#endif
            }
#if !SILVERLIGHT
            if (RequestTimeout.HasValue)
            {
                request.Timeout = (int)RequestTimeout.Value.TotalMilliseconds;
            }
#endif
            
#if !SILVERLIGHT
            if (KeepAlive)
            {
                request.KeepAlive = true;
            }
#endif
        }

        protected virtual void AppendHeaders(WebRequest request)
        {
            if (!(request is HttpWebRequest))
            {
                return;
            }

            // [DC]: Combine all duplicate headers into CSV
            var headers = new Dictionary<string, string>(0);
            foreach(var header in Headers)
            {
                string value;
                if(headers.ContainsKey(header.Name))
                {
                    value = String.Concat(headers[header.Name], ",", header.Value);
                    headers.Remove(header.Name);
                }
                else
                {
                    value = header.Value;
                }

                headers.Add(header.Name, value);
            }

            foreach (var header in headers)
            {
                if (_restrictedHeaderActions.ContainsKey(header.Key))
                {
                    _restrictedHeaderActions[header.Key].Invoke((HttpWebRequest) request, header.Value);
                }
                else
                {
                    AddHeader(header, request);
                }
            }

#if TRACE
            Trace.WriteLine("HEADERS:");
            foreach (var trace in request.Headers.AllKeys.Select(
                key => String.Concat("\t", key, ": ", request.Headers[key])))
            {
                Trace.WriteLine(trace);
            }
#endif
        }

        private static void AddHeader(KeyValuePair<string, string> header, WebRequest request)
        {
#if !SILVERLIGHT
            request.Headers.Add(header.Key, header.Value);
#else
            request.Headers[header.Key] = header.Value;
#endif
        }

#if !Silverlight
        private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions
            = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase)
                  {
                      {"Accept", (r, v) => r.Accept = v},
                      {"Connection", (r, v) => r.Connection = v},
                      {"Content-Length", (r, v) => r.ContentLength = Convert.ToInt64(v)},
                      {"Content-Type", (r, v) => r.ContentType = v},
                      {"Expect", (r, v) => r.Expect = v},
                      {"Date", (r, v) => { /* Set by system */ }},
                      {"Host", (r, v) => { /* Set by system */ }},
                      {"RetryIf-Modified-Since", (r, v) => r.IfModifiedSince = Convert.ToDateTime(v)},
                      {"Range", (r, v) => { throw new NotSupportedException( /* r.AddRange() */); }},
                      {"Referer", (r, v) => r.Referer = v},
                      {"Transfer-Encoding", (r, v) => { r.TransferEncoding = v; r.SendChunked = true; }},
                      {"User-Agent", (r, v) => r.UserAgent = v}
                  };
#else
        private readonly IDictionary<string, Action<HttpWebRequest, string>> _restrictedHeaderActions
            = new Dictionary<string, Action<HttpWebRequest, string>>(StringComparer.OrdinalIgnoreCase) {
                      { "Accept",            (r, v) => r.Accept = v },
                      { "Connection",        (r, v) => { /* Set by Silverlight */ }},           
                      { "Content-Length",    (r, v) => { /* Set by Silverlight */ }},
                      { "Content-Type",      (r, v) => r.ContentType = v },
                      { "Expect",            (r, v) => { /* Set by Silverlight */ }},
                      { "Date",              (r, v) => { /* Set by system */ }},
                      { "Host",              (r, v) => { /* Set by system */ }},
                      { "If-Modified-Since", (r, v) => { throw new NotSupportedException(/* r.AddRange() */); }},
                      { "Range",             (r, v) => { throw new NotSupportedException(/* r.AddRange() */); }},
                      { "Referer",           (r, v) => { throw new NotSupportedException(/* r.AddRange() */); }},
                      { "Transfer-Encoding", (r, v) => { throw new NotSupportedException(/* r.AddRange() */); }},
                      { "User-Agent",        (r, v) => { throw new NotSupportedException(/* r.AddRange() */); }}             
                  };
#endif

        protected virtual string AppendParameters(string url)
        {
            var parameters = 0;
            foreach (var parameter in Parameters.Where(parameter => !(parameter is HttpPostParameter) || Method == WebMethod.Post))
            {
                // GET parameters in URL
                url = url.Then(parameters > 0 || url.Contains("?") ? "&" : "?");
                url = url.Then("{0}={1}".FormatWith(parameter.Name, parameter.Value.UrlEncode()));
                parameters++;
            }

            return url;
        }

        // [DC] Headers don't need to be unique, this should change
        protected virtual WebHeaderCollection ParseInfoHeaders(IEnumerable<PropertyInfo> properties,
                                                               IDictionary<string, string> transforms)
        {
            var headers = new Dictionary<string, string>();
            
            Info.ParseNamedAttributes<HeaderAttribute>(properties, transforms, headers);

            var collection = new WebHeaderCollection();
            headers.ForEach(p => collection.Add(new WebHeader(p.Key, p.Value)));

            return collection;
        }

        protected virtual WebParameterCollection ParseInfoParameters(IEnumerable<PropertyInfo> properties,
                                                                     IDictionary<string, string> transforms)
        {
            var parameters = new Dictionary<string, string>();
            
            Info.ParseNamedAttributes<ParameterAttribute>(properties, transforms, parameters);

            var collection = new WebParameterCollection();
            parameters.ForEach(p => collection.Add(new WebParameter(p.Key, p.Value)));

            return collection;
        }

        protected virtual WebParameterCollection ParseInfoParameters()
        {
            IEnumerable<PropertyInfo> properties;
            IDictionary<string, string> transforms;
            ParseTransforms(out properties, out transforms);
            return ParseInfoParameters(properties, transforms);
        }

        private void ParseUserAgent(IEnumerable<PropertyInfo> properties)
        {
            var count = 0;
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<UserAgentAttribute>(true);
                count += attributes.Count();
                if (count > 1)
                {
                    throw new ArgumentException("Cannot declare more than one user agent per query");
                }

                if (count < 1)
                {
                    continue;
                }

                if (!UserAgent.IsNullOrBlank())
                {
                    continue;
                }

                var value = property.GetValue(Info, null);
                UserAgent = value != null ? value.ToString() : null;
            }
        }

        private void ParseWebEntity(IEnumerable<PropertyInfo> properties)
        {
            if (Entity != null)
            {
                // Already set by client or request
                return;
            }

            var count = 0;
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes<EntityAttribute>(true);
                count += attributes.Count();
                if (count > 1)
                {
                    throw new ValidationException("Cannot declare more than one entity per query");
                }

                if (count < 1)
                {
                    continue;
                }

                if (Entity != null)
                {
                    // Already set in this pass
                    continue;
                }

                var value = property.GetValue(Info, null);

                var content = value != null ? value.ToString() : null;
                var contentEncoding = attributes.Single().ContentEncoding;
                var contentType = attributes.Single().ContentType;

                Entity = new WebEntity
                {
                    Content = content,
                    ContentEncoding = contentEncoding,
                    ContentType = contentType
                };
            }
        }

        protected string HandleWebException(WebException ex)
        {
            if (ex.Response is HttpWebResponse)
            {
                WebResponse = ex.Response;

                using (var reader = new StreamReader(WebResponse.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    var args = new WebQueryResponseEventArgs(result, ex);

                    OnQueryResponse(args);
                    return result;
                }
            }

            return string.Empty;
        }

        protected abstract void SetAuthorizationHeader(WebRequest request, string header);
        protected abstract void AuthenticateRequest(WebRequest request);

        private static string CreateCacheKey(string prefix, string url)
        {
            return !prefix.IsNullOrBlank() ? "{0}_{1}".FormatWith(prefix, url) : url;
        }

        protected virtual string ExecuteWithCache(ICache cache,
                                                  string url,
                                                  string key,
                                                  Func<ICache, string, string> cacheScheme)
        {
            var fetch = cache.Get<string>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);
                return fetch;
            }

            var result = cacheScheme.Invoke(cache, url);
            return result;
        }

        protected virtual string ExecuteWithCacheAndAbsoluteExpiration(ICache cache,
                                                                       string url,
                                                                       string key,
                                                                       DateTime expiry,
                                                                       Func<ICache, string, DateTime, string> cacheScheme)
        {
            var fetch = cache.Get<string>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);

                return fetch;
            }

            var result = cacheScheme.Invoke(cache, url, expiry);
            return result;
        }

        protected virtual string ExecuteWithCacheAndSlidingExpiration(ICache cache,
                                                                      string url,
                                                                      string key,
                                                                      TimeSpan expiry,
                                                                      Func<ICache, string, TimeSpan, string>
                                                                          cacheScheme)
        {
            var fetch = cache.Get<string>(CreateCacheKey(key, url));
            if (fetch != null)
            {
                // [DC]: In order to build results, an event must still raise
                var responseArgs = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(responseArgs);
                return fetch;
            }

            var result = cacheScheme.Invoke(cache, url, expiry);
            return result;
        }

#if !SILVERLIGHT
        protected virtual string ExecuteGetOrDelete(GetOrDelete method, string url, string key, ICache cache, out WebException exception)
        {
            WebException ex = null;
            var ret = ExecuteWithCache(cache, url, key, (c, u) => ExecuteGetOrDelete(method, cache, url, key, out ex));
            exception = ex;
            return ret; 

        }

        protected virtual string ExecuteGetOrDelete(GetOrDelete method, 
                                                    string url, 
                                                    string key, 
                                                    ICache cache, 
                                                    DateTime absoluteExpiration, 
                                                    out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndAbsoluteExpiration(cache, url, key, absoluteExpiration,
                                                            (c, u, e) =>
                                                            ExecuteGetOrDelete(method, cache, url, key, absoluteExpiration, out ex));
            exception = ex;
            return ret; 
        }

        protected virtual string ExecuteGetOrDelete(GetOrDelete method, 
                                                    string url, 
                                                    string key, 
                                                    ICache cache, 
                                                    TimeSpan slidingExpiration, 
                                                    out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndSlidingExpiration(cache, url, key, slidingExpiration,
                                                           (c, u, e) =>
                                                           ExecuteGetOrDelete(method, cache, url, key, slidingExpiration, out ex));
            exception = ex;
            return ret; 
        }

        private string ExecuteGetOrDelete(GetOrDelete method,
                                          ICache cache, 
                                          string url, 
                                          string key, 
                                          out WebException exception)
        {
            var result = ExecuteGetOrDelete(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result);
            }
            return result;
        }

        private string ExecuteGetOrDelete(GetOrDelete method, ICache cache, string url, string key,
                                          DateTime absoluteExpiration, out WebException exception)
        {
            var result = ExecuteGetOrDelete(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result, absoluteExpiration);
            }
            return result;
        }

        private string ExecuteGetOrDelete(GetOrDelete method, ICache cache, string url, string key,
                                          TimeSpan slidingExpiration, out WebException exception)
        {
            var result = ExecuteGetOrDelete(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result, slidingExpiration);
            }
            return result;
        }
#endif  

        public virtual event EventHandler<WebQueryRequestEventArgs> QueryRequest;
        public virtual void OnQueryRequest(WebQueryRequestEventArgs args)
        {
            if (QueryRequest != null)
            {
                QueryRequest(this, args);
            }
        }

        public virtual event EventHandler<WebQueryResponseEventArgs> QueryResponse;
        public virtual void OnQueryResponse(WebQueryResponseEventArgs args)
        {
            if (QueryResponse != null)
            {
                QueryResponse(this, args);
            }
        }

#if !SILVERLIGHT
        public virtual void ExecuteStreamGet(string url, TimeSpan duration, int resultCount)
        {
            WebResponse = null;

            var request = BuildGetOrDeleteWebRequest(GetOrDelete.Get, url);

            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            Stream stream = null;
            WebResponse response = null;

            try
            {
                response = request.GetResponse();

                using (stream = response.GetResponseStream())
                {
                    // [DC]: cannot refactor this block to common method; will cause wc/hwr to hang
                    var count = 0;
                    var results = new List<string>();
                    var start = DateTime.UtcNow;

                    using (var reader = new StreamReader(stream))
                    {
                        string line;

                        while ((line = reader.ReadLine()).Length > 0)
                        {
                            if (line.Equals(Environment.NewLine))
                            {
                                // Keep-Alive
                                continue;
                            }

                            if (line.Equals("<html>"))
                            {
                                // We're looking at a 401 or similar; construct error result?
                                return;
                            }

                            results.Add(line);
                            count++;

                            if (count < resultCount)
                            {
                                // Result buffer
                                continue;
                            }

                            var sb = new StringBuilder();
                            foreach (var result in results)
                            {
                                sb.AppendLine(result);
                            }

                            var responseArgs = new WebQueryResponseEventArgs(sb.ToString());
                            OnQueryResponse(responseArgs);

                            count = 0;

                            var now = DateTime.UtcNow;
                            if (duration == TimeSpan.Zero || now.Subtract(start) < duration)
                            {
                                continue;
                            }

                            // Time elapsed
                            request.Abort();
                            return;
                        }

                        // Stream dried up
                    }

                    request.Abort();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    response = ex.Response;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }

                WebResponse = response;
            }
        }

        public virtual void ExecuteStreamPost(PostOrPut method, string url, TimeSpan duration, int resultCount)
        {
            WebResponse = null;
            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            Stream stream = null;
            try
            {
                using (stream = request.GetRequestStream())
                {
                    stream.Write(content, 0, content.Length);
                    stream.Close();

                    var response = request.GetResponse();
                    WebResponse = response;
                    
                    using (var responseStream = response.GetResponseStream())
                    {
                        // [DC]: cannot refactor this block to common method; will cause hwr to hang
                        var count = 0;
                        var results = new List<string>();
                        var start = DateTime.UtcNow;

                        using (var reader = new StreamReader(responseStream))
                        {
                            string line;

                            while ((line = reader.ReadLine()).Length > 0)
                            {
                                if (line.Equals(Environment.NewLine))
                                {
                                    // Keep-Alive
                                    continue;
                                }

                                if (line.Equals("<html>"))
                                {
                                    // We're looking at a 401 or similar; construct error result?
                                    return;
                                }

                                results.Add(line);
                                count++;

                                if (count < resultCount)
                                {
                                    // Result buffer
                                    continue;
                                }

                                var sb = new StringBuilder();
                                foreach (var result in results)
                                {
                                    sb.AppendLine(result);
                                }

                                var responseArgs = new WebQueryResponseEventArgs(sb.ToString());
                                OnQueryResponse(responseArgs);

                                count = 0;

                                var now = DateTime.UtcNow;
                                if (now.Subtract(start) < duration)
                                {
                                    continue;
                                }

                                // Time elapsed
                                request.Abort();
                                return;
                            }

                            // Stream dried up
                        }
                    }
                }
            }
            catch (WebException)
            {
                // 
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }
#endif

#if !SILVERLIGHT
        protected virtual string ExecuteGetOrDelete(GetOrDelete method, string url, out WebException exception)
        {
            WebResponse = null;
            var request = BuildGetOrDeleteWebRequest(method, url);
            
            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            return ExecuteGetOrDelete(request, out exception);
        }

        private string ExecuteGetOrDelete(WebRequest request, out WebException exception)
        {
            try
            {
                var response = request.GetResponse();
                WebResponse = response;

                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var result = reader.ReadToEnd();

                        var responseArgs = new WebQueryResponseEventArgs(result);
                        OnQueryResponse(responseArgs);

                        exception = null;
                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex;
                return HandleWebException(ex);
            }
        }
#endif

        protected virtual HttpWebRequest BuildMultiPartFormRequest(PostOrPut method, string url,
                                                                   IEnumerable<HttpPostParameter> parameters,
                                                                   out byte[] bytes)
        {
            var boundary = Guid.NewGuid().ToString();
            var request = (HttpWebRequest) WebRequest.Create(url);
            AuthenticateRequest(request);

#if !SILVERLIGHT
            request.PreAuthenticate = true;
            request.AllowWriteStreamBuffering = true;
#endif
            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";

            // [DC]: This will need to be refactored for large uploads
            var contents = BuildMultiPartFormRequestParameters(boundary, parameters);
            var payload = contents.ToString();

#if TRACE
            Trace.WriteLine(method.ToUpper() + ": " + url);
            Trace.WriteLine("BODY: " + payload);
#endif

#if !Smartphone
            bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(payload);
#else
            bytes = Encoding.GetEncoding(1252).GetBytes(payload);
#endif

#if !SILVERLIGHT
            request.ContentLength = bytes.Length;
#endif
            return request;
        }

        protected static StringBuilder BuildMultiPartFormRequestParameters(string boundary,
                                                                           IEnumerable<HttpPostParameter> parameters)
        {
            var header = string.Format("--{0}", boundary);
            var footer = string.Format("--{0}--", boundary);
            var contents = new StringBuilder();

            foreach (var parameter in parameters)
            {
                contents.AppendLine(header);
                switch (parameter.Type)
                {
                    case HttpPostParameterType.File:
                        {
#if !Smartphone && !SILVERLIGHT
                            var fileBytes = File.ReadAllBytes(parameter.FilePath);
#else
                            byte[] fileBytes;
                            var info = new FileInfo(parameter.FilePath);
                            using (var fs = new FileStream(parameter.FilePath, FileMode.Open, FileAccess.Read))
                            {
                                using (var br = new BinaryReader(fs))
                                {
                                    fileBytes = br.ReadBytes((int) info.Length);
                                }
                            }
#endif
                            const string fileMask = "Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"";
                            var fileHeader = fileMask.FormatWith(parameter.Name, parameter.FileName);
#if !Smartphone
                            var fileData = Encoding.GetEncoding("iso-8859-1").GetString(fileBytes, 0, fileBytes.Length);
#else
                            var fileData = Encoding.GetEncoding(1252).GetString(fileBytes, 0, fileBytes.Length);
#endif
                            contents.AppendLine(fileHeader);
                            contents.AppendLine("Content-Type: {0}".FormatWith(parameter.ContentType.ToLower()));
                            contents.AppendLine();
                            contents.AppendLine(fileData);

                            break;
                        }
                    case HttpPostParameterType.Field:
                        {
                            contents.AppendLine("Content-Disposition: form-data; name=\"{0}\"".FormatWith(parameter.Name));
                            contents.AppendLine();
                            contents.AppendLine(parameter.Value);
                            break;
                        }
                }
            }

            contents.AppendLine(footer);
            return contents;
        }

#if !SILVERLIGHT
        protected virtual string ExecutePostOrPut(PostOrPut method, string url, out WebException exception)
        {
            WebResponse = null;
            exception = null;
            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(content, 0, content.Length);
                    stream.Close();

                    // [DC] Avoid disposing until no longer needed to build results
                    var response = request.GetResponse();
                    WebResponse = response;

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();

                        var responseArgs = new WebQueryResponseEventArgs(result);
                        OnQueryResponse(responseArgs);

                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex; 
                return HandleWebException(ex);
            }
        }

        protected virtual string ExecutePostOrPut(PostOrPut method, 
                                                  string url, 
                                                  IEnumerable<HttpPostParameter> parameters,
                                                  out WebException exception)
        {
            WebResponse = null;
            byte[] bytes;
            var request = BuildMultiPartFormRequest(method, url, parameters, out bytes);

            try
            {
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    // Avoid disposing until no longer needed to build results
                    var response = request.GetResponse();
                    WebResponse = response;

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();

                        var responseArgs = new WebQueryResponseEventArgs(result);
                        OnQueryResponse(responseArgs);

                        WebResponse = response;
                        exception = null;

                        return result;
                    }
                }
            }
            catch (WebException ex)
            {
                exception = ex;
                return HandleWebException(ex);
            }
        }
#endif

#if !SILVERLIGHT
        public virtual string Request(string url, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDelete(GetOrDelete.Get, url, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, out exception);
                case WebMethod.Delete:
                    return ExecuteGetOrDelete(GetOrDelete.Delete, url, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDelete(GetOrDelete.Get, url, key, cache, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, out exception);
                case WebMethod.Post: 
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, out exception);
                case WebMethod.Delete:
                    return ExecuteGetOrDelete(GetOrDelete.Delete, url, key, cache, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDelete(GetOrDelete.Get, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Delete:
                    return ExecuteGetOrDelete(GetOrDelete.Delete, url, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDelete(GetOrDelete.Get, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Delete:
                    return ExecuteGetOrDelete(GetOrDelete.Delete, url, key, cache, slidingExpiration, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, IEnumerable<HttpPostParameter> parameters, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, parameters, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, parameters, out exception);
                default:
                    throw new NotSupportedException("Only HTTP POSTs and PUTs can use multi-part parameters");
            }
        }
#endif
        public virtual IAsyncResult RequestAsync(string url)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Get, url);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Delete, url);
                default:
                    throw new NotSupportedException("Unknown web method");
            }
        }

        public virtual IAsyncResult RequestAsync(string url, 
                                                 string key, 
                                                 ICache cache)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Get, url, key, cache);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Delete, url, key, cache);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }

        public virtual IAsyncResult RequestAsync(string url,
                                                 string key, 
                                                 ICache cache, 
                                                 DateTime absoluteExpiration)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Get, url, key, cache, absoluteExpiration);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, absoluteExpiration);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, absoluteExpiration);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Delete, url, key, cache, absoluteExpiration);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }

        public virtual IAsyncResult RequestAsync(string url, 
                                                 string key, 
                                                 ICache cache, 
                                                 TimeSpan slidingExpiration)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Get, url, key, cache, slidingExpiration);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, slidingExpiration);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, slidingExpiration);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetOrDelete.Delete, url, key, cache, slidingExpiration);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }

        public virtual IAsyncResult RequestAsync(string url, IEnumerable<HttpPostParameter> parameters)
        {
            switch (Method)
            {
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, parameters);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, parameters);
                default:
                    throw new NotSupportedException("Only HTTP POSTS can use multi-part forms");
            }
        }

#if !SILVERLIGHT
        public virtual string ExecutePostOrPut(PostOrPut method, 
                                               string url, 
                                               string key, 
                                               ICache cache, 
                                               out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCache(cache, url, key, (c, u) => ExecutePostOrPut(method, cache, url, key, out ex));
            exception = ex;
            return ret; 
        }

        public virtual string ExecutePostOrPut(PostOrPut method, string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndAbsoluteExpiration(cache, url, key, absoluteExpiration,
                                                            (c, u, e) =>
                                                            ExecutePostOrPut(method, cache, url, key, absoluteExpiration, out ex));
            exception = ex;
            return ret; 

        }

        public virtual string ExecutePostOrPut(PostOrPut method, string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception )
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndSlidingExpiration(cache, url, key, slidingExpiration,
                                                           (c, u, e) =>
                                                           ExecutePostOrPut(method, cache, url, key, slidingExpiration, out ex));
            exception = ex; 
            return ret; 
        }

        private string ExecutePostOrPut(PostOrPut method, 
                                        ICache cache, 
                                        string url, 
                                        string key, 
                                        out WebException exception)
        {
            var result = ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result);
            }
            return result;
        }

        private string ExecutePostOrPut(PostOrPut method, 
                                        ICache cache, 
                                        string url, 
                                        string key,
                                        DateTime absoluteExpiration, 
                                        out WebException exception)
        {
            var result = ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result, absoluteExpiration);
            }
            return result;
        }

        private string ExecutePostOrPut(PostOrPut method, ICache cache, string url, string key,
                                        TimeSpan slidingExpiration, out WebException exception)
        {
            var result = ExecutePostOrPut(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result, slidingExpiration);
            }
            return result;
        }

        public static string QuickGet(string url)
        {
            return QuickGet(url, null, null, null);
        }

        public static string QuickGet(string url, string username, string password)
        {
            return QuickGet(url, null, username, password);
        }

        public static string QuickGet(string url, IDictionary<string, string> headers, string username, string password)
        {
            var request = WebRequest.Create(url);
            request.PreAuthenticate = true;
            if(!username.IsNullOrBlank() && !password.IsNullOrBlank())
            {
                request.Headers["Authorization"] = WebExtensions.ToBasicAuthorizationHeader(username, password);    
            }

            WebResponse response = null;

            try
            {
                using (response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse && response != null)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = reader.ReadToEnd();
                        return result;
                    }
                }

                return string.Empty;
            }
        }
#endif
    }
}