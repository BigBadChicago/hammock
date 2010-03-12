namespace Hammock.Web.OAuth
{
    public partial class OAuthWorkflow
    {
        /// <summary>
        /// The request token url.
        /// </summary>
        /// <seealso cref="http://oauth.net/core/1.0#request_urls"/>
        public string RequestTokenUrl { get; set; }

        /// <summary>
        /// The access token url.
        /// </summary>
        /// <seealso cref="http://oauth.net/core/1.0#request_urls"/>
        public string AccessTokenUrl { get; set; }

        /// <summary>
        /// THe user authorization url.
        /// </summary>
        /// <seealso cref="http://oauth.net/core/1.0#request_urls"/>
        public string AuthorizationUrl { get; set; }
    }
}