using System;
using System.Net;
using System.Web;
using Hammock.Extensions;

namespace Hammock.Web.Mocks
{
    public class MockWebRequestFactory : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            var query = HttpUtility.ParseQueryString(uri.Query);

            var statusCode = query["mockStatusCode"];
            var statusDescription = query["mockStatusDescription"];
            var content = query["mockContent"];
            var contentType = query["mockContentType"];

            var request = new MockHttpWebRequest(uri);

            // Should be the real deal
            if (!statusCode.IsNullOrBlank()) request.ExpectStatusCode = statusCode;
            if (!statusDescription.IsNullOrBlank()) request.ExpectStatusDescription = statusDescription;
            if (!content.IsNullOrBlank()) request.ExpectContent = content;
            if (!contentType.IsNullOrBlank()) request.ExpectContentType = contentType;

            return request;
        }
    }
}
