using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Authentication.OAuth;
using Hammock.OAuth;
using Hammock.Serialization;
using Hammock.Tests.Converters;
using Hammock.Tests.Postmark;
using Hammock.Web;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Hammock.Tests
{
    [TestFixture]
    public partial class RestClientTests
    {
        private string _twitterUsername;
        private string _twitterPassword;
        
        private string _postmarkServerToken;
        private string _postmarkFromAddress;
        private string _postmarkToAddress;

        private string _consumerKey;
        private string _consumerSecret;

        [SetUp]
        public void SetUp()
        {
            _twitterUsername = ConfigurationManager.AppSettings["TwitterUsername"];
            _twitterPassword = ConfigurationManager.AppSettings["TwitterPassword"];

            _postmarkServerToken = ConfigurationManager.AppSettings["PostmarkServerToken"];
            _postmarkFromAddress = ConfigurationManager.AppSettings["PostmarkFromAddress"];
            _postmarkToAddress = ConfigurationManager.AppSettings["PostmarkToAddress"];

            _consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["OAuthConsumerSecret"];
        }

        public IWebCredentials BasicAuthForTwitter
        {
            get
            {
                var credentials = new BasicAuthCredentials
                                      {
                                          Username = _twitterUsername,
                                          Password = _twitterPassword
                                      };
                return credentials;
            }
        }

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
        public void Can_make_basic_auth_request_synchronously()
        {
            var client = new RestClient
                             {
                                 Authority = "http://api.twitter.com",
                                 VersionPath = "1"
                             };

            var request = new RestRequest
                              {
                                  Credentials = BasicAuthForTwitter,
                                  Path = "statuses/home_timeline.json"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        public void Can_make_basic_auth_request_with_headers_synchronously()
        {
            var client = new RestClient
                             {
                                 Authority = "http://api.twitter.com",
                                 VersionPath = "1",
                                 UserAgent = "Hammock"
                             };

            client.AddHeader("Always", "on every request");

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "/statuses/home_timeline.json"
            };

            request.AddHeader("Only", "on this request");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        public void Can_make_basic_auth_request_with_duplicate_headers_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "/statuses/home_timeline.json"
            };

            // Headers don't have to have unique names
            client.AddHeader("Always", "on every client");
            request.AddHeader("Always", "on this request");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        public void Can_make_basic_auth_request_get_with_url_parameters_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "/statuses/home_timeline.json"
            };

            client.AddParameter("client", "true");
            request.AddParameter("request", "true");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        public void Can_make_basic_auth_request_get_with_duplicate_url_parameters_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "/statuses/home_timeline.json"
            };

            // Since parameters should be unique, request should trump client
            client.AddParameter("client", "true");
            request.AddParameter("client", "false");
            request.AddParameter("request", "true");

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore("This test makes a live update")]
        public void Can_make_basic_auth_request_post_with_post_parameters_synchronously()
        {
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTwitter,
                Path = "/statuses/update.json",
                Method = WebMethod.Post
            };

            client.AddParameter("status", "testing something new and awesome");
            
            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore("This test requires Postmark access and costs money")]
        public void Can_make_basic_auth_request_post_with_json_entity_synchronously()
        {
            var settings = GetSerializerSettings();
            var message = new PostmarkMessage
                              {
                                  From = _postmarkFromAddress,
                                  To = _postmarkToAddress,
                                  Subject = "Test passed!",
                                  TextBody = "Hello from the Hammock unit tests!",
                                  Headers = new NameValueCollection
                                                {
                                                    {"Email-Header", "Shows up on an email"},
                                                    {"Email-Header", "Shows up on an email, too"}
                                                }
                              };

            var serializer = new HammockJsonDotNetSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.postmarkapp.com",
                Serializer = serializer,
                Deserializer = serializer
            };

            client.AddHeader("Accept", "application/json");
            client.AddHeader("Content-Type", "application/json; charset=utf-8");
            client.AddHeader("X-Postmark-Server-Token", _postmarkServerToken);
            client.AddHeader("User-Agent", "Hammock");
            
            var request = new RestRequest
                              {
                                  Path = "email",
                                  Entity = message
                              };

            var response = client.Request<PostmarkResponse>(request);
            var result = response.ContentEntity;

            Assert.IsNotNull(response);
            Assert.IsNotNull(result);
        }

        [Test]
        public void Can_get_oauth_request_token_sequentially()
        {
            var client = new RestClient
                             {
                                 Authority = "http://twitter.com/oauth"
                             };

            var request = new RestRequest
                              {
                                  Credentials = OAuthForTwitterRequestToken,
                                  Path="request_token"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            var settings = new JsonSerializerSettings
                               {
                                   MissingMemberHandling = MissingMemberHandling.Ignore,
                                   NullValueHandling = NullValueHandling.Include,
                                   DefaultValueHandling = DefaultValueHandling.Include
                               };

            settings.Converters.Add(new UnicodeJsonStringConverter());
            settings.Converters.Add(new NameValueCollectionConverter());
            return settings;
        }
    }
}


