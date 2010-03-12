using System;
using System.Net;

namespace Hammock.Web
{
#if !SILVERLIGHT
    /// <summary>
    /// A lighter-weight basic credentials object to avoid using <see cref="NetworkCredential"/>
    /// in partial trust scenarios.
    /// </summary>
    [Serializable]
#endif
    public class WebCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebCredentials"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public WebCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; private set; }
    }
}