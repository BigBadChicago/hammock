using System;

namespace Hammock.Model
{
#if !SILVERLIGHT
    /// <summary>
    /// A data class representing either a request or an access token returned during an OAuth session.
    /// </summary>
    [Serializable]
#endif
    public class OAuthToken
    {
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public virtual string Token { get; set; }

        /// <summary>
        /// Gets or sets the token secret.
        /// </summary>
        public virtual string TokenSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the callback was confirmed.
        /// This value is only populated if request token authorization was requested with a callback.
        /// </summary>
        /// <value><c>true</c> if the callback was confirmed; otherwise, <c>false</c>.</value>
        public virtual bool CallbackConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// This value is only populated if client authentication was used.
        /// </summary>
        /// <value>The user ID.</value>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user screen name.
        /// This value is only populated if client authentication was used.
        /// </summary>
        /// <value>The user screen name.</value>
        public virtual string ScreenName { get; set; }
    }
}