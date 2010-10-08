using System;
using System.Net;
using System.Windows;
using Hammock.Authentication.OAuth;
using Hammock.Web;
using Microsoft.Phone.Controls;

namespace Demo.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPageLoaded;
        }

        static void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            const string requestTokenUrl = "http://api.twitter.com/oauth";

            // 1. You need a workflow object to hijack Hammock's authentication
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = "ConsumerKey",
                ConsumerSecret = "ConsumerSecret",
                Version = "1.0"
            };

            // 2. Create a new info instance specific the request type, i.e. BuildXXX;
            //    remember that any additional querystring parameters have to be known
            //    in advance for signature signing. Use the WebParameterCollection overload
            //    if non-OAuth paramters are needed.

            var additionalParameters = new WebParameterCollection(); // <-- non-oauth params go in here
            
            var workflow = new OAuthWorkflow(credentials) // <-- copy ctor to avoid some repetition
                               {
                                   RequestTokenUrl = requestTokenUrl
                               }; 

            var info = workflow.BuildRequestTokenInfo(WebMethod.Get, additionalParameters);

            // 3. You need an OAuthWebQuery instance to pull authentication details; use
            //    the OAuthQueryInfo instance above to seed it.
            var query = credentials.GetQueryFor(
                requestTokenUrl,                // OAuth request token URL
                additionalParameters,           // Additional non-OAuth parameters
                info,                           // Info built using the workflow above
                info.WebMethod                  // WebMethod
                );

            // 4. OAuth can be set in the Authorization header or in URL parameters,
            //    depending on the implementation used. So call the new query.GetAuthorizationContent()
            //    method to get back a string containing either the header value or
            //    URL parameter part. You'll have to put it in the right spot depending on
            //    which one you chose and whether it's GET or POST with URL parameters.
            var auth = query.GetAuthorizationContent();

            // 5. Set auth on your plain old WebClient (not including all the workarounds for SL perms here)
            var client = new WebClient();
            client.Headers["Authorization"] = auth;
            client.OpenReadCompleted += ClientOpenReadCompleted;
            client.OpenReadAsync(new Uri(requestTokenUrl));
        }

        static void ClientOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            Console.WriteLine(e.Result);
        }
    }
}