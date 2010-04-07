using Hammock.Streaming;
using Hammock.Tests.Helpers;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Timeout(30000)]
        [Category("Streaming")]
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

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }
    }
}
