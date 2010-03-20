using Hammock.Caching;
using Hammock.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Caching")]
        [Category("Async")]
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
