using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Retries;
using Hammock.Web.Mocks;
using Hammock.Serialization;
using Hammock.Tasks;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestClient : RestBase, IRestClient
    {
        private const string MockContentType = "mockContentType";
        private const string MockScheme = "mockScheme";
        private const string MockProtocol = "mock";
        private const string MockStatusDescription = "mockStatusDescription";
        private const string MockContent = "mockContent";
        public virtual string Authority { get; set; }

#if SILVERLIGHT
        public virtual bool HasElevatedPermissions { get; set; }
#endif

#if !Silverlight
        private bool _firstTry = true;
#endif
        private int _remainingRetries;
        private TimedTask _task;

#if !Silverlight
        public virtual RestResponse Request(RestRequest request)
        {
            var query = RequestImpl(request);

            return BuildResponseFromResult(request, query);
        }

        public virtual RestResponse<T> Request<T>(RestRequest request)
        {
            var query = RequestImpl(request);

            return BuildResponseFromResult<T>(request, query);
        }

        public RestResponse Request()
        {
            var query = RequestImpl(null);

            return BuildResponseFromResult(null, query);
        }

        public RestResponse<T> Request<T>()
        {
            var query = RequestImpl(null);

            return BuildResponseFromResult<T>(null, query);
        }

        private WebQuery RequestImpl(RestRequest request)
        {
            request = request ?? new RestRequest();
            var uri = request.BuildEndpoint(this);
            var query = GetQueryFor(request, uri);
            SetQueryMeta(request, query);

            var retryPolicy = GetRetryPolicy(request);
            if (_firstTry)
            {
                _remainingRetries = (retryPolicy != null ? retryPolicy.RetryCount : 0) + 1;
                _firstTry = false;
            }

            WebQueryResult previous = null;
            while (_remainingRetries > 0)
            {
                var url = uri.ToString();
                if (RequestExpectsMock(request))
                {
                    url = BuildMockRequestUrl(request, query, url);
                }

                WebException exception;
                if (!RequestWithCache(request, query, url, out exception) &&
                    !RequestMultiPart(request, query, url, out exception))
                {
                    query.Request(url, out exception);
                }

                query.Result.Exception = exception;
                query.Result.PreviousResult = previous;
                var current = query.Result;
               
                var retry = false;
                if(retryPolicy != null)
                {
                    foreach(RetryErrorCondition condition in retryPolicy.RetryConditions)
                    {
                        if(exception == null)
                        {
                            continue;
                        }
                        retry |= condition.RetryIf(exception);
                    }

                    if(retry)
                    {
                        previous = current;
                        _remainingRetries--;
                    }
                    else
                    {
                        _remainingRetries = 0;
                    }
                }
                else
                {
                    _remainingRetries = 0;
                }

                query.Result = current;
            }

            _firstTry = _remainingRetries == 0;
            return query;
        }

        private bool RequestMultiPart(RestBase request, WebQuery query, string url, out WebException exception)
        {
            var parameters = GetPostParameters(request);
            if(parameters == null || parameters.Count() == 0)
            {
                exception = null;
                return false;
            }

            // [DC]: Default to POST if no method provided
            query.Method = query.Method != WebMethod.Post && Method != WebMethod.Put ? WebMethod.Post : query.Method;
            query.Request(url, parameters, out exception);
            return true;
        }

        private bool RequestWithCache(RestBase request, WebQuery query, string url, out WebException exception)
        {
            var cache = GetCache(request);
            if (Cache == null)
            {
                exception = null;
                return false;
            }

            var options = GetCacheOptions(request);
            if (options == null)
            {
                exception = null;
                return false;
            }

            // [DC]: This is currently prefixed to the full URL
            var function = GetCacheKeyFunction(request);
            var key = function != null ? function.Invoke() : "";

            switch (options.Mode)
            {
                case CacheMode.NoExpiration:
                    query.Request(url, key, cache, out exception);
                    break;
                case CacheMode.AbsoluteExpiration:
                    var expiry = options.Duration.FromNow();
                    query.Request(url, key, cache, expiry, out exception);
                    break;
                case CacheMode.SlidingExpiration:
                    query.Request(url, key, cache, options.Duration, out exception);
                    break;
                default:
                    throw new NotSupportedException("Unknown CacheMode");
            }

            return true;
        }
#endif
        private string BuildMockRequestUrl(RestRequest request, WebQuery query, string url)
        {
            WebRequest.RegisterPrefix(MockProtocol, new MockWebRequestFactory());
            if (url.Contains("https"))
            {
                url = url.Replace("https", MockProtocol);
                query.Parameters.Add(MockScheme, "https");
            }
            if (url.Contains("http"))
            {
                url = url.Replace("http", MockProtocol);
                query.Parameters.Add(MockScheme, "http");
            }

            if (request.ExpectStatusCode.HasValue)
            {
                query.Parameters.Add("mockStatusCode", ((int)request.ExpectStatusCode.Value).ToString());
                if (request.ExpectStatusDescription.IsNullOrBlank())
                {
                    query.Parameters.Add(MockStatusDescription, request.ExpectStatusCode.ToString());
                }
            }
            if (!request.ExpectStatusDescription.IsNullOrBlank())
            {
                query.Parameters.Add(MockStatusDescription, request.ExpectStatusDescription);
            }

            var entity = SerializeExpectEntity(request);
            if (entity != null)
            {
                query.Parameters.Add(MockContent, entity.Content);
                query.Parameters.Add(MockContentType, entity.ContentType);
            }
            else
            {
                if (!request.ExpectContent.IsNullOrBlank())
                {
                    query.Parameters.Add(MockContent, request.ExpectContent);
                    query.Parameters.Add(MockContentType,
                                         !request.ExpectContentType.IsNullOrBlank()
                                             ? request.ExpectContentType
                                             : "text/html"
                        );
                }
                else
                {
                    if (!request.ExpectContentType.IsNullOrBlank())
                    {
                        query.Parameters.Add(
                            MockContentType, request.ExpectContentType
                            );
                    }
                }
            }

            if (request.ExpectHeaders.Count > 0)
            {
                var names = new StringBuilder();
                var values = new StringBuilder();
                var count = 0;
                foreach (var key in request.ExpectHeaders.AllKeys)
                {
                    names.Append(key);
                    values.Append(request.ExpectHeaders[key].Value);
                    count++;
                    if (count < request.ExpectHeaders.Count)
                    {
                        names.Append(",");
                        values.Append(",");
                    }
                }

                query.Parameters.Add("mockHeaderNames", names.ToString());
                query.Parameters.Add("mockHeaderValues", values.ToString());
            }
            return url;
        }

        private static bool RequestExpectsMock(RestRequest request)
        {
            return request.ExpectEntity != null ||
                   request.ExpectHeaders.Count > 0 ||
                   request.ExpectStatusCode.HasValue ||
                   !request.ExpectContent.IsNullOrBlank() ||
                   !request.ExpectContentType.IsNullOrBlank() ||
                   !request.ExpectStatusDescription.IsNullOrBlank();
        }

        private ICache GetCache(RestBase request)
        {
            return request.Cache ?? Cache;
        }

        private IEnumerable<HttpPostParameter> GetPostParameters(RestBase request)
        {
            if(request.PostParameters != null)
            {
                foreach(var parameter in request.PostParameters)
                {
                    yield return parameter;
                }
            }

            if (PostParameters == null)
            {
                yield break;
            }

            foreach (var parameter in PostParameters)
            {
                yield return parameter;
            }
        }

        private CacheOptions GetCacheOptions(RestBase request)
        {
            return request.CacheOptions ?? CacheOptions;
        }

        private Func<string> GetCacheKeyFunction(RestBase request)
        {
            return request.CacheKeyFunction ?? CacheKeyFunction;
        }

        private string GetProxy(RestBase request)
        {
            return request.Proxy ?? Proxy;
        }

        private string GetUserAgent(RestBase request)
        {
            var userAgent = request.UserAgent.IsNullOrBlank()
                                ? UserAgent
                                : request.UserAgent;
            return userAgent;
        }

        private ISerializer GetSerializer(RestBase request)
        {
            return request.Serializer ?? Serializer;
        }

        private IWebCredentials GetWebCredentials(RestBase request)
        {
            var credentials = request.Credentials ?? Credentials;
            return credentials;
        }

        private IWebQueryInfo GetInfo(RestBase request)
        {
            var info = request.Info ?? Info;
            return info;
        }

        private TimeSpan? GetTimeout(RestBase request)
        {
            return request.Timeout ?? Timeout;
        }

        private WebMethod GetWebMethod(RestBase request)
        {
            var method = !request.Method.HasValue
                             ? !Method.HasValue
                                   ? WebMethod.Get
                                   : Method.Value
                             : request.Method.Value;

            return method;
        }

        private RetryPolicy GetRetryPolicy(RestBase request)
        {
            var policy = request.RetryPolicy ?? RetryPolicy;
            return policy;
        }
        
        private TaskOptions GetTaskOptions(RestBase request)
        {
            var options = request.TaskOptions ?? TaskOptions;
            return options;
        }

        private object GetTag(RestBase request)
        {
            var tag = request.Tag ?? Tag;
            return tag;
        }

        public virtual IAsyncResult BeginRequest(RestRequest request, RestCallback callback)
        {
            return BeginRequest(request, callback, null, null, false /* isInternal */);
        }

        public virtual IAsyncResult BeginRequest<T>(RestRequest request, RestCallback<T> callback)
        {
            return BeginRequest(request, callback, null, null, false /* isInternal */);
        }

        public IAsyncResult BeginRequest(RestCallback callback)
        {
            return BeginRequest(null, callback, null, null, false /* isInternal */);
        }

        public IAsyncResult BeginRequest(RestRequest request)
        {
            return BeginRequest(request, null);
        }

        public IAsyncResult BeginRequest<T>(RestRequest request)
        {
            return BeginRequest<T>(request, null);
        }

        public IAsyncResult BeginRequest<T>(RestCallback<T> callback)
        {
            return BeginRequest(null, callback);
        }

        // Pattern: http://msdn.microsoft.com/en-us/library/ms228963.aspx
        public RestResponse EndRequest(IAsyncResult result)
        {
            var webResult = EndRequestImpl(result);
            return webResult.AsyncState as RestResponse;
        }

        public RestResponse<T> EndRequest<T>(IAsyncResult result)
        {
            var webResult = EndRequestImpl<T>(result);
            return webResult.AsyncState as RestResponse<T>;
        }

        private WebQueryAsyncResult EndRequestImpl(IAsyncResult result)
        {
            var webResult = result as WebQueryAsyncResult;
            if (webResult == null)
            {
                throw new InvalidOperationException("The IAsyncResult provided was not for this operation.");
            }

            var tag = (Pair<RestRequest, RestCallback>)webResult.Tag;

            if (RequestExpectsMock(tag.First))
            {
                // [DC]: Mock results come via InnerResult
                webResult = (WebQueryAsyncResult)webResult.InnerResult;
            }

            if (webResult.CompletedSynchronously)
            {
                var query = webResult.AsyncState as WebQuery;
                if (query != null)
                {
                    // [DC]: From cache
                    CompleteWithQuery(query, tag.First, tag.Second, webResult);
                }
                else
                {
                    // [DC]: From mocks
                    webResult = CompleteWithMockWebResponse(result, webResult, tag);
                }
            }

            if (!webResult.IsCompleted)
            {
                webResult.AsyncWaitHandle.WaitOne();
            }
            return webResult;
        }

        private WebQueryAsyncResult EndRequestImpl<T>(IAsyncResult result)
        {
            var webResult = result as WebQueryAsyncResult;
            if (webResult == null)
            {
                throw new InvalidOperationException("The IAsyncResult provided was not for this operation.");
            }

            var tag = (Pair<RestRequest, RestCallback<T>>)webResult.Tag;

            if (RequestExpectsMock(tag.First))
            {
                // [DC]: Mock results come via InnerResult
                webResult = (WebQueryAsyncResult)webResult.InnerResult;
            }

            if (webResult.CompletedSynchronously)
            {
                var query = webResult.AsyncState as WebQuery;
                if(query != null)
                {
                    // [DC]: From cache
                    CompleteWithQuery(query, tag.First, tag.Second, webResult);
                }
                else
                {
                    // [DC]: From mocks
                    webResult = CompleteWithMockWebResponse(result, webResult, tag);
                }
            }

            if (!webResult.IsCompleted)
            {
                webResult.AsyncWaitHandle.WaitOne();
            }
            return webResult;
        }

        private WebQueryAsyncResult CompleteWithMockWebResponse<T>(
            IAsyncResult result, 
            IAsyncResult webResult, 
            Pair<RestRequest, 
            RestCallback<T>> tag)
        {
            var webResponse = (WebResponse)webResult.AsyncState;
            var restRequest = tag.First;
            
            string content;
            using(var stream = webResponse.GetResponseStream())
            {
                using(var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }

            var restResponse = new RestResponse<T>
                                   {
                                       Content = content,
                                       ContentType = webResponse.ContentType,
                                       ContentLength = webResponse.ContentLength,
                                       StatusCode = restRequest.ExpectStatusCode.HasValue
                                                        ? restRequest.ExpectStatusCode.Value
                                                        : 0,
                                       StatusDescription = restRequest.ExpectStatusDescription,
                                       ResponseUri = webResponse.ResponseUri,
                                       IsMock = true
                                   };

            foreach (var key in webResponse.Headers.AllKeys)
            {
                restResponse.Headers.Add(key, webResponse.Headers[key]);
            }
            
            var deserializer = restRequest.Deserializer ?? Deserializer;
            if (deserializer != null && !restResponse.Content.IsNullOrBlank())
            {
                restResponse.ContentEntity = deserializer.Deserialize<T>(restResponse.Content);
            }

            TraceResponseWithMock(restResponse);
                    
            var parentResult = (WebQueryAsyncResult)result;
            parentResult.AsyncState = restResponse;
            parentResult.IsCompleted = true;
                    
            var callback = tag.Second;
            if (callback != null)
            {
                callback.Invoke(restRequest, restResponse);
            }
            parentResult.Signal();
            return parentResult;
        }

        private WebQueryAsyncResult CompleteWithMockWebResponse(
            IAsyncResult result, 
            IAsyncResult webResult, 
            Pair<RestRequest, RestCallback> tag)
        {
            var webResponse = (WebResponse)webResult.AsyncState;
            var restRequest = tag.First;
            
            string content;
            using (var stream = webResponse.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }

            var restResponse = new RestResponse
                                   {
                                       Content = content,
                                       ContentType = webResponse.ContentType,
                                       ContentLength = webResponse.ContentLength,
                                       StatusCode = restRequest.ExpectStatusCode.HasValue
                                                        ? restRequest.ExpectStatusCode.Value
                                                        : 0,
                                       StatusDescription = restRequest.ExpectStatusDescription,
                                       ResponseUri = webResponse.ResponseUri,
                                       IsMock = true
                                   };

            foreach(var key in webResponse.Headers.AllKeys)
            {
                restResponse.Headers.Add(key, webResponse.Headers[key]);
            }

            var deserializer = restRequest.Deserializer ?? Deserializer;
            if (deserializer != null && !restResponse.Content.IsNullOrBlank() && restRequest.ResponseEntityType != null)
            {
                restResponse.ContentEntity = deserializer.Deserialize(restResponse.Content, restRequest.ResponseEntityType);
            }

            TraceResponseWithMock(restResponse);

            var parentResult = (WebQueryAsyncResult)result;
            parentResult.AsyncState = restResponse;
            parentResult.IsCompleted = true;

            var callback = tag.Second;
            if (callback != null)
            {
                callback.Invoke(restRequest, restResponse);
            }
            parentResult.Signal();
            return parentResult;
        }

        private static void TraceResponseWithMock(RestResponseBase restResponse)
        {
#if TRACE
            Trace.WriteLine(String.Concat(
                "RESPONSE: ", restResponse.StatusCode, " ", restResponse.StatusDescription)
                );
            Trace.WriteLineIf(restResponse.Headers.AllKeys.Count() > 0, "HEADERS:");
            foreach (var trace in restResponse.Headers.AllKeys.Select(
                key => String.Concat("\t", key, ": ", restResponse.Headers[key])))
            {
                Trace.WriteLine(trace);
            }
            Trace.WriteLine(String.Concat(
                "BODY: ", restResponse.Content)
                );
#endif
        }


        // TODO BeginRequest and BeginRequest<T> have too much duplication
        private IAsyncResult BeginRequest(RestRequest request, 
                                          RestCallback callback,
                                          WebQuery query,
                                          string url,
                                          bool isInternal)
        {
            request = request ?? new RestRequest();
            if (!isInternal)
            {
                // [DC]: Recursive call possible, only do this once
                var uri = request.BuildEndpoint(this);
                query = GetQueryFor(request, uri);
                SetQueryMeta(request, query);
                url = uri.ToString();
            }
            
            if (RequestExpectsMock(request))
            {
                url = BuildMockRequestUrl(request, query, url);
            }

            var retryPolicy = GetRetryPolicy(request);
            _remainingRetries = (retryPolicy != null
                                     ? retryPolicy.RetryCount
                                     : 0);

            Func<WebQueryAsyncResult> beginRequest
                = () => BeginRequestFunction(isInternal, 
                        request, 
                        query, 
                        url, 
                        callback);

            var result = beginRequest.Invoke();

            WebQueryResult previous = null;
            query.QueryResponse += (sender, args) =>
            {
                query.Result.PreviousResult = previous;
                var current = query.Result;

                var retry = false;
                if (retryPolicy != null)
                {
                    // [DC]: Query should already have exception applied
                    var exception = query.Result.Exception;

                    // Known error retries
                    foreach (RetryErrorCondition condition in retryPolicy.RetryConditions)
                    {
                        if (exception == null)
                        {
                            continue;
                        }
                        retry |= condition.RetryIf(exception);
                    }

                    // Generic unknown retries?
                    // todo

                    if (retry)
                    {
                        previous = current;
                        BeginRequest(request, callback, query, url, true);
                        Interlocked.Decrement(ref _remainingRetries);
                    }
                    else
                    {
                        _remainingRetries = 0;
                    }
                }
                else
                {
                    _remainingRetries = 0;
                }

                query.Result = current;

                // [DC]: Callback is for a final result, not a retry
                if (_remainingRetries == 0)
                {
                    CompleteWithQuery(query, request, callback, result);
                }
            };

            return result;
        }

        private WebQueryAsyncResult BeginRequestFunction(bool isInternal,
                                                         RestRequest request,
                                                         WebQuery query,
                                                         string url,
                                                         RestCallback callback)
        {
            WebQueryAsyncResult result;
            if (!isInternal)
            {
                if (!BeginRequestWithTask(request, callback, query, url, out result))
                {
                    if (!BeginRequestWithCache(request, query, url, out result))
                    {
                        if (!BeginRequestMultiPart(request, query, url, out result))
                        {
                            // Normal operation
                            result = query.RequestAsync(url);
                        }
                    }
                }
            }
            else
            {
                // Normal operation
                result = query.RequestAsync(url);
            }

            result.Tag = new Pair<RestRequest, RestCallback>
                             {
                                 First = request,
                                 Second = callback
                             };
        
            return result;
        }
        private IAsyncResult BeginRequest<T>(RestRequest request,
                                             RestCallback<T> callback,
                                             WebQuery query,
                                             string url,
                                             bool isInternal)
        {
            request = request ?? new RestRequest();
            if (!isInternal)
            {
                var uri = request.BuildEndpoint(this);
                query = GetQueryFor(request, uri);
                SetQueryMeta(request, query);
                url = uri.ToString();
            }

            if (RequestExpectsMock(request))
            {
                url = BuildMockRequestUrl(request, query, url);
            }

            var retryPolicy = GetRetryPolicy(request);
            _remainingRetries = (retryPolicy != null
                                     ? retryPolicy.RetryCount
                                     : 0);

            Func<WebQueryAsyncResult> beginRequest
                = () => BeginRequestFunction(
                    isInternal, 
                    request, 
                    query, 
                    url, 
                    callback);

            var result = beginRequest.Invoke();

            WebQueryResult previous = null;
            query.QueryResponse += (sender, args) =>
            {
                query.Result.PreviousResult = previous;
                var current = query.Result;

                var retry = false;
                if (retryPolicy != null)
                {
                    // [DC]: Query should already have exception applied
                    var exception = query.Result.Exception;
                    foreach (RetryErrorCondition condition in retryPolicy.RetryConditions)
                    {
                        if (exception == null)
                        {
                            continue;
                        }
                        retry |= condition.RetryIf(exception);
                    }

                    if (retry)
                    {
                        previous = current;
                        BeginRequest(request, callback, query, url, true);
                        Interlocked.Decrement(ref _remainingRetries);
                    }
                    else
                    {
                        _remainingRetries = 0;
                    }
                }
                else
                {
                    _remainingRetries = 0;
                }

                query.Result = current;

                // [DC]: Callback is for a final result, not a retry
                if (_remainingRetries == 0)
                {
                    CompleteWithQuery(query, request, callback, result);
                }
            };

            return result;
        }

        private void CompleteWithQuery<T>(WebQuery query, RestRequest request, RestCallback<T> callback, WebQueryAsyncResult result)
        {
            var response = BuildResponseFromResult<T>(request, query);
            result.AsyncState = response;
            result.IsCompleted = true;
            if(callback != null)
            {
                callback.Invoke(request, response);
            }
            result.Signal();
        }

        private void CompleteWithQuery(WebQuery query, RestRequest request, RestCallback callback, WebQueryAsyncResult result)
        {
            var response = BuildResponseFromResult(request, query);
            result.AsyncState = response;
            result.IsCompleted = true;
            if (callback != null)
            {
                callback.Invoke(request, response);
            }
            result.Signal();
        }

        private WebQueryAsyncResult BeginRequestFunction<T>(bool isInternal, RestRequest request, WebQuery query, string url, RestCallback<T> callback)
        {
            WebQueryAsyncResult result;
            if (!isInternal)
            {
                if (!BeginRequestWithTask(request, callback, query, url, out result))
                {
                    if (!BeginRequestWithCache(request, query, url, out result))
                    {
                        if (!BeginRequestMultiPart(request, query, url, out result))
                        {
                            // Normal operation
                            result = query.RequestAsync(url);
                        }
                    }
                }
            }
            else
            {
                // Normal operation
                result = query.RequestAsync(url);
            }

            result.Tag = new Pair<RestRequest, RestCallback<T>>
            {
                First = request,
                Second = callback
            };
            return result;
        }

        private bool BeginRequestWithTask(RestRequest request,
                                          RestCallback callback,
                                          WebQuery query,
                                          string url,
                                          out WebQueryAsyncResult result)
        {
            var taskOptions = GetTaskOptions(request);
            if (taskOptions == null)
            {
                result = null;
                return false;
            }

            if (taskOptions.RepeatInterval <= TimeSpan.Zero)
            {
                result = null;
                return false;
            }

#if !NETCF
            if (!taskOptions.GetType().IsGenericType)
            {
#endif
                // Tasks without rate limiting
                _task = new TimedTask(taskOptions.DueTime,
                                      taskOptions.RepeatInterval,
                                      taskOptions.RepeatTimes,
                                      taskOptions.ContinueOnError,
                                      skip => BeginRequest(request,
                                                           callback,
                                                           query,
                                                           url,
                                                           true));
#if !NETCF
            }
            else
            {
                // Tasks with rate limiting
                var task = BuildRateLimitingTask(request,
                                                 taskOptions,
                                                 callback,
                                                 query,
                                                 url);

                _task = (TimedTask) task;
            }
#endif
            var action = new Action(
                () => _task.Start()
                );

            var inner = action.BeginInvoke(ar =>
                                            {
                                                /* No callback */
                                            }, null);
            result = new WebQueryAsyncResult { InnerResult = inner };
            return true;
        }

        private bool BeginRequestWithTask<T>(RestRequest request,
                                          RestCallback<T> callback,
                                          WebQuery query,
                                          string url,
                                          out WebQueryAsyncResult result)
        {
            var taskOptions = GetTaskOptions(request);
            if (taskOptions == null)
            {
                result = null;
                return false;
            }

            if (taskOptions.RepeatInterval <= TimeSpan.Zero)
            {
                result = null;
                return false;
            }

#if !NETCF
            if (!taskOptions.GetType().IsGenericType)
            {
#endif
                // Tasks without rate limiting
                _task = new TimedTask(taskOptions.DueTime,
                                      taskOptions.RepeatInterval,
                                      taskOptions.RepeatTimes,
                                      taskOptions.ContinueOnError,
                                      skip => BeginRequest(request,
                                                           callback,
                                                           query,
                                                           url,
                                                           true));
#if !NETCF
            }
            else
            {
                // Tasks with rate limiting
                var task = BuildRateLimitingTask(request,
                                                 taskOptions,
                                                 callback,
                                                 query,
                                                 url);

                _task = (TimedTask) task;
            }
#endif
            var action = new Action(
                () => _task.Start()
                );

            var inner = action.BeginInvoke(ar =>
                                               {
                                                   /* No callback */
                                               }, null);
            result = new WebQueryAsyncResult {InnerResult = inner};
            return true;
        }

#if !NETCF
        private object BuildRateLimitingTask(RestRequest request,
                                            ITaskOptions taskOptions,
                                            RestCallback callback,
                                            WebQuery query,
                                            string url)
        {
            var taskAction = new Action<bool>(skip => BeginRequest(request, callback, query, url, true));

            return BuildRateLimitingTaskImpl(taskOptions, taskAction);
        }

        private object BuildRateLimitingTask<T>(RestRequest request,
                                            ITaskOptions taskOptions,
                                            RestCallback<T> callback,
                                            WebQuery query,
                                            string url)
        {
            var taskAction = new Action<bool>(skip => BeginRequest(request, 
                                                                   callback, 
                                                                   query, 
                                                                   url, 
                                                                   true));

            return BuildRateLimitingTaskImpl(taskOptions, taskAction);
        }

        private static object BuildRateLimitingTaskImpl(ITaskOptions taskOptions, 
                                                        Action<bool> taskAction)
        {
            var innerType = taskOptions.GetDeclaredTypeForGeneric(typeof(ITaskOptions<>));
            var rateType = typeof(RateLimitingRule<>).MakeGenericType(innerType);
            var taskType = typeof(TimedTask<>).MakeGenericType(innerType);
            var rateLimitingType = (RateLimitType)taskOptions.GetValue("RateLimitType");
                
            object taskRule;
            switch(rateLimitingType)
            {
                case RateLimitType.ByPercent:
                    var rateLimitingPercent = taskOptions.GetValue("RateLimitPercent");
                    taskRule = Activator.CreateInstance(
                        rateType, rateLimitingPercent
                        );
                    break;
                case RateLimitType.ByPredicate:
                    var rateLimitingPredicate = taskOptions.GetValue("RateLimitIf");
                    taskRule = Activator.CreateInstance(
                        rateType, rateLimitingPredicate
                        );
                    var getRateLimitStatus = taskOptions.GetValue("GetRateLimitStatus");
                    if (getRateLimitStatus != null)
                    {
                        rateType.SetValue("GetRateLimitStatus", getRateLimitStatus);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Activator.CreateInstance(taskType,
                                            taskOptions.DueTime, 
                                            taskOptions.RepeatInterval, 
                                            taskOptions.RepeatTimes,
                                            taskOptions.ContinueOnError,
                                            taskAction,
                                            taskRule);
        }
#endif

        private bool BeginRequestMultiPart(RestBase request,
                                           WebQuery query, 
                                           string url, 
                                           out WebQueryAsyncResult result)
        {
            var parameters = GetPostParameters(request);
            if (parameters == null || parameters.Count() == 0)
            {
                result = null;
                return false;
            }

            // [DC]: Default to POST if no method provided
            query.Method = query.Method != WebMethod.Post && Method != WebMethod.Put ? WebMethod.Post : query.Method;
            result = query.RequestAsync(url, parameters);
            return true;
        }

        private bool BeginRequestWithCache(RestBase request,
                                           WebQuery query, 
                                           string url, 
                                           out WebQueryAsyncResult result)
        {
            var cache = GetCache(request);
            if (Cache == null)
            {
                result = null;
                return false;
            }

            var options = GetCacheOptions(request);
            if (options == null)
            {
                result = null;
                return false;
            }

            // [DC]: This is currently prefixed to the full URL
            var function = GetCacheKeyFunction(request);
            var key = function != null ? function.Invoke() : "";
            
            switch (options.Mode)
            {
                case CacheMode.NoExpiration:
                    result = query.RequestAsync(url, key, cache);
                    break;
                case CacheMode.AbsoluteExpiration:
                    var expiry = options.Duration.FromNow();
                    result = query.RequestAsync(url, key, cache, expiry);
                    break;
                case CacheMode.SlidingExpiration:
                    result = query.RequestAsync(url, key, cache, options.Duration);
                    break;
                default:
                    throw new NotSupportedException("Unknown CacheMode");
            }

            return true;
        }
        
        private RestResponse BuildResponseFromResult(RestRequest request, WebQuery query)
        {
            request = request ?? new RestRequest();
            var result = query.Result;
            var response = BuildBaseResponse(result);

            DeserializeEntityBody(result, request, response);
            response.Tag = GetTag(request);

            return response;
        }
        private RestResponse<T> BuildResponseFromResult<T>(RestBase request, WebQuery query)
        {
            request = request ?? new RestRequest();
            var result = query.Result;
            var response = BuildBaseResponse<T>(result);

            DeserializeEntityBody(result, request, response);
            response.Tag = GetTag(request);

            return response;
        }

        private static readonly Func<RestResponseBase, WebQueryResult, RestResponseBase> _baseSetter =
                (response, result) =>
                {
                    response.InnerResponse = result.WebResponse;
                    response.InnerException = result.Exception;
                    response.RequestDate = result.RequestDate;
                    response.RequestUri = result.RequestUri;
                    response.RequestMethod = result.RequestHttpMethod;
                    response.RequestKeptAlive = result.RequestKeptAlive;
                    response.ResponseDate = result.ResponseDate;
                    response.ResponseUri = result.ResponseUri;
                    response.StatusCode = (HttpStatusCode)result.ResponseHttpStatusCode;
                    response.StatusDescription = result.ResponseHttpStatusDescription;
                    response.Content = result.Response;
                    response.ContentType = result.ResponseType;
                    response.ContentLength = result.ResponseLength;
                    response.IsMock = result.IsMock;
                    return response;
                };

        private static RestResponse BuildBaseResponse(WebQueryResult result)
        {
            var response = new RestResponse();

            _baseSetter.Invoke(response, result);

            return response;
        }

        private static RestResponse<T> BuildBaseResponse<T>(WebQueryResult result)
        {
            var response = new RestResponse<T>();

            _baseSetter.Invoke(response, result);

            return response;
        }

        private void DeserializeEntityBody(WebQueryResult result, RestRequest request, RestResponse response)
        {
            var deserializer = request.Deserializer ?? Deserializer;
            if(deserializer != null && !result.Response.IsNullOrBlank() && request.ResponseEntityType != null)
            {
                response.ContentEntity = deserializer.Deserialize(result.Response, request.ResponseEntityType);
            }
        }
        private void DeserializeEntityBody<T>(WebQueryResult result, RestBase request, RestResponse<T> response)
        {
            var deserializer = request.Deserializer ?? Deserializer;
            if (deserializer != null && !result.Response.IsNullOrBlank())
            {
                response.ContentEntity = deserializer.Deserialize<T>(result.Response);
            }
        }

        private void SetQueryMeta(RestRequest request, WebQuery query)
        {
            // [DC]: Trump duplicates by request over client over info values
            foreach (var parameter in Parameters)
            {
                if (query.Parameters[parameter.Name] != null)
                {
                    query.Parameters[parameter.Name].Value = parameter.Value;
                }
                else
                {
                    query.Parameters.Add(parameter);
                }
            }
            foreach(var parameter in request.Parameters)
            {
                if(query.Parameters[parameter.Name] != null)
                {
                    query.Parameters[parameter.Name].Value = parameter.Value;
                }
                else
                {
                    query.Parameters.Add(parameter);
                }
            }
            query.Headers.AddRange(Headers);
            query.Headers.AddRange(request.Headers);

            // [DC]: These properties are trumped by request over client
            query.UserAgent = GetUserAgent(request);
            query.Method = GetWebMethod(request);
            query.Proxy = GetProxy(request);
            query.RequestTimeout = GetTimeout(request);
            query.DecompressionMethods = request.DecompressionMethods | DecompressionMethods;
            
            SerializeEntityBody(query, request);
        }

        private void SerializeEntityBody(WebQuery query, RestRequest request)
        {
            var serializer = GetSerializer(request);
            if (serializer == null)
            {
                // No suitable serializer for entity
                return;
            }

            if(request.Entity == null || request.RequestEntityType == null)
            {
                // Not enough information to serialize
                return;
            }

            var entityBody = serializer.Serialize(request.Entity, request.RequestEntityType);
            query.Entity = !entityBody.IsNullOrBlank()
                               ? new WebEntity
                                     {
                                         Content = entityBody,
                                         ContentEncoding = serializer.ContentEncoding,
                                         ContentType = serializer.ContentType
                                     }
                               : null;
        }

        private WebEntity SerializeExpectEntity(RestRequest request)
        {
            var serializer = GetSerializer(request);
            if (serializer == null || request.ExpectEntity == null)
            {
                // No suitable serializer or entity
                return null;
            }

            var entityBody = serializer.Serialize(request.ExpectEntity, request.RequestEntityType);
            var entity = !entityBody.IsNullOrBlank()
                               ? new WebEntity
                               {
                                   Content = entityBody,
                                   ContentEncoding = serializer.ContentEncoding,
                                   ContentType = serializer.ContentType
                               } : null;
            return entity;
        }

        private WebQuery GetQueryFor(RestBase request, Uri uri)
        {
            var method = GetWebMethod(request);
            var credentials = GetWebCredentials(request);
            var info = GetInfo(request);
            
            // [DC]: UserAgent is set via Info
            // [DC]: Request credentials trump client credentials
            var query = credentials != null
                            ? credentials.GetQueryFor(uri.ToString(), request, info, method)
                            : new BasicAuthWebQuery(info);

#if SILVERLIGHT
            query.HasElevatedPermissions = HasElevatedPermissions;
#endif
            return query;
        }
    }
}