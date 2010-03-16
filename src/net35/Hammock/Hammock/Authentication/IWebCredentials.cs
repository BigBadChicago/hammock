using Hammock.Web;

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