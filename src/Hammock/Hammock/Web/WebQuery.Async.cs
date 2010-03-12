using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Hammock.Caching;

namespace Hammock.Web.Query
{
	public partial class WebQuery
	{
	    protected virtual IAsyncResult ExecuteGetOrDeleteAsync(GetOrDelete method, string url)
	    {
	        WebResponse = null;

	        var request = BuildGetOrDeleteWebRequest(method, url);
            var state = new Pair<WebRequest, object>
                            {
                                First = request,
                                Second = null
                            };

	        var args = new WebQueryRequestEventArgs(url);
	        OnQueryRequest(args);

	        return request.BeginGetResponse(GetAsyncResponseCallback, state);
	    }

	    private IAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                     string key, 
                                                     string url, 
                                                     WebRequest request)
	    {
	        var fetch = cache.Get<string>(key);

	        if (fetch != null)
	        {
	            var args = new WebQueryResponseEventArgs(fetch);
	            OnQueryResponse(args);

	            return null;
	        }
	        else
	        {
	            var state = new Pair<ICache, string>
	                            {
	                                First = cache,
	                                Second = key
	                            };

	            var args = new WebQueryRequestEventArgs(url);
	            OnQueryRequest(args);

	            return request.BeginGetRequestStream(GetAsyncResponseCallback, state);
	        }
	    }

	    private IAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
                                                     string key, 
                                                     string url, 
                                                     DateTime absoluteExpiration, 
                                                     WebRequest request)
	    {
	        var fetch = cache.Get<string>(key);

	        if (fetch != null)
	        {
	            var args = new WebQueryResponseEventArgs(fetch);
	            OnQueryResponse(args);

                return null;
	        }
	        else
	        {
	            var state = new Pair<ICache, Pair<string, DateTime>>
	                            {
	                                First = cache,
	                                Second = new Pair<string, DateTime>
	                                             {
	                                                 First = key,
	                                                 Second = absoluteExpiration
	                                             }
	                            };

	            var args = new WebQueryRequestEventArgs(url);
	            OnQueryRequest(args);

                return request.BeginGetRequestStream(GetAsyncResponseCallback, state);
	        }
	    }

	    private IAsyncResult ExecuteGetOrDeleteAsync(ICache cache, 
	                                                 string key, 
	                                                 string url,
	                                                 TimeSpan slidingExpiration, 
	                                                 WebRequest request)
	    {
	        var fetch = cache.Get<string>(key);

	        if (fetch != null)
	        {
	            var args = new WebQueryResponseEventArgs(fetch);
	            OnQueryResponse(args);

	            return null;
	        }
	        else
	        {
	            var state = new Pair<ICache, Pair<string, TimeSpan>>
	                            {
	                                First = cache,
	                                Second = new Pair<string, TimeSpan>
	                                             {
	                                                 First = key,
	                                                 Second = slidingExpiration
	                                             }
	                            };

	            var args = new WebQueryRequestEventArgs(url);
	            OnQueryRequest(args);

	            return request.BeginGetResponse(GetAsyncResponseCallback, state);
	        }
	    }

        protected virtual IAsyncResult ExecuteGetOrDeleteAsync(GetOrDelete method,
                                                               string url, 
                                                               string prefixKey, 
                                                               ICache cache)
	    {
	        WebResponse = null;

	        var request = BuildGetOrDeleteWebRequest(method, url);
	        var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, request);

            /*
	        ThreadPool.QueueUserWorkItem(work =>
	            ExecuteGetAsyncAndCache(cache, key, url, request)
	            );
            */
	    }

        protected virtual IAsyncResult ExecuteGetOrDeleteAsync(GetOrDelete method, 
                                                               string url, 
                                                               string prefixKey, 
                                                               ICache cache, 
                                                               DateTime absoluteExpiration)
	    {
	        WebResponse = null;

            var request = BuildGetOrDeleteWebRequest(method, url);
	        var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, absoluteExpiration, request);

            /*
	        ThreadPool.QueueUserWorkItem(work =>
	            ExecuteGetAsyncAndCacheWithExpiry(cache, key, url, absoluteExpiration, request)
	            );
            */
	    }

	    protected virtual IAsyncResult ExecuteGetOrDeleteAsync(GetOrDelete method,
                                                               string url, 
                                                               string prefixKey,
                                                               ICache cache,
                                                               TimeSpan slidingExpiration)
	    {
	        WebResponse = null;

	        var request = BuildGetOrDeleteWebRequest(method, url);
	        var key = CreateCacheKey(prefixKey, url);

            return ExecuteGetOrDeleteAsync(cache, key, url, slidingExpiration, request);

            /*
	        ThreadPool.QueueUserWorkItem( work =>
	            ExecuteGetAsyncAndCacheWithExpiry(cache, key, url, slidingExpiration, request)
	            );
            */
	    }
        
        private static void GetAsyncResponseTimeout(object state, bool timedOut)
        {
            if (!timedOut)
            {
                return;
            }

            var request = state as WebRequest;
            if (request != null)
            {
                request.Abort();
            }
        }

        protected virtual void GetAsyncResponseCallback(IAsyncResult asyncResult)
        {
            // [DC]: Second parameter is fuzzy cache
            var state = (Pair<WebRequest, object>)asyncResult.AsyncState;
            var request = state.First;
            var store = state.Second;

#if !Smartphone && !Silverlight
            // Async operations ignore the WebRequest's Timeout property
            ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle,
                                                   GetAsyncResponseTimeout,
                                                   request, request.Timeout,
                                                   true);
