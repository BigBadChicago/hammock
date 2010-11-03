using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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

#if SILVERLIGHT && !WindowsPhone
using System.Windows.Browser;
using System.Net.Browser;
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

        private WebEntity _entity;
        protected internal virtual WebEntity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
                HasEntity = _entity != null;
            }
        }
        
        public virtual WebMethod Method { get; set; }
        public virtual string Proxy { get; set; }
        public virtual string AuthorizationHeader { get; internal set; }
        public DecompressionMethods DecompressionMethods { get; set; }
        public virtual TimeSpan? RequestTimeout { get; set; }
        public virtual WebQueryResult Result { get; internal set; }
        public virtual object UserState { get; internal set; }

#if SILVERLIGHT
        public virtual bool HasElevatedPermissions { get; set; }

        // [DC]: Headers to use when access isn't direct
        public virtual string SilverlightAuthorizationHeader { get; set; }
        public virtual string SilverlightMethodHeader { get; set; }
        public virtual string SilverlightUserAgentHeader { get; set; }
        public virtual string SilverlightAcceptEncodingHeader { get; set; }        
#endif
        
#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
        public virtual bool KeepAlive { get; set; }
        public virtual bool FollowRedirects { get; internal set; }
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

        public virtual Byte[] ByteResponse
        {
            get; set;
        }

        public virtual bool HasEntity { get; set; }
        public virtual byte[] PostContent { get; set; }

#if SL3 || SL4
        static WebQuery()
        {
            // [DC]: Opt-in to the networking stack so we can get headers for proxies
            WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
        }
