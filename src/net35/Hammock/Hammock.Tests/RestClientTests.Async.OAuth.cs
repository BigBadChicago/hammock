using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Async")]
        [Category("OAuth")]
        public void Can_get_oauth_request_token_asynchronously()
        {
            var client = new RestClient
            {
                Authority = "http://twitter.com/oauth",
                Credentials = OAuthForTwitterRequestToken,
            };

            var request = new RestRequest
            {
                Path = "request_token"
            };

            var asyncResult = client.BeginRequest(request);
            var response = client.EndRequest(asyncResult);
            Assert.IsNotNull(response);
        }
    }
}
