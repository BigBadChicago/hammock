using System;
using System.Net;
using Hammock.Extensions;

namespace Hammock.Web
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

            var credentials = GetAuthorizationHeader();
            AuthorizationHeader = header;

#if !SILVERLIGHT || WindowsPhone
            request.Headers[header] = credentials;
#else
            if (HasElevatedPermissions)
            {
                request.Headers[header] = credentials;
            }
            else
            {
                request.Headers[SilverlightAuthorizationHeader] = AuthorizationHeader;
            }
#endif
        }

        private string GetAuthorizationHeader()
        {
            return WebExtensions.ToBasicAuthorizationHeader(_username, _password);
        }

        protected override void AuthenticateRequest(WebRequest request)
        {
            SetAuthorizationHeader(request, "Authorization");
        }

        public override string GetAuthorizationContent()
        {
            return GetAuthorizationHeader();
        }
    }
}