using System;
using System.Threading;
using Hammock.Retries;
using NUnit.Framework;
using Timeout = Hammock.Retries.Timeout;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        private class AlwaysRetry : RetryCustomCondition<object>
        {
            public override Predicate<object> RetryIf
            {
                get
                {
                    return o => true;
                }
            }
            public override Func<object> ConditionFunction
            {
                get
                {
                    return Condition;
                }
                set
                {
                    base.ConditionFunction = value;
                }
            }

            private static object Condition()
            {
                return new object();
            }
        }

        [Test]
        [Category("Async")]
        [Category("Retries")]
        public void Can_set_retry_policy_asynchronously()
        {
            var retryPolicy = new RetryPolicy { RetryCount = 5 };
            retryPolicy.RetryOn(new NetworkError(),
                                new Timeout(),
                                new ConnectionClosed());

            var client = new RestClient
            {
                RetryPolicy = retryPolicy,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json",
                Credentials = BasicAuthForTwitter
            };

            var asyncResult = client.BeginRequest(request);
            var response = client.EndRequest(asyncResult);
            Assert.IsNotNull(response);
        }

        [Test]
        [Category("Async")]
        [Category("Retries")]
        public void Can_retry_correct_number_of_times()
        {
            var retryPolicy = new RetryPolicy { RetryCount = 5 };
            
            //otherwise useless retry policy that always retries
            retryPolicy.RetryOn(new AlwaysRetry());

            var client = new RestClient
            {
                RetryPolicy = retryPolicy,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json",
                Credentials = BasicAuthForTwitter
            };

            var asyncResult = client.BeginRequest(request);
            var response = client.EndRequest(asyncResult);
            Assert.AreEqual(5, response.TimesTried);
            Assert.IsNotNull(response);
            Thread.Sleep(5000);
        }
    }
}
