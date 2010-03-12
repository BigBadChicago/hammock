using System;
using Hammock.Extensions;
using Hammock.Web;
using Hammock.Web.Query;
using Hammock.Web.Query.Basic;

namespace Hammock.Authentication
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class BasicAuthCredentials : IWebCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public WebQuery GetQueryFor(string url, 
                                    RestBase request, 
                                    IWebQueryInfo info, 
                                    WebMethod method)
        {
            return HasAuth
                ? new BasicAuthWebQuery(info, Username, Password)
                : new BasicAuthWebQuery(info);
        }

        public bool HasAuth
        {
            get
            {
                return !Username.IsNullOrBlank() && !Password.IsNullOrBlank();
            }
        }
    }
}