using System;
using Hammock.Server;

namespace Hammock.Framework.Mono.Modules
{
	public class RoutingModule : IHttpModule 
	{
		public virtual IAsyncResult BeginProcess(IHttpContext context)
		{
			return null;
		}
		
		public virtual IHttpContext EndProcess(IAsyncResult result)
		{
			return null;
		}
		
		public virtual void Dispose()
		{
			
		}
		
		public RoutingModule ()
		{
			
		}
	}
}

