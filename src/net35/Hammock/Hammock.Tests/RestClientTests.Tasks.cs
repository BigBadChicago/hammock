using Hammock.Tasks;
using Hammock.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        public class SimpleRateLimit
        {
            public int RemainingHits { get; set; }
        }

        [Test]
        public void Can_initiate_recurring_task()
        {
            var taskOptions = new TaskOptions
                              {
                                  RepeatTimes = 5,
                                  RepeatInterval = 1.Minutes()
                              };

            var client = new RestClient
                             {
                                 Authority = "http://api.twitter.com",
                                 VersionPath = "1",
                                 Credentials = BasicAuthForTwitter
                             };

            var request = new RestRequest
                              {
                                  Path = "account/rate_limit_status.xml",
                                  TaskOptions = taskOptions
                              };

            var async = client.BeginRequest(request,
                                            (req, resp) =>
                                                {
                                                    
                                                });
            Assert.IsNotNull(async);
        }

        [Test]
        public void Can_initiate_recurring_task_with_rate_limiting_rule()
        {
            var taskOptions = new TaskOptions<SimpleRateLimit>
                                  {
                                      RepeatTimes = 5,
                                      RepeatInterval = 1.Minutes(),
                                      RateLimitingRule = new RateLimitingRule<SimpleRateLimit>(50.0)
                                  };

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Credentials = BasicAuthForTwitter
            };

            var request = new RestRequest
            {
                Path = "account/rate_limit_status.xml",
                TaskOptions = taskOptions
            };

            var async = client.BeginRequest(request,
                                            (req, resp) =>
                                            {

                                            });
            Assert.IsNotNull(async);
        }
    }
}
