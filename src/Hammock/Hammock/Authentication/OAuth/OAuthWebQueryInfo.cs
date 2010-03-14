using Hammock.Attributes.Specialized;
using Hammock.Web;

namespace Hammock.Authentication.OAuth
{
    public class OAuthWebQueryInfo : IWebQueryInfo
    {
        [Parameter("oauth_consumer_key")]
        public string ConsumerKey { get; set; }

        [Parameter("oauth_token")]
        public string Token { get; set; }

        [Parameter("oauth_nonce")]
        public string Nonce { get; set; }

        [Parameter("oauth_timestamp")]
        public string Timestamp { get; set; }

        [Parameter("oauth_signature_method")]
        public string SignatureMethod { get; set; }

        [Parameter("oauth_signature")]
        public string Signature { get; set; }

        [Parameter("oauth_version")]
        public string Version { get; set; }

        [Parameter("oauth_callback")]
        public string Callback { get; set; }

        [Parameter("oauth_verifier")]
        public string Verifier { get; set; }

        [Parameter("x_auth_mode")]
        public string ClientMode { get; set; }

        [Parameter("x_auth_username")]
        public string ClientUsername { get; set; }

        [Parameter("x_auth_password")]
        public string ClientPassword { get; set; }

        [UserAgent]
        public string UserAgent { get; set; }

        public WebMethod WebMethod { get; set; }
        
        public OAuthParameterHandling ParameterHandling { get; set; }
        
        internal string ConsumerSecret { get; set; }
        
        internal string TokenSecret { get; set; }
    }
}