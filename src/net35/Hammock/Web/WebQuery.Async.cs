using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Hammock.Caching;
using Hammock.Web.Mocks;
#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif
namespace Hammock.Web
{
    public partial class WebQuery
    {
        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method, string url, object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var state = new Triplet<WebRequest, object, object>
                            {
                                First = request,
                                Second = null,
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<string>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                {
                    CompletedSynchronously = true
                };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, string>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, string>
                                                 {
                                        First = cache,
                                        Second = key
                                    },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url, 
                                                            DateTime absoluteExpiration, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<string>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                                 {
                                     CompletedSynchronously = true,
                                     AsyncState = this
                                 };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, Pair<string, DateTime>>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, Pair<string, DateTime>>
                                                 {
                                                     First = cache,
                                                     Second = new Pair<string, DateTime>
                                                                  {
                                                                      First = key,
                                                                      Second = absoluteExpiration
                                                                  }
                                                 },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        private WebQueryAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                            string key, 
                                                            string url,
                                                            TimeSpan slidingExpiration, 
                                                            WebRequest request,
                                                            object userState)
        {
            var fetch = cache.Get<string>(key);
            if (fetch != null)
            {
                var args = new WebQueryResponseEventArgs(fetch);
                OnQueryResponse(args);

                var result = new WebQueryAsyncResult
                {
                    CompletedSynchronously = true
                };
                return result;
            }
            else
            {
                var state = new Triplet<WebRequest, Pair<ICache, Pair<string, TimeSpan>>, object>
                                {
                                    First = request,
                                    Second = new Pair<ICache, Pair<string, TimeSpan>>
                                                 {
                                                     First = cache,
                                                     Second = new Pair<string, TimeSpan>
                                                                  {
                                                                      First = key,
                                                                      Second = slidingExpiration
                                                                  }
                                                 },
                                    Third = userState
                                };

                var args = new WebQueryRequestEventArgs(url);
                OnQueryRequest(args);

                var inner = request.BeginGetResponse(GetAsyncResponseCallback, state);
                RegisterAbortTimer(request, inner); 
                var result = new WebQueryAsyncResult { InnerResult = inner };
                return result;
            }
        }

