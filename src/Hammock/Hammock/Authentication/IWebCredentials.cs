using Hammock.Web;
using Hammock.Web.Query;

namespace Hammock.Authentication
{
    public interface IWebCredentials
    {
        WebQuery GetQueryFor(string url, 
                             RestBase request, 
                             IWebQueryInfo info, 
                             WebMethod method);
    }
}