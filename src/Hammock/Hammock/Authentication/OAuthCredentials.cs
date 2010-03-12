using System;
using Hammock.OAuth;
using Hammock.Web;
using Hammock.Web.OAuth;
using Hammock.Web.Query;
using Hammock.Web.Query.OAuth;

namespace Hammock.Authentication
{
    public class OAuthCredentials : IWebCredentials
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public OAuthParameterHandling ParameterHandling { get; set; }
        public OAuthSignatureMethod SignatureMethod { get; set; }
        public OAuthType Type { get; set; }

        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string Verifier { get; set; }
        public string ClientUsername { get; set; }
        public string ClientPassword { get; set; }

        public string RequestTokenUrl { get; set; }
        public string AccessTokenUrl { get; set; }
        public string AuthorizationUrl { get; set; }
        public string CallbackUrl { get; set; }

        public WebQuery GetQueryFor(string url, RestBase request, IWebQueryInfo info, WebMethod method)
        {
            OAuthWebQueryInfo oauth;

            var workflow = new OAuthWorkflow
                               {
                                   ConsumerKey = ConsumerKey,
                                   ConsumerSecret = ConsumerSecret,
                                   ParameterHandling = ParameterHandling,
                                   SignatureMethod = SignatureMethod,
                                   RequestTokenUrl = RequestTokenUrl,
                                   AccessTokenUrl = AccessTokenUrl,
                                   AuthorizationUrl = AuthorizationUrl,
                                   CallbackUrl = CallbackUrl,
                                   ClientPassword = ClientPassword,
                                   ClientUsername = ClientUsername,
                                   Verifier = Verifier
                               };

            switch(Type)
            {
                case OAuthType.RequestToken:
                    workflow.RequestTokenUrl = url;
                    oauth = workflow.BuildRequestTokenInfo(method, request.Parameters);
                    break;
                case OAuthType.AccessToken:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildAccessTokenInfo(method, request.Parameters);
                    break;
                case OAuthType.ProtectedResource:
                    oauth = workflow.BuildProtectedResourceInfo(method, request.Parameters, url);
                    break;
                case OAuthType.ClientAuthentication:
                    oauth = workflow.BuildClientAuthAccessTokenInfo(method, request.Parameters);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OAuthWebQuery(oauth);
        }
    }
}
