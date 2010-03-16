using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Retries;
using Hammock.Tasks;
using Hammock.Web;
#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System.Collections.Specialized;
using Hammock.Web.Mocks;

#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestClient : RestBase, IRestClient
    {
        public virtual string Authority { get; set; }

        private bool _firstTry = true;
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

        private WebQuery RequestImpl(RestRequest request)
        {
            var uri = request.BuildEndpoint(this);
            var query = GetQueryFor(request, uri);
            SetQueryMeta(request, query);

            // rate-limiting
            // recurring tasks
            // requestimpl for async
            // mock trigger

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
                    WebRequest.RegisterPrefix("mock", new MockWebRequestFactory());
                    url = url.Replace("https", "mock").Replace("http", "mock");
                    
                    if(request.ExpectStatusCode.HasValue)
                    {
                        query.Parameters.Add("mockStatusCode", ((int)request.ExpectStatusCode.Value).ToString());
                        if(request.ExpectStatusDescription.IsNullOrBlank())
                        {
                            query.Parameters.Add("mockStatusDescription", request.ExpectStatusCode.ToString());
                        }
                    }
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

        private static bool RequestExpectsMock(RestRequest request)
        {
            return request.ExpectEntity != null ||
                   request.ExpectHeaders.Count > 0 ||
                   request.ExpectStatusCode.HasValue ||
                   !request.ExpectContent.IsNullOrBlank() ||
                   !request.ExpectContentType.IsNullOrBlank() ||
                   !request.ExpectStatusDescription.IsNullOrBlank();
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

        public virtual IAsyncResult BeginRequest(RestRequest request, RestCallback callback)
        {
            var uri = request.BuildEndpoint(this);
            var query = GetQueryFor(request, uri);

            var taskOptions = GetTaskOptions(request);
            if(taskOptions != null)
            {
                if (taskOptions.RepeatInterval > TimeSpan.Zero)
                {
                    // Tasks without rate limiting
                    _task = new TimedTask(TimeSpan.Zero, 
                                          taskOptions.RepeatInterval, 
                                          taskOptions.RepeatTimes,
                                          true, skip => BeginRequestImpl(request, callback, query, uri));

                    // Tasks with rate limiting
                }
            }

            throw new NotImplementedException("Don't have an IAsyncResult from timed task yet");

            // Normal operation
            BeginRequestImpl(request, callback, query, uri);
        }

        public virtual IAsyncResult BeginRequestImpl(RestRequest request, RestCallback callback, WebQuery query, Uri uri)
        {
            query.QueryResponse += (sender, args) =>
            {
                var response = BuildResponseFromResult(request, query);
                callback.Invoke(request, response);
            };

            return query.RequestAsync(uri.ToString());
        }

        public virtual IAsyncResult BeginRequest<T>(RestRequest request, RestCallback<T> callback)
        {
            var uri = request.BuildEndpoint(this);
            var query = GetQueryFor(request, uri);

            query.QueryResponse += (sender, args) =>
            {
                var response = BuildResponseFromResult<T>(request, query);
                callback.Invoke(request, response);
            };
           
            return query.RequestAsync(uri.ToString());
        }
        
        private RestResponse BuildResponseFromResult(RestRequest request, WebQuery query)
        {
            var result = query.Result;
            var response = BuildBaseResponse(result);

            DeserializeEntityBody(result, request, response);

            return response;
        }

        private RestResponse<T> BuildResponseFromResult<T>(RestBase request, WebQuery query)
        {
            var result = query.Result;
            var response = BuildBaseResponse<T>(result);

            DeserializeEntityBody(result, request, response);

            return response;
        }

        private static RestResponse BuildBaseResponse(WebQueryResult result)
        {
            var response = new RestResponse
                       {
                           StatusCode = (HttpStatusCode)result.ResponseHttpStatusCode,
                           StatusDescription = result.ResponseHttpStatusDescription,
                           Content = result.Response,
                           ContentType = result.ResponseType,
                           ContentLength = result.ResponseLength,
                           ResponseUri = result.ResponseUri,
                       };

            return response;
        }

        private static RestResponse<T> BuildBaseResponse<T>(WebQueryResult result)
        {
            var response = new RestResponse<T>
            {
                StatusCode = (HttpStatusCode)result.ResponseHttpStatusCode,
                StatusDescription = result.ResponseHttpStatusDescription,
                Content = result.Response,
                ContentType = result.ResponseType,
                ContentLength = result.ResponseLength,
                ResponseUri = result.ResponseUri,
            };

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
            // mocks

            // [DC]: Trump duplicates by request over client value
            query.Parameters.AddRange(Parameters);
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
            
            SerializeEntityBody(query, request);
        }

        private void SerializeEntityBody(WebQuery query, RestRequest request)
        {
            var serializer = request.Serializer ?? Serializer;
            if (serializer == null)
            {
                // No suitable serializer for entity
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

            return query;
        }
    }
}