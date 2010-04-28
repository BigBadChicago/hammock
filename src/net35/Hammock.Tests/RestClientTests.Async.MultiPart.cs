using System.Diagnostics;
using System.Net;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Async")]
        [Category("MultiPart")]
        [Timeout(10000)]
        public void Can_send_form_field_asynchronously()
        {
            ServicePointManager.Expect100Continue = false;
            
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Credentials = BasicAuthForTwitter,
                UserAgent = "Hammock",
            };

            var request = new RestRequest
            {
                Path = "account/update_profile_image.json"
            };

            request.AddField("email", "bob@example.com");
            var asyncResult = client.BeginRequest(request);
            var response = client.EndRequest(asyncResult);
            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore("Makes a live update to a Twitter profile")]
        [Category("Async")]
        [Category("MultiPart")]
        public void Can_send_form_file_asynchronously()
        {
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Credentials = BasicAuthForTwitter,
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Path = "account/update_profile_image.json"
            };

            request.AddFile("photo", "photo.jpg", "twitterProfilePhoto.jpg");

            var asyncResult = client.BeginRequest(request);
            var response = client.EndRequest(asyncResult);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
