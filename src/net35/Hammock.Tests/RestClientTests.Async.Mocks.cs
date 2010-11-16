using System.Collections.Specialized;
using System.Net;
using Hammock.Extras.Serialization;
using Hammock.Tests.Postmark;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Mocks")]
        [Category("Async")]
        public void Can_request_with_mock_response_with_request_entity_asynchronously()
        {
            var settings = GetSerializerSettings();

            var message = new PostmarkMessage
            {
                From = _postmarkFromAddress,
                To = _postmarkToAddress,
                Subject = "Test passed!",
                TextBody = "Hello from the Hammock unit tests!",
                Headers = new NameValueCollection
                                                {
                                                    {"Email-Header", "Shows up on an email"},
                                                    {"Email-Header", "Shows up on an email, too"}
                                                }
            };

            var serializer = new JsonSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.postmarkapp.com",
                Serializer = serializer,
                Deserializer = serializer
            };
            client.AddHeader("Accept", "application/json");
            client.AddHeader("Content-Type", "application/json; charset=utf-8");
            client.AddHeader("X-Postmark-Server-Token", _postmarkServerToken);
            client.AddHeader("User-Agent", "Hammock");

            var request = new RestRequest
            {
                Path = "email",
                Entity = message
            };

            // Mocking triggers
            var success = new PostmarkResponse
            {
                Status = PostmarkStatus.Success,
                Message = "OK"
            };
            request.ExpectStatusCode = (HttpStatusCode)200;
            request.ExpectEntity = success;
            request.ExpectHeader("Mock", "true");

            var asyncResult = client.BeginRequest<PostmarkResponse>(request);
            var response = client.EndRequest<PostmarkResponse>(asyncResult);
            var result = response.ContentEntity;

            Assert.IsNotNull(response);
            Assert.IsNotNull(result);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.IsMock);
        }
    }
}
