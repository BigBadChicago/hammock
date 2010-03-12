using System;
using System.Net;
using Hammock.Extensions;

namespace Hammock.Web.Query.Basic
{
    /// <summary>
    /// A web query engine for making requests that use basic HTTP authorization.
    /// </summary>
    public class BasicAuthWebQuery : WebQuery
    {
        private readonly string _password;
        private readonly string _username;

        public BasicAuthWebQuery(IWebQueryInfo info, string username, string password) :
            this(info)
        {
            _username = username;
            _password = password;
        }

        public BasicAuthWebQuery(IWebQueryInfo info) :
            base(info)
        {
        }

        public bool HasAuth
        {
            get
            {
                return
                    (!_username.IsNullOrBlank()
                     && !String.IsNullOrEmpty(_password));
            }
        }

        protected override void SetAuthorizationHeader(WebRequest request, string header)
        {
            if (!HasAuth)
            {
                return;
            }

            var credentials = WebExtensions.ToAuthorizationHeader(_username, _password);
            AuthorizationHeader = header;

#if !SILVERLIGHT
            request.PreAuthenticate = true;
            request.Headers[header] = credentials;
#else
            request.Headers["X-Twitter-Auth"] = AuthorizationHeader;
#endif
        }

        protected override void AuthenticateRequest(WebRequest request)
        {
            SetAuthorizationHeader(request, "Authorization");
        }
    }
}