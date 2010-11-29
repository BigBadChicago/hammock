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
    }
}


