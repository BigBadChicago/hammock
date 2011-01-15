using System.ComponentModel.Composition;
using System.Web;

namespace Hammock.Hosting.AspNet
{
    [Export(typeof(IHostApplication))]
    public class AspNetApplication : HttpApplication, IHostApplication
    {
        
    }
}