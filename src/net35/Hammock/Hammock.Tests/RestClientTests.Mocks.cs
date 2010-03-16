using System.Net;
using Hammock.Extras;
using Hammock.Tests.Postmark;
using Hammock.Web.Mocks;
using NUnit.Framework;

namespace Hammock.Tests
{
	partial class RestClientTests
	{
        [Test]
        public void Can_request_with_mock_response()
        {
            var settings = GetSerializerSettings();
            var serializer = new HammockJsonDotNetSerializer(settings);

            // Mocking should be unique to RestRequest
            // Add IsMock to result
           
            var client = new RestClient
                             {
                                 Authority = "http://api.postmarkapp.com",
                                 Path = "email",
                                 Serializer = serializer,
                                 Deserializer = serializer
                             };

            var success = new PostmarkResponse
                                 {
                                     Status = PostmarkStatus.Success,
                                     Message = "OK"
                                 };

            var request = new RestRequest();
            request.ExpectHeader("Mock", "true");
            request.ExpectEntity = success;

            var response = client.Request<PostmarkResponse>(request);
            Assert.IsNotNull(response.ContentEntity);
            Assert.IsTrue(response.ContentEntity.Status == PostmarkStatus.Success);
        }

        [Test]
        public void Can_request_with_mock_response_with_entity()
        {
            
        }
	}
}
