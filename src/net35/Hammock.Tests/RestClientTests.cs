using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Tests.Converters;
using Hammock.Tests.Postmark;
using Hammock.Tests.Postmark.Converters;
using Hammock.Web;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = Hammock.Extras.Serialization.JsonSerializer;

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
        private string _accessToken;
        private string _tokenSecret;

        private bool _ignoreTestsThatPostToTwitter = true; 

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
            _accessToken = ConfigurationManager.AppSettings["OAuthAccessToken"];
            _tokenSecret = ConfigurationManager.AppSettings["OAuthTokenSecret"];

            var ignore = ConfigurationManager.AppSettings["IgnoreStatusUpdateTests"];
            try
            {
                if (!string.IsNullOrEmpty(ignore))
                {
                    _ignoreTestsThatPostToTwitter = bool.Parse(ignore);
                }
            }
            catch (FormatException)
            {
               Console.WriteLine( "Couldn't parse IgnoreStatusUpdateTests setting value '{0}' as a boolean value.", ignore);
            }
        }

        public IWebCredentials BasicAuthForTestService
        {
            get
            {
                var credentials = new BasicAuthCredentials
                                      {
                                          Username = "hammockuser",
                                          Password = "GloriousRest"
                                      };
                return credentials;
            }
        }

        [Test]
        public void Can_set_user_agent_from_client()
        {
            var client = new RestClient
                             {
                                 Authority = "http://empty-journey-80.heroku.com",
                                 UserAgent = "Hammock"
                             };
            var response = client.Request();
            Assert.IsNotNull(response);
        }

        [Test]
        public void Can_make_basic_auth_request_synchronously()
        {
            var client = new RestClient
                             {
                                 Authority = "http://empty-journey-80.heroku.com",
                             };

            var request = new RestRequest
                              {
                                  Credentials = BasicAuthForTestService,
                                  Path = "/"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); 
        }

        [Test]
        public void Can_make_head_request()
        {
            var client = new RestClient
            {
                Authority = "http://bit.ly",
                UserAgent = "Hammock"
            };

            // http://bit.ly/ay9par
            var request = new RestRequest
            {
                Path = "ay9par",
                Method = WebMethod.Head
            };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(
                "http://tweetsharp.codeplex.com/", response.Headers["Location"]
                );
        }

        [Test]
        public void Can_post_url_for_shortening()
        {
            // http://tinyurl.com/api-create.php?url=

            const string url = "http://hammock.codeplex.com";

            var client = new RestClient
            {
                Authority = "http://tinyurl.com",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Path = "api-create.php",
                Method = WebMethod.Get
            };
            request.AddParameter("url", url);

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Content.StartsWith("http://"));
        }

        [Test]
        public void Can_make_basic_auth_request_with_headers_synchronously()
        {
            var client = new RestClient
                             {
                                 Authority = "http://empty-journey-80.heroku.com",
                                 UserAgent = "Hammock"
                             };

            client.AddHeader("Always", "on every request");

            var request = new RestRequest
            {
                Credentials = BasicAuthForTestService,
                Path = "/"
            };

            request.AddHeader("Only", "on this request");

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); 
        }

        [Test]
        public void Can_get_content_as_stream()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com/1/",
                FollowRedirects = true,
                UserAgent = "Hammock"
            };
            
            // http://api.twitter.com/version/users/profile_image/:screen_name.format
            var request = new RestRequest { Path = "users/profile_image/danielcrenna.json" };
            var response = client.Request(request);

            Assert.IsNotNull(response.ContentStream);
            var bytes = ReadFully(response.ContentStream);
            Assert.IsNotNull(bytes);
            Assert.AreEqual(response.ContentLength, bytes.LongLength);
        }

        [Test]
        public void Can_get_content_as_bytes()
        {
            var client = new RestClient
            {
                Authority = "http://api.twitter.com/1/",
                FollowRedirects = true,
                UserAgent = "Hammock"
            };

            // http://api.twitter.com/version/users/profile_image/:screen_name.format
            var request = new RestRequest { Path = "users/profile_image/danielcrenna.json" };
            var response = client.Request(request);

            Assert.IsNotNull(response.ContentBytes);
            Assert.IsNotNull(response.Content, "Stream wasn't safeguarded against closure!");

            Assert.AreEqual(response.ContentLength, response.ContentBytes.LongLength);
        }

        [Test]
        public void Can_make_basic_auth_request_with_duplicate_headers_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://empty-journey-80.heroku.com",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTestService,
                Path = "/"
            };

            // Headers don't have to have unique names
            client.AddHeader("Always", "on every client");
            request.AddHeader("Always", "on this request");

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); 
        }

        [Test]
        public void Can_make_basic_auth_request_get_with_url_parameters_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://empty-journey-80.heroku.com",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTestService,
                Path = "/"
            };

            client.AddParameter("client", "true");
            request.AddParameter("request", "true");

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); 
        }

        [Test]
        public void Can_make_basic_auth_request_get_with_duplicate_url_parameters_synchronously()
        {
            var client = new RestClient
            {
                Authority = "http://empty-journey-80.heroku.com",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTestService,
                Path = "/"
            };

            // Since parameters should be unique, request should trump client
            client.AddParameter("client", "true");
            request.AddParameter("client", "false");
            request.AddParameter("request", "true");

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public void Can_make_basic_auth_request_post_with_post_parameters_synchronously()
        {
            if (_ignoreTestsThatPostToTwitter)
            {
                Assert.Ignore("This test makes a live update - enable in app.config to run this test");
            }
            ServicePointManager.Expect100Continue = false;

            var client = new RestClient
            {
                Authority = "http://empty-journey-80.heroku.com",
                UserAgent = "Hammock"
            };

            var request = new RestRequest
            {
                Credentials = BasicAuthForTestService,
                Path = "/posty",
                Method = WebMethod.Post
            };

            client.AddParameter("status", "tweet tweet");
            
            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); 
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

            var serializer = new JsonSerializer(settings);

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

        [Test]
        public void Can_use_client_standalone_sequentially()
        {
            var client = new RestClient
                             {
                                 Authority = "http://empty-journey-80.heroku.com",
                                 Path = "/ping"
                             };

            var response = client.Request();
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Can_use_client_standalone_with_type_sequentially()
        {
            var settings = GetSerializerSettings();
            var serializer = new JsonSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Path = "account/rate_limit_status.json",
                Serializer = serializer,
                Deserializer = serializer
            };

            var response = client.Request<TwitterRateLimitStatus>();
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ContentEntity);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Can_use_request_with_no_client_authority()
        {
            var client = new RestClient();
            var request = new RestRequest
                              {
                                  Path = "http://api.twitter.com/statuses/public_timeline.json"
                              };

            var response = client.Request(request);
            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        public static byte[] ReadFully(Stream stream)
        {
            return ReadFully(stream, 0);
        }

        // http://www.yoda.arachsys.com/csharp/readbinary.html
        public static byte[] ReadFully(Stream stream, long initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

    }
}


