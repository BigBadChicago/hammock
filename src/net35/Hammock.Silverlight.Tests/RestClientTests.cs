using System.Net;
using Hammock.Web;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hammock.Silverlight.Tests
{
    [TestFixture]
    public partial class RestClientTests
    {
        [SetUp]
        public void SetUp()
        {
            
        }

        [Test]
        public void Can_make_head_request()
        {
            var client = new RestClient
            {
                Authority = "http://is.gd",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Path = "bF9rh",
                Method = WebMethod.Head
            };

            var result = client.BeginRequest(request);
            var response = client.EndRequest(result);
            Assert.IsNotNull(response);

            var longUrl = response.Headers["X-Pingback"];
            Assert.AreEqual("http://tweetsharp.com/xmlrpc.php", longUrl);
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            var settings = new JsonSerializerSettings
                               {
                                   MissingMemberHandling = MissingMemberHandling.Ignore,
                                   NullValueHandling = NullValueHandling.Include,
                                   DefaultValueHandling = DefaultValueHandling.Include
                               };
            return settings;
        }
    }
}


