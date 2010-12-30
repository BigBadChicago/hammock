using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

namespace Hammock.Hosting.AspNet
{
    [Export(typeof(IHttpModule))]
    public class AspNetModule : IHttpModule
    {
        private static readonly object _sync = new object();
        private static HttpApplication _application;

        public virtual void Init(HttpApplication context)
        {
            EnsureApplicationInitialized();
        }

        private static void EnsureApplicationInitialized()
        {
            lock(_sync)
            {
                if(_application == null)
                {
                    lock(_sync)
                    {
                        InitializeApplication();

                        if(_application == null)
                        {
                            throw new InvalidOperationException("Could not find an ASP.NET application host!");
                        }
                    }
                }
            }
        }

        private static void InitializeApplication()
        {
            var referencedAssemblies = BuildManager.GetReferencedAssemblies();
            foreach (Assembly assembly in referencedAssemblies)
            {
                var types = assembly.GetTypes();
                foreach(var type in types)
                {
                    if (!typeof (IHostApplication).IsAssignableFrom(type) || 
                        !type.IsSubclassOf(typeof (HttpApplication)))
                    {
                        continue;
                    }

                    _application = (HttpApplication)Activator.CreateInstance(type);
                    return;
                }
            }
        }

        public virtual void Dispose()
        {
            
        }
    }
}