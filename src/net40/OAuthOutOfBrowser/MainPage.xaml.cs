using System;
using System.Net;
using System.Net.Browser;
using System.Threading;
using Hammock;
using Hammock.Authentication.OAuth;
using OAuthOutOfBrowser.Controls;
using OAuthOutOfBrowser.Model;

namespace OAuthOutOfBrowser
{
    public partial class MainPage
    {
        private readonly SynchronizationContext _context;
        private readonly RestClient _client;

        public MainPage()
        {
            InitializeComponent();

            WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);

            _context = SynchronizationContext.Current;

            _client = new RestClient
                          {
                              Authority = "https://api.twitter.com",
                              HasElevatedPermissions = true
                          };

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            StartOAuthWorkflow();
        }

        private void StartOAuthWorkflow()
        {
            var credentials = new OAuthCredentials
                                  {
                                      ConsumerKey = "W2Xrij6aX5dMyweMBaO8og",
                                      ConsumerSecret = "7QyAnDb6I4Z6WLVWz6ri2lAGnwzgxlCd81qzQeGsoY",
                                      Type = OAuthType.RequestToken,
                                      ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                      SignatureMethod = OAuthSignatureMethod.HmacSha1
                                  };

            var request = new RestRequest
                              {
                                  Credentials = credentials,
                                  Path = "oauth/request_token"
                              };

            // 1a. Get an Unauthorized Request Token
            _client.BeginRequest(request,
                                 (req, resp, state) =>
                                     {
                                         // 1b. Parse the token
                                         var requestToken = OAuth.ParseToken(resp.Content);

                                         _context.Post(
                                             (s) =>
                                                 {
                                                     // 2. Send the user to the Authorization URL
                                                     var url = string.Format(
                                                         "{0}/oauth/authorize/?oauth_token={1}", _client.Authority,
                                                         requestToken.Token);

                                                     var login = new Login {Uri = new Uri(url)};
                                                     login.Closed += (sender, e) =>
                                                                         {
                                                                             var tokenString = ((Login) sender).Result;
                                                                             if (string.IsNullOrEmpty(tokenString))
                                                                             {
                                                                                 return;
                                                                             }
                                                                         };
                                                     login.Show();
                                                 }, null
                                             );
                                     }
                );
        }
    }
}
