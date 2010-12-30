using System.ComponentModel.Composition;

namespace Hammock.Hosting.AspNet
{
    [Export(typeof(IHost))]
    public class AspNetHost : IHost
    {
        private readonly IHostApplication _application;

        public AspNetHost()
        {
            _application = new AspNetApplication();
        }

        public virtual IHostApplication GetApplication()
        {
            return _application;
        }
    }
}
