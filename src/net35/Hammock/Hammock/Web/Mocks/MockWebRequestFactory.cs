using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Hammock.Extensions;

#if !SILVERLIGHT
using System.Web;
#else
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Web.Mocks
{
    public class MockWebRequestFactory : IWebRequestCreate
    {
        public const string MockScheme = "mockScheme";
        public const string MockStatusCode = "mockStatusCode";
        public const string MockStatusDescription = "mockStatusDescription";
        public const string MockContent = "mockContent";
        public const string MockContentType = "mockContentType";
        public const string MockHeaderNames = "mockHeaderNames";
        public const string MockHeaderValues = "mockHeaderValues";

        public WebRequest Create(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);

            var scheme = query[MockScheme];
            var statusCode = query[MockStatusCode];
            var statusDescription = query[MockStatusDescription];
            var content = query[MockContent];
            var contentType = query[MockContentType];
            var headerNames = query[MockHeaderNames];
            var headerValues = query[MockHeaderValues];

            // Remove mocks parameters
            var queryString = new NameValueCollection();
            foreach(var key in query.AllKeys)
            {
                if(key.EqualsAny(
                    MockScheme,
                    MockStatusCode,
                    MockStatusDescription,
                    MockContent,
                    MockContentType,
                    MockHeaderNames,
                    MockHeaderValues
                    ))
                {
                    continue;
                }
                queryString.Add(key, query[key]);
            }
            var uriQuery = queryString.ToQueryString();
            uri = new Uri("{0}://{1}{2}{3}".FormatWithInvariantCulture
                              (scheme, uri.Authority, uri.AbsolutePath, uriQuery)
                              );

            var request = new MockHttpWebRequest(uri);

            int statusCodeValue;
#if !NETCF
            int.TryParse(statusCode, out statusCodeValue);
#else
            try
            {
                statusCodeValue = int.Parse(statusCode);
            }
            catch (Exception)
            {
                statusCodeValue = 0;
            }
#endif

            if (!statusCode.IsNullOrBlank()) request.ExpectStatusCode = (HttpStatusCode)statusCodeValue;
            if (!statusDescription.IsNullOrBlank()) request.ExpectStatusDescription = statusDescription;
            if (!content.IsNullOrBlank()) request.Content = content;
            if (!contentType.IsNullOrBlank()) request.ContentType = contentType;

            if(!headerNames.IsNullOrBlank() && !headerValues.IsNullOrBlank())
            {
                var headers = new NameValueCollection();
                var names = headerNames.Split(',').Where(n => !n.IsNullOrBlank()).ToArray();
                var values = headerValues.Split(',').Where(v => !v.IsNullOrBlank()).ToArray();
                if(names.Count() == values.Count())
                {
                    for(var i = 0; i < names.Count(); i++)
                    {
                        headers.Add(names[i], values[i]);
                    }
                }

                foreach(var key in headers.AllKeys)
                {
                    request.ExpectHeaders.Add(key, headers[key]);
                }
            }
            return request;
        }
    }
}