#endif

        protected WebQuery() : this(null)
        {

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
            Result.IsMock = WebResponse is MockHttpWebResponse;
            Result.TimedOut = TimedOut;
            string version;
            int statusCode;
            string statusDescription;
            System.Net.WebHeaderCollection headers;
            string contentType;
            long contentLength;
            Uri responseUri;
            CastWebResponse(
                out version, out statusCode, out statusDescription, out headers, 
                out contentType, out contentLength, out responseUri
                );

            TraceResponse(
                responseUri, version, headers, statusCode, statusDescription, e.Response
                );

            Result.WebResponse = WebResponse;
            Result.ResponseHttpStatusCode = statusCode;
            Result.ResponseHttpStatusDescription = statusDescription;
            Result.ResponseType = contentType;
            Result.ResponseLength = contentLength;
            Result.ResponseUri = responseUri;
            Result.Exception = e.Exception;
            Result.ByteResponse = ByteResponse;
            
        }

        [Conditional("TRACE")]
        private static void TraceResponse(Uri uri, string version, System.Net.WebHeaderCollection headers, int statusCode, string statusDescription, string response)
        {
            Trace.WriteLine(
                String.Concat("\r\n--RESPONSE:", " ", uri)
                );
            Trace.WriteLine(
                String.Concat(version, " ", statusCode, " ", statusDescription)
                );
            foreach (var trace in headers.AllKeys.Select(
                key => String.Concat(key, ": ", headers[key])))
            {
                Trace.WriteLine(trace);
            }
            Trace.WriteLine(String.Concat("\r\n ", response)
                );
        }

        private void SetRequestResults(WebQueryRequestEventArgs e)
        {
            Result.RequestDate = DateTime.UtcNow;
            Result.RequestUri = new Uri(e.Request);
#if !SILVERLIGHT
            Result.RequestKeptAlive = KeepAlive;
#endif
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
            return !HasEntity
                       ? BuildPostOrPutFormWebRequest(method, url, out content)
                       : BuildPostOrPutEntityWebRequest(method, url, out content);
        }

        protected virtual WebRequest BuildPostOrPutFormWebRequest(PostOrPut method, string url, out byte[] content)
        {
            var parameters = AppendParameters(url).Replace(url + "?", "");

            var request = WebRequest.Create(url);
            AuthenticateRequest(request);
#if SILVERLIGHT
            var httpMethod = method == PostOrPut.Post ? "POST" : "PUT"; ;
            if (HasElevatedPermissions)
            {
                request.Method = httpMethod;
            }
            else
            {
                request.Method = "POST";
                request.Headers[SilverlightMethodHeader] = httpMethod;
            }
#else
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
#endif
            request.ContentType = "application/x-www-form-urlencoded";

            TraceRequest(request);

            HandleRequestMeta(request);

            var encoding = Encoding.UTF8;
            content = PostContent ?? encoding.GetBytes(parameters);

#if TRACE
            Trace.WriteLine(String.Concat(
                "\r\n", content)
                );
#endif

#if !SILVERLIGHT
            request.ContentLength = content.Length;
#endif
            return request;
        }

        private WebRequest BuildPostOrPutEntityWebRequest(PostOrPut method, string url, out byte[] content)
        {
            url = AppendParameters(url);

            var request = WebRequest.Create(url);
            AuthenticateRequest(request);
#if SILVERLIGHT && !WindowsPhone
            var httpMethod = method == PostOrPut.Post ? "POST" : "PUT"; ;
            if (HasElevatedPermissions)
            {
                request.Method = httpMethod;
            }
            else
            {
                request.Method = "POST";
                request.Headers[SilverlightMethodHeader] = httpMethod;
            }
#else
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
#endif

            TraceRequest(request);

            HandleRequestMeta(request);

            if (Entity != null)
            {
                var entity = Entity.Content;

                content = Entity.ContentEncoding.GetBytes(entity);
                request.ContentType = Entity.ContentType;
#if TRACE
                Trace.WriteLine(String.Concat(
                    "\r\n", entity)
                    );
#endif
                
#if !SILVERLIGHT 
                // [DC]: This is set by Silverlight
                request.ContentLength = content.Length;
#endif
            }
            else
            {
                content = new MemoryStream().ToArray();
            }

            return request;
        }

        protected virtual WebRequest BuildGetDeleteHeadOptionsWebRequest(GetDeleteHeadOptions method, string url)
        {
            url = AppendParameters(url);

            var request = WebRequest.Create(url);
#if SILVERLIGHT && !WindowsPhone
            var httpMethod = method.ToUpper();
            if (HasElevatedPermissions)
            {
                request.Method = httpMethod;
            }
            else
            {
                request.Method = "POST";
                request.Headers[SilverlightMethodHeader] = httpMethod;
            }
#else
            request.Method = method.ToUpper();
#endif
            AuthenticateRequest(request);

            TraceRequest(request);
            
            HandleRequestMeta(request);

            return request;
        }

        private void HandleRequestMeta(WebRequest request)
        {
            // [DC] LSP violation necessary for "pure" mocks
            if (request is HttpWebRequest)
            {
                SetRequestMeta((HttpWebRequest)request);
            }
            else
            {
                AppendHeaders(request);
                if (!UserAgent.IsNullOrBlank())
                {
#if SILVERLIGHT && !WindowsPhone
                    // [DC] User-Agent is still restricted in elevated mode
                    request.Headers[SilverlightUserAgentHeader] = UserAgent;
#else
                    request.Headers["User-Agent"] = UserAgent;
#endif
                }
            }
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
            request.AllowAutoRedirect = FollowRedirects;
#endif

            if (!UserAgent.IsNullOrBlank())
            {
#if !SILVERLIGHT && !WindowsPhone
                request.UserAgent = UserAgent;
#else
                // [DC]: User agent is still restricted in elevated permissions
                request.Headers[SilverlightUserAgentHeader] = UserAgent;
#endif
            }

            if (DecompressionMethods != DecompressionMethods.None)
            {
#if !SILVERLIGHT && !WindowsPhone
                request.AutomaticDecompression = DecompressionMethods;
#else
                if (HasElevatedPermissions)
                {
                    switch (DecompressionMethods)
                    {
                        case DecompressionMethods.GZip:
                            request.Headers["AcceptEncoding"] = "gzip";
                            break;
                        case DecompressionMethods.Deflate:
                            request.Headers["AcceptEncoding"] = "deflate";
                            break;
                        case DecompressionMethods.GZip | DecompressionMethods.Deflate:
                            request.Headers["AcceptEncoding"] = "gzip,deflate";
                            break;
                    }
                }
                else
                {
                    switch (DecompressionMethods)
                    {
                        case DecompressionMethods.GZip:
                            request.Headers[SilverlightAcceptEncodingHeader] = "gzip";
                            break;
                        case DecompressionMethods.Deflate:
                            request.Headers[SilverlightAcceptEncodingHeader] = "deflate";
                            break;
                        case DecompressionMethods.GZip | DecompressionMethods.Deflate:
                            request.Headers[SilverlightAcceptEncodingHeader] = "gzip,deflate";
                            break;
                    }
                }
#endif
            }
#if !SILVERLIGHT
            if (RequestTimeout.HasValue)
            {
                // [DC] Need to synchronize these as Timeout is ignored in async requests
                request.Timeout = (int)RequestTimeout.Value.TotalMilliseconds;
                request.ReadWriteTimeout = (int)RequestTimeout.Value.TotalMilliseconds;
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
            if (!(request is HttpWebRequest) &&
                !(request is MockHttpWebRequest))
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
                    if(request is HttpWebRequest)
                    {
#if SILVERLIGHT
                    if(header.Key.EqualsIgnoreCase("User-Agent"))
                    {
                        // [DC]: User-Agent is still restricted in elevated mode
                        request.Headers[SilverlightUserAgentHeader ?? "X-UserAgent"] = UserAgent;
                        continue;
                    }

                    if(header.Key.EqualsIgnoreCase("Accept-Encoding"))
                    {
                        if (HasElevatedPermissions)
                        {
                            request.Headers[header.Key] = UserAgent;
                        }
                        else
                        {
                            request.Headers[SilverlightAcceptEncodingHeader] = UserAgent;
                        }
                        continue;
                    }

                    if(header.Key.EqualsIgnoreCase("Authorization"))
                    {
                        if (HasElevatedPermissions)
                        {
                            request.Headers[header.Key] = AuthorizationHeader;
                        }
                        else
                        {
                            request.Headers[SilverlightAuthorizationHeader] = AuthorizationHeader;
                        }
                        continue;
                    }
#endif

                    _restrictedHeaderActions[header.Key].Invoke((HttpWebRequest) request, header.Value);
                    }
                    if(request is MockHttpWebRequest)
                    {
                        AddHeader(header, request);
                    }
                }
                else
                {
                    AddHeader(header, request);
                }
            }

