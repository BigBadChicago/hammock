using Hammock.OAuth;

namespace Hammock.Web.OAuth
{
    /// <summary>
    /// A class to encapsulate OAuth authentication flow.
    /// <seealso cref="http://oauth.net/core/1.0#anchor9"/>
    /// </summary>
    public partial class OAuthWorkflow
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string Verifier { get; set; }

        public OAuthSignatureMethod SignatureMethod { get; set; }
        public OAuthParameterHandling ParameterHandling { get; set; }

        public string ClientUsername { get; set; }
        public string ClientPassword { get; set; }
    }
}