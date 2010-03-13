using System;

namespace Hammock.OAuth
{
#if !SILVERLIGHT
    /// <summary>
    /// Hashing strategies supported by OAuth.
    /// </summary>
    /// <seealso cref="http://oauth.net/core/1.0#signing_process"/>
    [Serializable]
#endif
    public enum OAuthSignatureMethod
    {
        /// <summary>
        /// Plain text is only permitted when combined with the HTTPS protocol.
        /// </summary>
        PlainText,
        /// <summary>
        /// Uses HMAC-SHA1 for signing requests.
        /// </summary>
        HmacSha1,
        /// <summary>
        /// Uses RSA-SHA1 for signing requests.
        /// </summary>
        RsaSha1
    }
}