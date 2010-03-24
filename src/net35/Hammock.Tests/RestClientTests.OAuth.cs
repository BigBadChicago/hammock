using System;
using System.Net;
using Hammock.Authentication;
using Hammock.Authentication.OAuth;
using Hammock.Web;
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

        public IWebCredentials OAuthForTwitterProtectedResource
        {
            get
            {
                var credentials = new OAuthCredentials
                                      {
                                          Type = OAuthType.ProtectedResource,
                                          SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                          ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                          ConsumerKey = _consumerKey,
                                          ConsumerSecret = _consumerSecret,
                                          Token = _accessToken,
                                          TokenSecret = _tokenSecret,
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
                                  Path = "request_token"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_oauth_request_post_with_post_parameters_synchronously()
        {
            if (_ignoreTestsThatPostToTwitter)
            {
                Assert.Ignore("This test makes a live update - enable in app.config to run this test");
            }
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = OAuthForTwitterProtectedResource,
                Path = "/statuses/update.xml",
                Method = WebMethod.Post
            };

            client.AddParameter("status", string.Format("OAuth Post at {0}. tweet tweet", DateTime.Now.ToShortTimeString()));

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to post successfully");
        }

    }
}
