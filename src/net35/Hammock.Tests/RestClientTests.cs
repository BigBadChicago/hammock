using System;
using System.Configuration;
using NUnit.Framework;

namespace Hammock.Tests
{
    [TestFixture]
    public partial class RestClientTests
    {
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _tokenSecret;
        
        [SetUp]
        public void SetUp()
        {
            _consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["OAuthConsumerSecret"];
            _accessToken = ConfigurationManager.AppSettings["OAuthAccessToken"];
            _tokenSecret = ConfigurationManager.AppSettings["OAuthTokenSecret"];
        }

        [Test]
        public void foo()
        {
            var client = new RestClient();
            client.Authority = "http://stackauth.com/1.0/sites";
            var response = client.Request();
            Console.WriteLine(response.Content);

        }
    }
}


