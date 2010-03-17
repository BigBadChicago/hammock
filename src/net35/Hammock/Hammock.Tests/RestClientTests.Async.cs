using Hammock.Caching;
using Hammock.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
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

            var success = false;
            var callback = new RestCallback(
                (req, resp) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);

                        success = true;
                    }
                );

            var asyncResult = client.BeginRequest(request, callback);
            asyncResult.AsyncWaitHandle.WaitOne();
            
            Assert.IsTrue(success);
        }

        [Test]
        public void Can_make_basic_auth_request_with_end_pattern_asynchronously()
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
                });

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
        }

        [Test]
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

            var success = false;
            var callback = new RestCallback(
                (req, resp) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);

                        success = true;
                    });

            var asyncResult = client.BeginRequest(request, callback);
            asyncResult.AsyncWaitHandle.WaitOne();

            Assert.IsTrue(success);
        }

        [Test]
        public void Can_make_basic_auth_request_with_caching_asynchronously()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Cache = CacheFactory.AspNetCache,
                CacheKeyFunction = () => _twitterUsername,
                CacheOptions = new CacheOptions
                {
                    Duration = 10.Minutes(),
                    Mode = CacheMode.AbsoluteExpiration
                }
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "statuses/home_timeline.json",
            };

            var firstResult = client.BeginRequest(request);
            var first = client.EndRequest(firstResult);
            Assert.IsNotNull(first);
            Assert.IsFalse(first.IsFromCache, "First request was not served from the web.");

            var secondResult = client.BeginRequest(request);
            var second = client.EndRequest(secondResult);
            Assert.IsNotNull(second);
            Assert.IsTrue(second.IsFromCache, "Second request was not served from cache.");
        }
    }
}