#if TRACE
            foreach (var trace in request.Headers.AllKeys.Select(
                key => String.Concat(key, ": ", request.Headers[key])))
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

#if !SILVERLIGHT
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
                      { "Accept",            (r, v) => { /* Not supported here },
                      { "Connection",        (r, v) => { /* Set by Silverlight */ }},           
                      { "Content-Length",    (r, v) => { /* Set by Silverlight */ }},
                      { "Content-Type",      (r, v) => r.ContentType = v },
                      { "Expect",            (r, v) => { /* Set by Silverlight */ }},
                      { "Date",              (r, v) => { /* Set by Silverlight */ }},
                      { "Host",              (r, v) => { /* Set by Silverlight */ }},
                      { "If-Modified-Since", (r, v) => { /* Not supported */ }},
                      { "Range",             (r, v) => { /* Not supported */ }},
                      { "Referer",           (r, v) => { /* Not supported */ }},
                      { "Transfer-Encoding", (r, v) => { /* Not supported */ }},
                      { "User-Agent",        (r, v) => { /* Not supported here */  }}             
                  };
#endif

        protected virtual string AppendParameters(string url)
        {
            var count = 0;

            var parameters = Parameters.Where(
                parameter => !(parameter is HttpPostParameter) || Method == WebMethod.Post).Where(
                parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)
                );

            foreach (var parameter in parameters)
            {
                // GET parameters in URL
                url = url.Then(count > 0 || url.Contains("?") ? "&" : "?");
                url = url.Then("{0}={1}".FormatWith(parameter.Name, parameter.Value.UrlEncode()));
                count++;
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

        public abstract string GetAuthorizationContent();

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
        protected virtual string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, string url, string key, ICache cache, out WebException exception)
        {
            WebException ex = null;
            var ret = ExecuteWithCache(cache, url, key, (c, u) => ExecuteGetDeleteHeadOptions(method, cache, url, key, out ex));
            exception = ex;
            return ret; 

        }

        protected virtual string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, 
                                                    string url, 
                                                    string key, 
                                                    ICache cache, 
                                                    DateTime absoluteExpiration, 
                                                    out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndAbsoluteExpiration(cache, url, key, absoluteExpiration,
                                                            (c, u, e) =>
                                                            ExecuteGetDeleteHeadOptions(method, cache, url, key, absoluteExpiration, out ex));
            exception = ex;
            return ret; 
        }

        protected virtual string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, 
                                                    string url, 
                                                    string key, 
                                                    ICache cache, 
                                                    TimeSpan slidingExpiration, 
                                                    out WebException exception)
        {
            WebException ex = null; 
            var ret = ExecuteWithCacheAndSlidingExpiration(cache, url, key, slidingExpiration,
                                                           (c, u, e) =>
                                                           ExecuteGetDeleteHeadOptions(method, cache, url, key, slidingExpiration, out ex));
            exception = ex;
            return ret; 
        }

        private string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method,
                                                   ICache cache, 
                                                   string url, 
                                                   string key, 
                                                   out WebException exception)
        {
            var result = ExecuteGetDeleteHeadOptions(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result);
            }
            return result;
        }

        private string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, ICache cache, string url, string key,
                                                   DateTime absoluteExpiration, out WebException exception)
        {
            var result = ExecuteGetDeleteHeadOptions(method, url, out exception);
            if (exception == null)
            {
                cache.Insert(CreateCacheKey(key, url), result, absoluteExpiration);
            }
            return result;
        }

        private string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, ICache cache, string url, string key,
                                                   TimeSpan slidingExpiration, out WebException exception)
        {
            var result = ExecuteGetDeleteHeadOptions(method, url, out exception);
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
            var handler = QueryRequest;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public virtual event EventHandler<WebQueryResponseEventArgs> QueryResponse;
        public virtual void OnQueryResponse(WebQueryResponseEventArgs args)
        {
            var handler = QueryResponse;
            if (handler != null)
            {
                handler(this, args);
            }
        }

#if !SILVERLIGHT
        protected virtual string ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions method, string url, out WebException exception)
        {
            WebResponse = null;
            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            
            var requestArgs = new WebQueryRequestEventArgs(url);
            OnQueryRequest(requestArgs);

            return ExecuteGetDeleteHeadOptions(request, out exception);
        }

        private string ExecuteGetDeleteHeadOptions(WebRequest request, out WebException exception)
        {
            try
            {
                var response = request.GetResponse();
                WebResponse = response;
                
                using (var stream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[4096];

                    string content;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                        } while (count != 0);

                        memoryStream.Position = 0;
                        using (StreamReader sr = new StreamReader(memoryStream))
                        {
                            content = sr.ReadToEnd();
                        }

                        ByteResponse = memoryStream.ToArray();
                        var result = content;

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
        protected virtual WebRequest BuildMultiPartFormRequest(PostOrPut method, string url,
                                                               IEnumerable<HttpPostParameter> parameters,
                                                               out byte[] bytes)
        {
            url = AppendParameters(url);

            var boundary = Guid.NewGuid().ToString();
            var request = WebRequest.Create(url);
            AuthenticateRequest(request);

            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";

            TraceRequest(request);

            HandleRequestMeta(request);

#if !Smartphone 
            var encoding = Encoding.GetEncoding("iso-8859-1");
#else
            var encoding =  Encoding.GetEncoding(1252);
#endif
            // [DC]: This will need to be refactored for larger uploads
            var contents = BuildMultiPartFormRequestParameters(encoding, boundary, parameters);
            var payload = contents.ToString();
            
            bytes = encoding.GetBytes(payload);

#if !SILVERLIGHT
            request.ContentLength = bytes.Length;
#endif
            return request;
        }

        [Conditional("TRACE")]
        private static void TraceRequest(WebRequest request)
        {
            var version = request is HttpWebRequest ?
#if SILVERLIGHT
                "HTTP/v1.1" :
#else
                string.Concat("HTTP/", ((HttpWebRequest)request).ProtocolVersion) :
#endif
 "HTTP/v1.1";

            Trace.WriteLine(
                String.Concat("--REQUEST: ", request.RequestUri.Scheme, "://", request.RequestUri.Host)
                );
            var pathAndQuery = String.Concat(
                request.RequestUri.AbsolutePath, string.IsNullOrEmpty(request.RequestUri.Query)
                                                     ? ""
                                                     : string.Concat("?", request.RequestUri.Query)
                );
            Trace.WriteLine(
                String.Concat(request.Method, " ", pathAndQuery, " ", version
                ));
        }

        protected static StringBuilder BuildMultiPartFormRequestParameters(Encoding encoding, string boundary, IEnumerable<HttpPostParameter> parameters)
        {
            var header = string.Format("--{0}", boundary);
            var footer = string.Format("--{0}--", boundary);
            var contents = new StringBuilder();

            foreach (var parameter in parameters)
            {
                contents.AppendLine(header);
#if TRACE
                Trace.WriteLine(header);
#endif
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
                            var fileData = encoding.GetString(fileBytes, 0, fileBytes.Length);
                            var fileLine = "Content-Type: {0}".FormatWith(parameter.ContentType.ToLower());

                            contents.AppendLine(fileHeader);
                            contents.AppendLine(fileLine);
                            contents.AppendLine();
                            contents.AppendLine(fileData);

#if TRACE
                            Trace.WriteLine(fileHeader);
                            Trace.WriteLine(fileLine);
                            Trace.WriteLine("");
                            Trace.WriteLine(String.Concat("[FILE DATA][", encoding, "]"));
#endif
                            break;
                        }
                    case HttpPostParameterType.Field:
                        {
                            var fieldLine = "Content-Disposition: form-data; name=\"{0}\"".FormatWith(parameter.Name);
                            contents.AppendLine(fieldLine);
                            contents.AppendLine();
                            contents.AppendLine(parameter.Value);
#if TRACE
                            Trace.WriteLine(fieldLine);
                            Trace.WriteLine("");
                            Trace.WriteLine(parameter.Value);
#endif
                            break;
                        }
                }
            }

            contents.AppendLine(footer);
#if TRACE
            Trace.WriteLine(footer);
#endif
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
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, out exception);
                case WebMethod.Delete:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, out exception);
                case WebMethod.Head:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, out exception);
                case WebMethod.Options:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, out exception);
                case WebMethod.Post: 
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, out exception);
                case WebMethod.Delete:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, out exception);
                case WebMethod.Head:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache,  out exception);
                case WebMethod.Options:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, DateTime absoluteExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Delete:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Head:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache, absoluteExpiration, out exception);
                case WebMethod.Options:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, absoluteExpiration, out exception);
                default:
                    throw new NotSupportedException("Unsupported web method");
            }
        }

        public virtual string Request(string url, string key, ICache cache, TimeSpan slidingExpiration, out WebException exception)
        {
            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Put:
                    return ExecutePostOrPut(PostOrPut.Put, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Post:
                    return ExecutePostOrPut(PostOrPut.Post, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Delete:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Head:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, out exception);
                case WebMethod.Options:
                    return ExecuteGetDeleteHeadOptions(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, out exception);
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

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, userState);
                default:
                    throw new NotSupportedException("Unknown web method");
            }
        }
