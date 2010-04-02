using Hammock.Caching;
using Hammock.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
	{
        [Test]
        [Category("Caching")]
        public void Can_make_basic_auth_request_with_caching_synchronously()
        {
            var client = new RestClient
                             {
                                 Authority = "http://api.twitter.com",
                                 VersionPath = "1",
                             };

            var request = new RestRequest
                              {
                                  Credentials = BasicAuthForTwitter,
                                  Path = "statuses/home_timeline.json",
                                  Cache = CacheFactory.AspNetCache,
                                  CacheKeyFunction = () => _twitterUsername,
                                  CacheOptions = new CacheOptions
                                  {
                                      Duration = 10.Minutes(),
                                      Mode = CacheMode.AbsoluteExpiration
                                  }
                              };

            var first = client.Request(request);
            Assert.IsNotNull(first);
            Assert.IsFalse(first.IsFromCache, "First request was not served from the web.");

            var second = client.Request(request);
            Assert.IsNotNull(second);
            Assert.IsTrue(second.IsFromCache, "Second request was not served from cache.");
        }
	}
}
