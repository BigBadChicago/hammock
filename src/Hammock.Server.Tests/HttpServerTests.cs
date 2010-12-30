using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Hammock.Server.Defaults;
using NUnit.Framework;

namespace Hammock.Server.Tests
{
    [TestFixture]
    public class HttpServerTests
    {
        [Test]
        public void Can_make_one_request_and_receive_response()
        {
            var server = new HttpServer();
            server.Start(Address.Loopback, 8080);

            var client = new RestClient { Authority = "http://localhost:8080" };
            var request = new RestRequest();
            var response = client.Request(request);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void Can_make_asynchronous_requests_and_receive_responses()
        {
            // 00:00:15.2458720

            const int trials = 1000;
            ServicePointManager.DefaultConnectionLimit = trials;

            var server = new HttpServer();
            server.Start(Address.Loopback, 8080);

            var block = new AutoResetEvent(false);
            var client = new RestClient { Authority = "http://localhost:8080" };
            var request = new RestRequest();
            
            var timespan =
                WithTimer(
                    () =>
                        {
                            for (var i = 0; i < trials; i++)
                            {
                                client.BeginRequest(request,
                                                    (req, resp, state) =>
                                                    {
                                                        Assert.IsNotNull(resp);
                                                        Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                                                        if (i == trials)
                                                        {
                                                            block.Set();
                                                        }
                                                    }
                                    );
                            }

                            block.WaitOne();
                        }
                    );
            
            var peak = server.GetPeak();
            Trace.WriteLine("Peak queue was " + peak);
            Trace.WriteLine("Total Time:" + timespan);
        }

        public TimeSpan WithTimer(Action action)
        {
            var start = DateTime.Now;

            action.Invoke();

            return DateTime.Now - start;
        }
    }
}
