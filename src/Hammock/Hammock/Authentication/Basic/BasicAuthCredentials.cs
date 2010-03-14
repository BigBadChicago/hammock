using System;
using Hammock.Extensions;
using Hammock.Web;

namespace Hammock.Authentication.Basic
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class BasicAuthCredentials : IWebCredentials
    {
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }

        public WebQuery GetQueryFor(string url, 
                                    RestBase request, 
                                    IWebQueryInfo info, 
                                    WebMethod method)
        {
            return HasAuth
                       ? new BasicAuthWebQuery(info, Username, Password)
                       : new BasicAuthWebQuery(info);
        }

        public virtual bool HasAuth
        {
            get
            {
                return !Username.IsNullOrBlank() && !Password.IsNullOrBlank();
            }
        }
    }
}