        protected virtual void RegisterAbortTimer(WebRequest request, IAsyncResult asyncResult)
        {
#if SILVERLIGHT
            var timeout = RequestTimeout != null ? 
                (int)RequestTimeout.Value.TotalMilliseconds
                : 0;
#else
            var timeout = request.Timeout;
#endif 

#if !Smartphone && !WindowsPhone
            // Async operations ignore the WebRequest's Timeout property
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
                                                   GetAsyncResponseTimeout,
                                                   request, timeout,
                                                   true);
#endif
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method,
                                                                      string url, 
                                                                      string prefixKey, 
                                                                      ICache cache,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, request, userState);
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method, 
                                                                      string url, 
                                                                      string prefixKey, 
                                                                      ICache cache, 
                                                                      DateTime absoluteExpiration,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, absoluteExpiration, request, userState);
        }

        protected virtual WebQueryAsyncResult ExecuteGetOrDeleteAsync(GetDeleteHeadOptions method,
                                                                      string url, 
                                                                      string prefixKey,
                                                                      ICache cache,
                                                                      TimeSpan slidingExpiration,
                                                                      object userState)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(method, url);
            var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, slidingExpiration, request, userState);
        }
        
        private void GetAsyncResponseTimeout(object state, bool timedOut)
        {
            if (!timedOut)
            {
                return;
            }

            var request = state as WebRequest;
            if (request != null)
            {
                TimedOut = true;
                request.Abort();
            }
        }

        protected virtual void GetAsyncResponseCallback(IAsyncResult asyncResult)
        {
            object store;
            var request = GetAsyncCacheStore(asyncResult, out store);

            try
            {
                var response = request.EndGetResponse(asyncResult);
                using (response)
                {
#if SILVERLIGHT
                    if(DecompressionMethods == DecompressionMethods.GZip || 
                       DecompressionMethods == DecompressionMethods.Deflate)
                    {
                        response = new GzipHttpWebResponse((HttpWebResponse)response);
                    }
#endif

                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = reader.ReadToEnd();

                            WebResponse = response;

                            if (store != null)
                            {
                                // No expiration specified
                                if (store is Pair<ICache, string>)
                                {
                                    var cache = store as Pair<ICache, string>;
                                    cache.First.Insert(cache.Second, result);
                                }

                                // Absolute expiration specified
                                if (store is Pair<ICache, Pair<string, DateTime>>)
                                {
                                    var cache = store as Pair<ICache, Pair<string, DateTime>>;
                                    cache.First.Insert(cache.Second.First, result, cache.Second.Second);
                                }

                                // Sliding expiration specified
                                if (store is Pair<ICache, Pair<string, TimeSpan>>)
                                {
                                    var cache = store as Pair<ICache, Pair<string, TimeSpan>>;
                                    cache.First.Insert(cache.Second.First, result, cache.Second.Second);
                                }
                            }

                            // Only send query when caching is complete
                            var args = new WebQueryResponseEventArgs(result);
                            OnQueryResponse(args);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var result = HandleWebException(ex);

                var responseArgs = new WebQueryResponseEventArgs(result) { Exception = ex };

                OnQueryResponse(responseArgs);
            }
        }

        private static WebRequest GetAsyncCacheStore(IAsyncResult asyncResult, out object store)
        {
            WebRequest request;

            var noCache = asyncResult.AsyncState as Triplet<WebRequest, object, object>;
            if(noCache != null)
            {
                request = noCache.First;
                store = noCache.Second;
            }
            else
            {
                var absoluteCache = asyncResult.AsyncState as Triplet<WebRequest, Pair<ICache, Pair<string, DateTime>>, object>;
                if(absoluteCache != null)
                {
                    request = absoluteCache.First;
                    store = absoluteCache.Second;
                }
                else
                {
                    var slidingCache = asyncResult.AsyncState as Triplet<WebRequest, Pair<ICache, Pair<string, TimeSpan>>, object>;
                    if(slidingCache != null)
                    {
                        request = slidingCache.First;
                        store = slidingCache.Second;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            "asyncResult", "Wrong cache signature found."
                            );
                    }
                }
            }
            return request;
        }

        private bool _isStreaming;
        public virtual bool IsStreaming
        {
            get
            {
                return _isStreaming;
            }
        }
        public virtual bool TimedOut { get; set; }
        private void GetAsyncStreamCallback(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as Pair<WebRequest, Pair<TimeSpan, int>>;
            if (state == null)
            {
                // Unrecognized state signature
                throw new ArgumentNullException("asyncResult",
                                                "The asynchronous post failed to return its state");
            }

            var request = state.First;
            var duration = state.Second.First;
            var resultCount = state.Second.Second;

            WebResponse response = null;
            Stream stream = null;

            try
            {
                using (response = request.EndGetResponse(asyncResult))
                {
#if SILVERLIGHT
                    if (DecompressionMethods == DecompressionMethods.GZip ||
                       DecompressionMethods == DecompressionMethods.Deflate)
                    {
                        response = new GzipHttpWebResponse((HttpWebResponse)response);
                    }
#endif
                    using (stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            _isStreaming = true;

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
                                        EndStreaming(request);
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
                                    results.Clear();

                                    count = 0;

                                    var now = DateTime.UtcNow;
                                    if (now.Subtract(start) < duration)
                                    {
                                        continue;
                                    }

                                    // Time elapsed
                                    EndStreaming(request);
                                    return;
                                }

                                // Stream dried up
                            }
                            EndStreaming(request);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var result = HandleWebException(ex);

                var responseArgs = new WebQueryResponseEventArgs(result);
                OnQueryResponse(responseArgs);
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

        private void EndStreaming(WebRequest request)
        {
            _isStreaming = false;
            var endArgs = new WebQueryResponseEventArgs("END STREAMING");
            OnQueryResponse(endArgs);
            request.Abort();
        }

        protected virtual void PostAsyncRequestCallback(IAsyncResult asyncResult)
        {
            WebRequest request;
            byte[] content;
            object userState;
            Triplet<ICache, object, string> store;

            var state = asyncResult.AsyncState as Triplet<WebRequest, byte[], object>;
            if (state == null)
            {
                // No expiration specified
                if (asyncResult is Triplet<WebRequest, Triplet<byte[], ICache, string>, object>)
                {
                    var cacheScheme = (Triplet<WebRequest, Triplet<byte[], ICache, string>, object>)asyncResult;
                    var cache = cacheScheme.Second.Second;

                    var url = cacheScheme.First.RequestUri.ToString();
                    var prefix = cacheScheme.Second.Third;
                    var key = CreateCacheKey(prefix, url);

                    var fetch = cache.Get<string>(key);
                    if (fetch != null)
                    {
                        var args = new WebQueryResponseEventArgs(fetch);
                        OnQueryResponse(args);
                        return;
                    }

                    request = cacheScheme.First;
                    content = cacheScheme.Second.First;
                    userState = cacheScheme.Third;
                    store = new Triplet<ICache, object, string>
                                {
                                    First = cache,
                                    Second = null,
                                    Third = prefix
                                };
                }
                else
                    // Absolute expiration specified
                    if (asyncResult is Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>)
                    {
                        var cacheScheme = (Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>)asyncResult;
                        var url = cacheScheme.First.RequestUri.ToString();
                        var cache = cacheScheme.Second.Second.First;
                        var expiry = cacheScheme.Second.Second.Second;

                        var prefix = cacheScheme.Second.Second.Third;
                        var key = CreateCacheKey(prefix, url);

                        var fetch = cache.Get<string>(key);
                        if (fetch != null)
                        {
                            var args = new WebQueryResponseEventArgs(fetch);
                            OnQueryResponse(args);
                            return;
                        }

                        request = cacheScheme.First;
                        content = cacheScheme.Second.First;
                        userState = cacheScheme.Third;
                        store = new Triplet<ICache, object, string>
                                    {
                                        First = cache,
                                        Second = expiry,
                                        Third = prefix
                                    };
                    }
                    else
                        // Sliding expiration specified
                        if (asyncResult is Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>)
                        {
                            var cacheScheme = (Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>)asyncResult;
                            var url = cacheScheme.First.RequestUri.ToString();
                            var cache = cacheScheme.Second.Second.First;
                            var expiry = cacheScheme.Second.Second.Second;

                            var prefix = cacheScheme.Second.Second.Third;
                            var key = CreateCacheKey(prefix, url);

                            var fetch = cache.Get<string>(key);
                            if (fetch != null)
                            {
                                var args = new WebQueryResponseEventArgs(fetch);
                                OnQueryResponse(args);
                                return;
                            }

                            request = cacheScheme.First;
                            content = cacheScheme.Second.First;
                            userState = cacheScheme.Third;
                            store = new Triplet<ICache, object, string>
                                        {
                                            First = cache,
                                            Second = expiry,
                                            Third = prefix
                                        };
                        }
                        else
                        {
                            // Unrecognized state signature
                            throw new ArgumentNullException("asyncResult",
                                                            "The asynchronous post failed to return its state");
                        }
            }
            else
            {
                request = state.First;
                content = state.Second;
                userState = state.Third;
                store = null;
            }

            // No cached response
            using (var stream = request.EndGetRequestStream(asyncResult))
            {
                if (content != null)
                {
                    stream.Write(content, 0, content.Length);
                    stream.Flush();
                }
                stream.Close();

                request.BeginGetResponse(PostAsyncResponseCallback,
                                         new Triplet<WebRequest, Triplet<ICache, object, string>, object>
                                             {
                                                 First = request,
                                                 Second = store,
                                                 Third = userState
                                             });
            }
        }

        protected virtual void PostAsyncResponseCallback(IAsyncResult asyncResult)
        {
            var state = asyncResult.AsyncState as Triplet<WebRequest, Triplet<ICache, object, string>, object>;
            if (state == null)
            {
                throw new ArgumentNullException("asyncResult", 
                                                "The asynchronous post failed to return its state");
            }

            var request = state.First;
            if (request == null)
            {
                throw new ArgumentNullException("asyncResult", 
                                                "The asynchronous post failed to return a request");
            }

            try
            {
                // Avoid disposing until no longer needed to build results
                var response = request.EndGetResponse(asyncResult);

#if SILVERLIGHT
                if (DecompressionMethods == DecompressionMethods.GZip ||
                    DecompressionMethods == DecompressionMethods.Deflate)
                {
                    response = new GzipHttpWebResponse((HttpWebResponse)response);
                }
#endif
                WebResponse = response;

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    if (state.Second != null)
                    {
                        var cache = state.Second.First;
                        var expiry = state.Second.Second;
                        var url = request.RequestUri.ToString();

                        var prefix = state.Second.Third;
                        var key = CreateCacheKey(prefix, url);

                        if (expiry is DateTime)
                        {
                            // absolute
                            cache.Insert(key, result, (DateTime) expiry);
                        }

                        if (expiry is TimeSpan)
                        {
                            // sliding
                            cache.Insert(key, result, (TimeSpan) expiry);
                        }
                    }

                    var args = new WebQueryResponseEventArgs(result);
                    OnQueryResponse(args);
                }
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, string url, object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, byte[], object>
                            {
                                First = request,
                                Second = content,
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            RegisterAbortTimer(request, inner);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method,
                                                                    string url,
                                                                    IEnumerable<HttpPostParameter> parameters,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;

            var request = BuildMultiPartFormRequest(method, url, parameters, out content);
            var state = new Triplet<WebRequest, byte[], object>
                            {
                                First = request,
                                Second = content,
                                Third = userState
                            };
            var args = new WebQueryRequestEventArgs(url);

            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult {InnerResult = inner};
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey,
                                                                    ICache cache,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Triplet<byte[], ICache, string>, object>
                            {
                                First = request,
                                Second = new Triplet<byte[], ICache, string>
                                             {
                                                 First = content,
                                                 Second = cache,
                                                 Third = prefixKey
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey, 
                                                                    ICache cache, 
                                                                    DateTime absoluteExpiration,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>, object>
                            {
                                First = request,
                                Second = new Pair<byte[], Triplet<ICache, DateTime, string>>
                                             {
                                                 First = content,
                                                 Second = new Triplet<ICache, DateTime, string>
                                                              {
                                                                  First = cache,
                                                                  Second = absoluteExpiration,
                                                                  Third = prefixKey
                                                              }
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        protected virtual WebQueryAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                                    string url, 
                                                                    string prefixKey,
                                                                    ICache cache, 
                                                                    TimeSpan slidingExpiration,
                                                                    object userState)
        {
            WebResponse = null;

            byte[] content;
            var request = BuildPostOrPutWebRequest(method, url, out content);

            var state = new Triplet<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>, object>
                            {
                                First = request,
                                Second = new Pair<byte[], Triplet<ICache, TimeSpan, string>>
                                             {
                                                 First = content,
                                                 Second = new Triplet<ICache, TimeSpan, string>
                                                              {
                                                                  First = cache,
                                                                  Second = slidingExpiration,
                                                                  Third = prefixKey
                                                              }
                                             },
                                Third = userState
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetRequestStream(PostAsyncRequestCallback, state);
            RegisterAbortTimer(request, inner); 
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        public virtual WebQueryAsyncResult ExecuteStreamGetAsync(string url, 
                                                                TimeSpan duration, 
                                                                int resultCount)
        {
            WebResponse = null;

            var request = BuildGetDeleteHeadOptionsWebRequest(GetDeleteHeadOptions.Get, url);

            var state = new Pair<WebRequest, Pair<TimeSpan, int>>
                            {
                                First = request,
                                Second = new Pair<TimeSpan, int>
                                             {
                                                 First = duration,
                                                 Second = resultCount
                                             }
                            };

            var args = new WebQueryRequestEventArgs(url);
            OnQueryRequest(args);

            var inner = request.BeginGetResponse(GetAsyncStreamCallback, state);
            var result = new WebQueryAsyncResult { InnerResult = inner };
            return result;
        }

        private object ResponseAsHttpWebResponse(out int statusCode, 
                                                 out string statusDescription, 
                                                 out string contentType, 
                                                 out long contentLength, 
                                                 out Uri responseUri, 
                                                 out System.Net.WebHeaderCollection headers)
        {
            var httpWebResponse = WebResponse != null && WebResponse is HttpWebResponse
                                      ? (HttpWebResponse) WebResponse
                                      : null;

            if(httpWebResponse == null)
            {
                statusCode = 0;
                statusDescription = null;
                contentType = null;
                contentLength = 0;
                responseUri = null;
                headers = null;
                return null;
            }

            statusCode = Convert.ToInt32(httpWebResponse.StatusCode, CultureInfo.InvariantCulture);
            statusDescription = httpWebResponse.StatusDescription;
            contentType = httpWebResponse.ContentType;
            contentLength = httpWebResponse.ContentLength;
            responseUri = httpWebResponse.ResponseUri;
            headers = httpWebResponse.Headers;
            return httpWebResponse;
        }

        private object ResponseAsMockHttpWebResponse(out int statusCode, 
                                                     out string statusDescription, 
                                                     out string contentType, 
                                                     out long contentLength, 
                                                     out Uri responseUri, 
                                                     out System.Net.WebHeaderCollection headers)
        {
            var httpWebResponse = WebResponse != null && WebResponse is MockHttpWebResponse
                                      ? (MockHttpWebResponse)WebResponse
                                      : null;

            if (httpWebResponse == null)
            {
                statusCode = 0;
                statusDescription = null;
                contentType = null;
                contentLength = 0;
                responseUri = null;
                headers = null;
                return null;
            }

            statusCode = Convert.ToInt32(httpWebResponse.StatusCode, CultureInfo.InvariantCulture);
            statusDescription = httpWebResponse.StatusDescription;
            contentType = httpWebResponse.ContentType;
            contentLength = httpWebResponse.ContentLength;
            responseUri = httpWebResponse.ResponseUri;
            headers = httpWebResponse.Headers;
            return httpWebResponse;
        }

        private void CastWebResponse(out int statusCode, 
                                     out string statusDescription, 
                                     out System.Net.WebHeaderCollection headers,
                                     out string contentType, 
                                     out long contentLength, 
                                     out Uri responseUri)
        {
            var response = ResponseAsHttpWebResponse(
                out statusCode, out statusDescription, 
                out contentType, out contentLength, 
                out responseUri, out headers
                );
            if (response != null)
            {
                return;
            }

            response = ResponseAsMockHttpWebResponse(
                out statusCode, out statusDescription,
                out contentType, out contentLength,
                out responseUri, out headers
                );

            // [DC]: Caching would result in a null response
            if(response == null)
            {
               headers = new System.Net.WebHeaderCollection(); 
            }
        }
    }
}