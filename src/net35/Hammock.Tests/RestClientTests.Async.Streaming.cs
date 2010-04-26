using System;
using Hammock.Streaming;
using Hammock.Tests.Helpers;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests
{
	partial class RestClientTests
	{
        [Test]
        [Category("Async")]
        [Category("Streaming")]
        [Timeout(30000)]
        public void Can_stream_with_get()
        {
            var options = new StreamOptions
            {
                Duration = 20.Seconds(),
                ResultsPerCallback = 10
            };

            var client = new RestClient
            {
                Authority = "http://stream.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "statuses/sample.json",
                StreamOptions = options
            };

            var responses = 0;
            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        responses++;
                    }
                );

            var result = client.BeginRequest(request, callback);
            var response = client.EndRequest(result);
            Assert.IsNotNull(response);
            Assert.GreaterOrEqual(responses, 1);
        }

        [Test]
        [Category("Async")]
        [Category("Streaming")]
        [Timeout(30000)]
        public void Can_stream_with_post_async()
        {
            var options = new StreamOptions
            {
                Duration = 20.Seconds(),
                ResultsPerCallback = 10
            };

            var client = new RestClient
            {
                Authority = "http://stream.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "statuses/sample.json",
                StreamOptions = options,
                Method = WebMethod.Post
            };

            var responses = 0;
            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        responses++;
                    }
                );

            var result = client.BeginRequest(request, callback);
            var response = client.EndRequest(result);
            Assert.IsNotNull(response);
            Assert.GreaterOrEqual(responses, 1);
        }

        [Test]
        [Category("Async")]
        [Category("Streaming")]
        [Timeout(30000)]
        public void Can_stream_with_get_and_cancel()
        {
            var options = new StreamOptions
            {
                Duration = 20.Seconds(),
                ResultsPerCallback = 10
            };

            var client = new RestClient
            {
                Authority = "http://stream.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "statuses/sample.json",
                StreamOptions = options
            };

            var responses = 0;
            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        if(responses == 5)
                        {
                            client.CancelStreaming();
                        }
                        else
                        {
                            responses++;
                        }
                    });

            var result = client.BeginRequest(request, callback);
            var response = client.EndRequest(result);
            
            Assert.IsNotNull(response);
            Assert.AreEqual(responses, 5);
        }
	}
}
