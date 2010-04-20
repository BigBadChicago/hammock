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

        public IWebCredentials OAuthForTwitterClientAuth
        {
            get
            {
                var credentials = new OAuthCredentials
                                      {
                                          Type = OAuthType.ClientAuthentication,
                                          SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                          ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                          ConsumerKey = _consumerKey,
                                          ConsumerSecret = _consumerSecret,
                                          ClientUsername = _twitterUsername,
                                          ClientPassword = _twitterPassword,
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
        public void Can_make_oauth_get_request()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com/",
                VersionPath = "1",
                Credentials = OAuthForTwitterProtectedResource,
            };

            var request = new RestRequest
            {
                Path = "account/verify_credentials.xml"
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to fetch successfully");
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_another_oauth_get_request()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com/",
                VersionPath = "1",
                Credentials = OAuthForTwitterProtectedResource,
            };

            var request = new RestRequest
            {
                Path = "statuses/mentions.xml"
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to fetch successfully");
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_oauth_get_request_with_url_parameters()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com/",
                VersionPath = "1",
                Credentials = OAuthForTwitterProtectedResource,
            };

            var request = new RestRequest
            {
                Path = "statuses/friends_timeline.json?since_id=200"
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to fetch successfully");
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_oauth_post_requests_with_post_parameters_synchronously()
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
                Path = "/statuses/update.json",
                Method = WebMethod.Post
            };

            request.AddParameter("status", string.Format("something #requiring #encoding @! {0}", DateTime.Now.ToShortTimeString()));
            

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to post successfully");
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_oauth_post_requests_with_post_parameters_synchronously_with_defined_path()
        {
            if (_ignoreTestsThatPostToTwitter)
            {
                Assert.Ignore("This test makes a live update - enable in app.config to run this test");
            }
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                UserAgent = "Hammock"
            };

            var update = Uri.EscapeDataString(string.Format(
                "something #requiring #encoding @! at {0}", DateTime.Now.ToShortTimeString()
                                                            ));
                       
            var path = string.Format(
                "http://api.twitter.com/1/statuses/update.json?status={0}", 
                update);

            var request = new RestRequest
            {
                Credentials = OAuthForTwitterProtectedResource,
                Path = path,
                Method = WebMethod.Post
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsFalse(response.Content.Contains("<error>"), "Did not appear to post successfully");
        }

        [Test]
        [Category("OAuth")]
        public void Can_make_oauth_request_post_with_post_parameters_set_on_client()
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

        [Test]
        [Category("OAuth")]
        [Ignore("Our consumer key is not currently approved for xauth")]
        public void Can_get_tokens_with_xauth()
        {
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "https://api.twitter.com/oauth"
            };

            var request = new RestRequest
            {
                Credentials = OAuthForTwitterClientAuth,
                Path = "access_token",
                Method = WebMethod.Get // Will force a POST
            };
             
            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