#else
        public virtual void RequestAsync(string url, object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, userState);
                    break;
                default:
                    throw new NotSupportedException("Unknown web method");
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        string key, 
                                                        ICache cache,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url,
                                                        string key, 
                                                        ICache cache, 
                                                        DateTime absoluteExpiration,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, absoluteExpiration, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         DateTime absoluteExpiration,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, absoluteExpiration, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, absoluteExpiration, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        string key, 
                                                        ICache cache, 
                                                        TimeSpan slidingExpiration,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, slidingExpiration, userState);
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, slidingExpiration, userState);
                case WebMethod.Delete:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, userState);
                case WebMethod.Head:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, userState);
                case WebMethod.Options:
                    return ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, userState);
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         string key,
                                         ICache cache,
                                         TimeSpan slidingExpiration,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Get:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Get, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Delete:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Delete, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Head:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Head, url, key, cache, slidingExpiration, userState);
                    break;
                case WebMethod.Options:
                    ExecuteGetOrDeleteAsync(GetDeleteHeadOptions.Options, url, key, cache, slidingExpiration, userState);
                    break;
                default:
                    throw new NotSupportedException(
                        "Unsupported web method: {0}".FormatWith(Method.ToUpper())
                        );
            }
        }
#endif

#if !WindowsPhone
        public virtual WebQueryAsyncResult RequestAsync(string url, 
                                                        IEnumerable<HttpPostParameter> parameters,
                                                        object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Put:
                    return ExecutePostOrPutAsync(PostOrPut.Put, url, parameters, userState);
                case WebMethod.Post:
                    return ExecutePostOrPutAsync(PostOrPut.Post, url, parameters, userState);
                default:
                    throw new NotSupportedException("Only HTTP POSTS can use multi-part forms");
            }
        }
#else
        public virtual void RequestAsync(string url,
                                         IEnumerable<HttpPostParameter> parameters,
                                         object userState)
        {
            UserState = userState;

            switch (Method)
            {
                case WebMethod.Put:
                    ExecutePostOrPutAsync(PostOrPut.Put, url, parameters, userState);
                    break;
                case WebMethod.Post:
                    ExecutePostOrPutAsync(PostOrPut.Post, url, parameters, userState);
                    break;
                default:
                    throw new NotSupportedException("Only HTTP POSTS can use multi-part forms");
            }
        }
#endif

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