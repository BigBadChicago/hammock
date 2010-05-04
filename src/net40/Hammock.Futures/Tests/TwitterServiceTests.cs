using System;
using System.Configuration;
using System.Net;
using NUnit.Framework;
using Twitter;

namespace Hammock.Futures.Tests
{
    [TestFixture]
    public class TwitterServiceTests
    {
        private string _username;
        private string _password;

        private string _consumerKey;
        private string _consumerSecret;

        private string _token;
        private string _tokenSecret;

        private bool _ignoreTestsThatPostToTwitter = true; 

        private TwitterService _service;

        [SetUp]
        public void Can_initialize_generated_service()
        {
            _username = ConfigurationManager.AppSettings["TwitterUsername"];
            _password = ConfigurationManager.AppSettings["TwitterPassword"];

            _consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["OAuthConsumerSecret"];
            _token = ConfigurationManager.AppSettings["OAuthAccessToken"];
            _tokenSecret = ConfigurationManager.AppSettings["OAuthTokenSecret"];

            _service = new TwitterService(_consumerKey, _consumerSecret);
            
            var ignore = ConfigurationManager.AppSettings["IgnoreStatusUpdateTests"];
            if(!bool.TryParse(ignore, out _ignoreTestsThatPostToTwitter))
            {
                _ignoreTestsThatPostToTwitter = true;
            }
        }

        [Test]
        public void Can_get_public_timeline()
        {
            _service.AuthenticateAs(_username, _password);
            
            var statuses = _service.Statuses.PublicTimeline();

            foreach(var status in statuses)
            {
                Console.WriteLine(status.text);
            }
        }

        [Test]
        public void Can_post_update_with_basic()
        {
            if (_ignoreTestsThatPostToTwitter)
            {
                Assert.Ignore("This test makes a live update - enable in app.config to run this test");
            }

            ServicePointManager.Expect100Continue = false;

            _service.AuthenticateAs(_username, _password);

            var status = _service.Statuses.Tweet("This is a crazy awesome new feature " + DateTime.Now.ToShortTimeString());

            Assert.IsNotNull(status);
        }

        [Test]
        public void Can_post_update_with_oauth()
        {
            if (_ignoreTestsThatPostToTwitter)
            {
                Assert.Ignore("This test makes a live update - enable in app.config to run this test");
            }

            ServicePointManager.Expect100Continue = false;

            _service.AuthenticateWith(_token, _tokenSecret);

            var status = _service.Statuses.Tweet("This is a crazy awesome new feature " + DateTime.Now.ToShortTimeString());

            Assert.IsNotNull(status);
        }
    }
}