#endif

            using (var response = (HttpWebResponse)request.EndGetResponse(asyncResult))
            {
                try
                {
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
                catch (WebException ex)
                {
                    var result = HandleWebException(ex);

                    var responseArgs = new WebQueryResponseEventArgs(result);
                    OnQueryResponse(responseArgs);
                }
            }
        }

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
                    using (stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
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
                            request.Abort();
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

	    protected virtual void PostAsyncRequestCallback(IAsyncResult asyncResult)
	    {
	        WebRequest request;
	        byte[] content;
	        Triplet<ICache, object, string> store;

	        var state = asyncResult.AsyncState as Pair<WebRequest, byte[]>;
	        if (state == null)
	        {
	            // No expiration specified
	            if (asyncResult is Pair<WebRequest, Triplet<byte[], ICache, string>>)
	            {
	                var cacheScheme = (Pair<WebRequest, Triplet<byte[], ICache, string>>) asyncResult;
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
	                store = new Triplet<ICache, object, string>
	                            {
	                                First = cache,
	                                Second = null,
	                                Third = prefix
	                            };
	            }
	            else
	                // Absolute expiration specified
	                if (asyncResult is Pair<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>>)
	                {
	                    var cacheScheme = (Pair<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>>) asyncResult;
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
	                    store = new Triplet<ICache, object, string>
	                                {
	                                    First = cache,
	                                    Second = expiry,
	                                    Third = prefix
	                                };
	                }
	                else
	                    // Sliding expiration specified
	                    if (asyncResult is Pair<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>>)
	                    {
	                        var cacheScheme = (Pair<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>>) asyncResult;
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
	            store = null;
	        }

	        // no cached response
	        using (var stream = request.EndGetRequestStream(asyncResult))
	        {
	            if (content != null)
	            {
	                stream.Write(content, 0, content.Length);
	            }
	            stream.Close();

	            request.BeginGetResponse(PostAsyncResponseCallback,
	                                     new Pair<WebRequest, Triplet<ICache, object, string>>
	                                         {First = request, Second = store});
	        }
	    }

	    protected virtual void PostAsyncResponseCallback(IAsyncResult asyncResult)
	    {
	        var state = asyncResult.AsyncState as Pair<WebRequest, Triplet<ICache, object, string>>;
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

	    protected virtual IAsyncResult ExecutePostOrPutAsync(PostOrPut method, string url)
	    {
	        WebResponse = null;

	        byte[] content;
	        var request = BuildPostOrPutWebRequest(method, url, out content);

	        var state = new Pair<WebRequest, byte[]> {First = request, Second = content};

	        var args = new WebQueryRequestEventArgs(url);
	        OnQueryRequest(args);

	        return request.BeginGetRequestStream(PostAsyncRequestCallback, state);
	    }

	    protected virtual IAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                             string url,
	                                                         IEnumerable<HttpPostParameter> parameters)
	    {
	        WebResponse = null;

	        byte[] content;

	        var request = BuildMultiPartFormRequest(method, url, parameters, out content);
	        var state = new Pair<WebRequest, byte[]> {First = request, Second = content};
	        var args = new WebQueryRequestEventArgs(url);

	        OnQueryRequest(args);

	        return request.BeginGetRequestStream(PostAsyncRequestCallback, state);
	    }

	    protected virtual IAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                             string url, 
                                                             string prefixKey,
	                                                         ICache cache)
	    {
	        WebResponse = null;

	        byte[] content;
	        var request = BuildPostOrPutWebRequest(method, url, out content);

	        var state = new Pair<WebRequest, Triplet<byte[], ICache, string>>
	                        {
	                            First = request,
	                            Second = new Triplet<byte[], ICache, string>
	                                         {
	                                             First = content,
	                                             Second = cache,
	                                             Third = prefixKey
	                                         }
	                        };

	        var args = new WebQueryRequestEventArgs(url);
	        OnQueryRequest(args);

	        return request.BeginGetRequestStream(PostAsyncRequestCallback, state);
	    }

	    protected virtual IAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                             string url, 
                                                             string prefixKey, 
                                                             ICache cache, 
                                                             DateTime absoluteExpiration)
	    {
	        WebResponse = null;

	        byte[] content;
	        var request = BuildPostOrPutWebRequest(method, url, out content);

	        var state = new Pair<WebRequest, Pair<byte[], Triplet<ICache, DateTime, string>>>
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
	                                         }
	                        };

	        var args = new WebQueryRequestEventArgs(url);
	        OnQueryRequest(args);

	        return request.BeginGetRequestStream(PostAsyncRequestCallback, state);
	    }

	    protected virtual IAsyncResult ExecutePostOrPutAsync(PostOrPut method, 
                                                             string url, 
                                                             string prefixKey,
	                                                         ICache cache, 
                                                             TimeSpan slidingExpiration)
	    {
	        WebResponse = null;

	        byte[] content;
	        var request = BuildPostOrPutWebRequest(method, url, out content);

	        var state = new Pair<WebRequest, Pair<byte[], Triplet<ICache, TimeSpan, string>>>
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
	                                         }
	                        };

	        var args = new WebQueryRequestEventArgs(url);
	        OnQueryRequest(args);

	        return request.BeginGetRequestStream(PostAsyncRequestCallback, state);
	    }

	    public virtual IAsyncResult ExecuteStreamGetAsync(string url, 
                                                          TimeSpan duration, 
                                                          int resultCount)
	    {
	        WebResponse = null;

	        var request = BuildGetOrDeleteWebRequest(GetOrDelete.Get, url);

	        var state = new Pair<WebRequest, Pair<TimeSpan, int>>
	                        {
	                            First = request,
	                            Second = new Pair<TimeSpan, int>
	                                         {
	                                             First = duration,
	                                             Second = resultCount
	                                         }
	                        };

	        return request.BeginGetResponse(GetAsyncStreamCallback, state);
        }
	    
	}
}
