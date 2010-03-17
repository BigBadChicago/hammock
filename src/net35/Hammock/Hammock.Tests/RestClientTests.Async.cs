using System.Net;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Async")]
        public void Can_make_basic_auth_request_asynchronously()
        {
            var client = new RestClient
            {
                Credentials = BasicAuthForTwitter,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json"
            };

            var callback = new RestCallback(
                (req, resp) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);
                    }
                );

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        [Category("Async")]
        public void Can_make_basic_auth_request_with_headers_asynchronously()
        {
            var client = new RestClient
            {
                Credentials = BasicAuthForTwitter,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json"
            };

            client.AddHeader("Always", "on the client");
            request.AddHeader("Only", "on this request");

            var callback = new RestCallback(
                (req, resp) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);
                    });

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
