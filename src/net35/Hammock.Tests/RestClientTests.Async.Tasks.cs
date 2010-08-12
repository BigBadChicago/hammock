using System;
using System.Threading;
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
        public class TwitterRateLimitStatus : IRateLimitStatus
        {
            [JsonProperty("remaining_hits", Required = Required.Always)]
            public virtual int RemainingHits { get; set; }

            [JsonProperty("hourly_limit", Required = Required.Always)]
            public virtual int HourlyLimit { get; set; }

            [JsonProperty("reset_time_in_seconds")]
            public virtual long ResetTimeInSeconds{get;set;}

            public virtual DateTime ResetTime{get;set;}

            #region IRateLimitStatus Members

            int IRateLimitStatus.RemainingUses
            {
                get { return RemainingHits; }
            }

            DateTime IRateLimitStatus.NextReset
            {
                get{ return ResetTime;}
            }

            #endregion
        }

        [Test]
        [Category("Async")]
        [Category("Tasks")]
        public void Can_initiate_recurring_task()
        {
            const int repeatTimes = 8;
            var taskOptions = new TaskOptions
            {
                RepeatTimes = repeatTimes,
                RepeatInterval = 3.Seconds(),
            };

            var settings = GetSerializerSettings();
            var serializer = new JsonDotNetSerializer(settings);

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
                                            (req, resp, state) =>
                                            {
                                                Interlocked.Increment(ref repeatCount);

                                                if (repeatCount == repeatTimes)
                                                {
                                                    success = true;
                                                }
                                            });
            Assert.IsNotNull(async);
            async.AsyncWaitHandle.WaitOne();
            Assert.AreEqual(repeatTimes, repeatCount);
            Assert.IsTrue(success, "Task manifest did not complete");
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
                                      RateLimitingPredicate = s => false
                                  };

            var settings = GetSerializerSettings();
            var serializer = new JsonDotNetSerializer(settings);

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
                                            (req, resp, state) =>
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

        [Test]
        [Category("Async")]
        [Category("Tasks")]
        public void Can_initiate_recurring_task_with_percentage_based_rate_limiting_rule()
        {
            //make up a rate limit status with no remaining hits
            var resetTime = DateTime.Now + 10.Minutes();
            var fakeRateLimit = new TwitterRateLimitStatus
            {
                HourlyLimit = 100,
                RemainingHits = 0,
                ResetTime = resetTime,
                ResetTimeInSeconds = (long)(resetTime - DateTime.Now).TotalSeconds
            };
            Func<IRateLimitStatus> getRateLimit = () => fakeRateLimit;

            var taskOptions = new TaskOptions<IRateLimitStatus>()
            {
                RepeatInterval = 2.Seconds(),
                RateLimitPercent = 20.0, 
                GetRateLimitStatus = getRateLimit,
            };

            var settings = GetSerializerSettings();
            var serializer = new JsonDotNetSerializer(settings);

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
            var block = new AutoResetEvent(false); // signalled after first callback
            var async = client.BeginRequest(request,
                                            (req, resp, state) =>
                                            {
                                                if (!resp.SkippedDueToRateLimitingRule)
                                                {
                                                    Interlocked.Increment(ref repeatCount);
                                                }
                                                block.Set();
                                            });
            Assert.IsNotNull(async);
            block.WaitOne();
            Assert.AreEqual(1, repeatCount);
            //wait 2 seconds to make sure the task doesn't run again
            Thread.Sleep((int)5.Seconds().TotalMilliseconds);
            //should still be 1 since the rate limit doesn't reset for 10 min
            Assert.AreEqual(1, repeatCount);
            client.CancelPeriodicTasks();
        }
    }
}
