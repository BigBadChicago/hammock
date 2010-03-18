using Hammock.Authentication;
using Hammock.Authentication.OAuth;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        public IWebCredentials OAuthForTwitterRequestToken
        {
            get
            {
                var credentials = new OAuthCredentials
                                      {
                                          Type = OAuthType.RequestToken,
                                          SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                          ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                          ConsumerKey = _consumerKey,
                                          ConsumerSecret = _consumerSecret,
                                      };
                return credentials;
            }
        }

        [Test]
        [Category("OAuth")]
        public void Can_get_oauth_request_token_sequentially()
        {
            var client = new RestClient
                             {
                                 Authority = "http://twitter.com/oauth",
                                 Credentials = OAuthForTwitterRequestToken,
                             };

            var request = new RestRequest
                              {
                                  Path="request_token"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }
    }
}
