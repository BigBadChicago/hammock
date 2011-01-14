using System;
using System.Collections.Generic;
using Hammock.Server;

namespace Hammock.Framework.Mono
{
	public class API : IDisposable
	{
		private IHttpServer _server;
		
		public virtual ICollection<RestMethod> Methods { get; set; }
		
		public API() : this(null)
		{
			Methods = new List<RestMethod>(0);
		}
		
		public API(IAddress address)
		{
			if(address == null)
			{
				address = new Hammock.Server.Defaults.Address(System.Net.IPAddress.Loopback);
			}
			
			var routing = new RoutingModule();
			
			_server = new RestServer();
			_server.Modules.Add(routing);
			_server.Start(address);
			
			// Scan for classes that implement IResource
			
			// Scan for custom routes
			
			// Add routes that map to resources
		}
		
		public virtual void Dispose()
		{
			if(_server != null)
			{
				_server.Dispose();
			}
		}
	}
}