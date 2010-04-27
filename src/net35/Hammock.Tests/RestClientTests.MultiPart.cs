using System.Net;
using Hammock.Authentication.Basic;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("MultiPart")]
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
        [Category("MultiPart")]
        public void Can_send_file_with_oauth()
        {
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Credentials = OAuthForTwitterProtectedResource,
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Path = "account/update_profile_image.json"
            };

            request.AddFile("failwhale", "failwhale.jpg", "failwhale.jpg", "image/jpeg");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }


        [Test]
        [Ignore("Makes a live update to a Twitter profile")]
        [Category("MultiPart")]
        public void Can_send_form_file_sequentially()
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

        [Test]
        [Ignore("Makes a live update to a Twitter profile")]
        [Category("MultiPart")]
        public void Can_post_picture_and_text_to_yfrog()
        {
            var client = new RestClient
            {
                Authority = "http://yfrog.com/api",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Path = "upload"
            };

            request.AddFile("media", "failwhale", "twitterProfilePhoto.jpg", "image/jpeg");
       
            request.AddField("username", _twitterUsername.ToLower() );
            request.AddField("password", _twitterPassword);
            request.AddField("message", "Bazinga!");
            

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK); 
        }
    }
}
