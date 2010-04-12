using Hammock.Extras;
using Hammock.Tasks;
using Hammock.Tests.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class TwitterRateLimitStatus
        {
            [JsonProperty("remaining_hits", Required = Required.Always)]
            public virtual int RemainingHits { get; set; }

            [JsonProperty("hourly_limit", Required = Required.Always)]
            public virtual int HourlyLimit { get; set; }
        }

        [Test]
        [Category("Tasks")]
        [Category("Async")]
        public void Can_initiate_recurring_task()
        {
            var settings = GetSerializerSettings();
            var serializer = new HammockJsonDotNetSerializer(settings);

            const int repeatTimes = 2;
            var taskOptions = new TaskOptions
                              {
                                  RepeatTimes = repeatTimes,
                                  RepeatInterval = 5.Seconds()
                              };

            var client = new RestClient
                             {
                                 Authority = "http://api.twitter.com",
                                 VersionPath = "1",
                                 Credentials = BasicAuthForTwitter,
                                 Serializer = serializer,
                                 Deserializer = serializer
                             };

            var request = new RestRequest
                              {
                                  Path = "account/rate_limit_status.json",
                                  TaskOptions = taskOptions,
                                  ResponseEntityType = typeof(TwitterRateLimitStatus)
                              };

            var repeatCount = 0;
            var async = client.BeginRequest(request,
                                            (req, resp) =>
                                                {
                                                    var rateLimit = resp.ContentEntity as TwitterRateLimitStatus;
                                                    Assert.IsNotNull(rateLimit);
                                                    repeatCount++;
                                                });
            Assert.IsNotNull(async);
            async.AsyncWaitHandle.WaitOne();

            // This would only return the first response, not all of them
            var response = client.EndRequest(async);
            Assert.IsNotNull(response);

            Assert.IsTrue(repeatCount < repeatTimes, "Task manifest did not complete");
        }

        [Test]
        [Category("Async")]
        [Category("Tasks")]
        public void Can_initiate_recurring_task_with_rate_limiting_rule()
        {
            const int repeatTimes = 3;
            //boring rate limiting rule that always says to skip the task
            var taskOptions = new TaskOptions<TwitterRateLimitStatus>
                                  {
                                      RepeatTimes = repeatTimes,
                                      RepeatInterval = 2.Seconds(),
                                      RateLimitType = RateLimitType.ByPredicate,
                                      RateLimitingPredicate = s => false
                                  };

            var settings = GetSerializerSettings();
            var serializer = new HammockJsonDotNetSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Credentials = BasicAuthForTwitter,
                Serializer = serializer,
                Deserializer = serializer
            };

            var request = new RestRequest
            {
                Path = "account/rate_limit_status.json",
                TaskOptions = taskOptions,
                ResponseEntityType = typeof(TwitterRateLimitStatus)
            };

            var success = false;
            var repeatCount = 0;
            var async = client.BeginRequest(request,
                                            (req, resp) =>
                                            {
                                                Assert.IsTrue(resp.SkippedDueToRateLimitingRule);
                                                repeatCount++;

                                                if (repeatCount == repeatTimes)
                                                {
                                                    success = true;
                                                }
                                            });
            Assert.IsNotNull(async);
            async.AsyncWaitHandle.WaitOne();
            Assert.That(repeatCount == repeatTimes);
            Assert.IsTrue(success, "Task manifest did not complete");
        }
    }
}
