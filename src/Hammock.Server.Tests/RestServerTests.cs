using System.Net;
using NUnit.Framework;

namespace Hammock.Server.Tests
{
    [TestFixture]
    public class RestServerTests
    {
        [Test]
        public void Can_listen_for_incoming_requests()
        {
            var server = new RestServer();
            server.Start(IPAddress.Loopback, 9090);

            var client = new RestClient { Authority = "localhost:9090" };
        }
    }
}
