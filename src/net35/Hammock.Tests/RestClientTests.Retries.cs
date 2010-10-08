using System;
using System.Net; 
using Hammock.Retries;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Retries")]
        public void Can_set_retry_policy()
        {
            var retryPolicy = new RetryPolicy { RetryCount = 5 };
            retryPolicy.RetryOn(new NetworkError(),
                                new Timeout(),
                                new ConnectionClosed());

            var client = new RestClient
                             {
                                 RetryPolicy = retryPolicy,
                                 Authority = "http://empty-journey-80.heroku.com",
                             };

            var request = new RestRequest
                              {
                                  Path = "/",
                                  Credentials = BasicAuthForTestService
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        public class IsFour : RetryCustomCondition<int>
        {
            public override Predicate<int> RetryIf
            {
                get
                {
                    return r => r == 4;
                }
            }
        }

        [Test]
        [Category("Retries")]
        public void Can_set_retry_policy_with_custom_condition()
        {
            var customCondition = new IsFour
                                      {
                                          ConditionFunction = () => 3
                                      };

            var retryPolicy = new RetryPolicy { RetryCount = 5 };
            retryPolicy.RetryOn(new NetworkError(),
                                new Timeout(),
                                new ConnectionClosed(),
                                customCondition
                                );

            var client = new RestClient
            {
                RetryPolicy = retryPolicy,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json",
                Credentials = OAuthForTwitterProtectedResource
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }
    }
}
