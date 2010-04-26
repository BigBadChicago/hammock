using System.Net;
using Hammock.Extras;
using Hammock.Tests.Helpers;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests
{
    partial class RestClientTests
    {
        [Test]
        [Category("Async")]
        public void Can_make_basic_auth_request_asynchronously()
        {
            var client = new RestClient
            {
                Credentials = BasicAuthForTwitter,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json"
            };

            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);
                    }
                );

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        [Category("Async")]
        public void Can_make_basic_auth_request_with_user_state_asynchronously()
        {
            var client = new RestClient
            {
                Credentials = BasicAuthForTwitter,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json"
            };

            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);
                        Assert.AreEqual(12345, state);
                    }
                );

            var asyncResult = client.BeginRequest(request, callback, 12345);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        [Category("Async")]
        public void Can_make_basic_auth_request_with_headers_asynchronously()
        {
            var client = new RestClient
            {
                Credentials = BasicAuthForTwitter,
                Authority = "http://api.twitter.com",
                VersionPath = "1"
            };

            var request = new RestRequest
            {
                Path = "statuses/home_timeline.json"
            };

            client.AddHeader("Always", "on the client");
            request.AddHeader("Only", "on this request");

            var callback = new RestCallback(
                (req, resp, state) =>
                    {
                        Assert.IsNotNull(req);
                        Assert.IsNotNull(resp);
                    });

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
        }

        [Test]
        public void Can_use_client_standalone_asynchronously()
        {
            var callback = new RestCallback(
                (req, resp, state) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                });

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Path = "statuses/public_timeline.json"
            };

            var asyncResult = client.BeginRequest(callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        public void Can_use_client_standalone_with_type_asynchronously()
        {
            var callback = new RestCallback<TwitterRateLimitStatus>(
                (req, resp, state) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                });

            var settings = GetSerializerSettings();
            var serializer = new HammockJsonDotNetSerializer(settings);

            var client = new RestClient
            {
                Authority = "http://api.twitter.com",
                VersionPath = "1",
                Path = "account/rate_limit_status.json",
                Serializer = serializer,
                Deserializer = serializer
            };

            var asyncResult = client.BeginRequest(callback);
            var response = client.EndRequest<TwitterRateLimitStatus>(asyncResult);

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ContentEntity);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
        }

        [Test]
        [Timeout(10000)]
        public void Can_timeout_on_asynchronous_query()
        {
            var client = new RestClient
            {
                Authority = "http://failwhale.diller.ca",
            };

            var request = new RestRequest
            {
                Path = "timeout.php"
            };
            request.AddParameter("delay", "6");
            request.Timeout = 3.Seconds(); 

            var callback = new RestCallback(
                (req, resp, state) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                }
            );

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);
                   
            Assert.IsTrue(response.TimedOut);
        }

        [Test]
        public void Can_timeout_on_asynchronous_post()
        {
            var client = new RestClient
            {
                Authority = "http://failwhale.diller.ca",
            };

            var request = new RestRequest
            {
                Path = "timeout.php"
            };
            request.AddParameter("delay", "6");
            request.Timeout = 3.Seconds();

            var callback = new RestCallback(
                (req, resp, state) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                }
            );
            request.Method = WebMethod.Post;
            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsTrue(response.TimedOut);
        }

        [Test]
        public void Can_not_timeout_on_asynchronous_query_with_timeout_set()
        {
            var client = new RestClient
            {
                Authority = "http://failwhale.diller.ca",
            };

            var request = new RestRequest
            {
                Path = "timeout.php"
            };
            request.AddParameter("delay", "1");
            request.Timeout = 4.Seconds();

            var callback = new RestCallback(
                (req, resp, state) =>
                {
                    Assert.IsNotNull(req);
                    Assert.IsNotNull(resp);
                }
            );

            var asyncResult = client.BeginRequest(request, callback);
            var response = client.EndRequest(asyncResult);

            Assert.IsFalse(response.TimedOut);
        }
    }
}
