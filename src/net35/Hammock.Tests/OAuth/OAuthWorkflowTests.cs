using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using Hammock.Authentication.OAuth;
using Hammock.Web;
using NUnit.Framework;

namespace Hammock.Tests.OAuth
{
    [TestFixture]
    public class OAuthWorkflowTests
    {
        private string _baseUrl;
        private string _consumerKey;
        private string _consumerSecret;
        private string _requestTokenUrl;
        private string _accessTokenUrl;
        private string _authorizeUrl;

        [SetUp]
        public void SetUp()
        {
            _baseUrl = ConfigurationManager.AppSettings["OAuthBaseUrl"];
            _consumerKey = ConfigurationManager.AppSettings["OAuthConsumerKey"];
            _consumerSecret = ConfigurationManager.AppSettings["OAuthConsumerSecret"];

            _requestTokenUrl = String.Format(_baseUrl, "request_token");
            _accessTokenUrl = String.Format(_baseUrl, "access_token");
            _authorizeUrl = String.Format(_baseUrl, "authorize");
        }

        private OAuthWebQuery GetRequestTokenQuery()
        {
            var oauth = new OAuthWorkflow
                            {
                                ConsumerKey = _consumerKey,
                                ConsumerSecret = _consumerSecret,
                                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                ParameterHandling = OAuthParameterHandling.UrlOrPostParameters,
                                RequestTokenUrl = _requestTokenUrl,
                                Version = "1.0"
                            };

            var info = oauth.BuildRequestTokenInfo(WebMethod.Get);
            return new OAuthWebQuery(info);
        }

        private OAuthWebQuery GetAccessTokenQuery(string token, string pin)
        {
            var oauth = new OAuthWorkflow
                            {
                                ConsumerKey = _consumerKey,
                                ConsumerSecret = _consumerSecret,
                                Token = token,
                                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                AccessTokenUrl = _accessTokenUrl,
                                Verifier = pin,
                                Version = "1.0"
                            };

            var info = oauth.BuildAccessTokenInfo(WebMethod.Post);
            return new OAuthWebQuery(info);
        }

        [Test]
        [Ignore("Can't be automated")]
        public void Can_authorize_access_token()
        {
            // Step 1 - Get Token and TokenSecret
            var requestTokenQuery = GetRequestTokenQuery();
            WebException exception;
            var requestTokenResponse = requestTokenQuery.Request(_requestTokenUrl, out exception);

            Console.WriteLine(_requestTokenUrl);
            Console.WriteLine("Request Token sent with header: ");
            Console.WriteLine(requestTokenQuery.AuthorizationHeader);
            Console.WriteLine();

            var startToken = requestTokenResponse.IndexOf("oauth_token=");
            var startSecret = requestTokenResponse.IndexOf("&oauth_token_secret=");

            Assert.GreaterOrEqual(startToken, 0);
            Assert.Greater(startSecret, 0);

            // Step 2 - Redirect to authorize this request token on Twitter
            var token = requestTokenResponse.Substring(startToken + "oauth_token=".Length,
                                                       requestTokenResponse.Length - startSecret -
                                                       "&oauth_token_secret".Length);
            var tokenSecret = requestTokenResponse.Substring(startSecret + "&oauth_token_secret=".Length);
            Console.WriteLine("Token: " + token);
            Console.WriteLine("Token Secret: " + tokenSecret);
            Console.WriteLine();

            // No live service to call back to (set your breakpoint here if debugging)            
            var authorizeUrl = _authorizeUrl + "?oauth_token=" + token;
            Console.WriteLine(authorizeUrl);
            Console.WriteLine("Visiting site to authorize...");
            var startInfo = new ProcessStartInfo {FileName = authorizeUrl};
            Process.Start(startInfo);

            /* <?xml version="1.0" encoding="UTF-8"?>
                <hash>
                  <request>/oauth/access_token</request>
                  <error>Invalid / expired Token</error>
                </hash>
             */

            // Step 3 - Get an access token after authorization
            var PIN = "12345"; // Use debugger to set this to the valid value
            var accessTokenQuery = GetAccessTokenQuery(token, PIN);
            var accessTokenResponse = accessTokenQuery.Request(_accessTokenUrl, out exception);

            Console.WriteLine(_accessTokenUrl);
            Console.WriteLine("Access Token sent with header: ");
            Console.WriteLine(accessTokenQuery.AuthorizationHeader);
            Console.WriteLine();

            Assert.IsNotNull(accessTokenResponse);
            startToken = accessTokenResponse.IndexOf("oauth_token=");
            startSecret = accessTokenResponse.IndexOf("&oauth_token_secret=");

            token = accessTokenResponse.Substring(startToken + "oauth_token=".Length,
                                                  requestTokenResponse.Length - startSecret -
                                                  "&oauth_token_secret".Length);
            tokenSecret = accessTokenResponse.Substring(startSecret + "&oauth_token_secret=".Length);

            Console.WriteLine("Token: {0}", token);
            Console.WriteLine("Token Secret: {0}", tokenSecret);
            Console.WriteLine();
        }

        [Test]
        public void Can_build_request_token()
        {
            OAuthWebQuery query = GetRequestTokenQuery();
            WebException exception;
            var response = query.Request(_requestTokenUrl, out exception);

            Assert.IsNotNull(response);
            Assert.IsNull(exception);
            Assert.AreNotEqual("Invalid OAuth Request", response);
        }
    }
}