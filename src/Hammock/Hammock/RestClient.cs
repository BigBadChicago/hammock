using System;
using System.Net;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Extensions;
using Hammock.Query;
using Hammock.Web;
using Hammock.Web.Query.Basic;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System.Collections.Specialized;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestClient : RestBase, IRestClient
    {
        public virtual string Authority { get; set; }

        protected internal HammockQueryInfo Info { get; private set; }

        public RestClient()
        {
            Headers = new NameValueCollection(0);
            Parameters = new WebParameterCollection();
            Info = new HammockQueryInfo();
        }

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

            // retries
            // exceptions
            // multi-part

            SetQueryMeta(request, query);

            var url = uri.ToString();

            if(!RequestWithCache(request, query, url))
            {
                WebException exception;
                query.Request(url, out exception);
            }

            return query;
        }

        private bool RequestWithCache(RestBase request, WebQuery query, string url)
        {
            var cache = GetCache(request);
            if (Cache == null)
            {
                return false;
            }

            var options = GetCacheOptions(request);
            if (options == null)
            {
                return false;
            }

            // [DC]: This is currently prefixed to the full URL
            var function = GetCacheKeyFunction(request);
            var key = function != null ? function.Invoke() : "";

            WebException exception;
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

        public virtual IAsyncResult BeginRequest(RestRequest request, RestCallback callback)
        {
            var uri = request.BuildEndpoint(this);
            var query = GetQueryFor(request, uri);
            
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
            var response = new RestResponse<T>
            {
                StatusCode = (HttpStatusCode)result.ResponseHttpStatusCode,
                StatusDescription = result.ResponseHttpStatusDescription,
                Content = result.Response,
                ContentType = result.ResponseType,
                ContentLength = result.ResponseLength,
                ResponseUri = result.ResponseUri
            };

            DeserializeEntityBody(result, request, response);

            return response;
        }

        private static RestResponse BuildBaseResponse(WebQueryResult result)
        {
            return new RestResponse
                       {
                           StatusCode = (HttpStatusCode)result.ResponseHttpStatusCode,
                           StatusDescription = result.ResponseHttpStatusDescription,
                           Content = result.Response,
                           ContentType = result.ResponseType,
                           ResponseUri = result.ResponseUri,
                       };
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

            query.Parameters.AddRange(Parameters);
            query.Parameters.AddRange(request.Parameters);
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
            
            // [DC]: UserAgent is set via Info
            // [DC]: Request credentials trump client credentials
            var query = credentials != null
                            ? credentials.GetQueryFor(uri.ToString(), request, Info, method)
                            : new BasicAuthWebQuery(Info);

            return query;
        }
    }
}