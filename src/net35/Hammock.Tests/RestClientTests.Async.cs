using System.Net;
using Hammock.Extras;
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

        [Test]
        public void Can_use_client_standalone_asynchronously()
        {
            var callback = new RestCallback(
                (req, resp) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                });

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Path = "statuses/public_timeline.json"
            };

            var asyncResult = client.BeginRequest(callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Can_use_client_standalone_with_type_asynchronously()
        {
            var callback = new RestCallback<TwitterRateLimitStatus>(
                (req, resp) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                });

            var settings = GetSerializerSettings();
            var serializer = new HammockJsonDotNetSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Path = "account/rate_limit_status.json",
                Serializer = serializer,
                Deserializer = serializer
            };

            var asyncResult = client.BeginRequest(callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ContentEntity);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }
    }
}
