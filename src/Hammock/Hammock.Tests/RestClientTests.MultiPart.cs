using System.Net;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        public void Can_send_form_field_sequentially()
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

            request.AddField("email", "bob@example.com");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore("Makes a live update to a Twitter profile")]
        public void Can_send_multi_part_request_sequentially()
        {
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

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }
    }
}
