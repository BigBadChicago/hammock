using System;
using Hammock.Framework;
using Hammock.Framework.DataAccess;

namespace Hammock.Framework.Mono.Demo
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			var api = new API();
			
			//api.Methods.Add<Customer>(
			//               ()=> {
			//	 					
			//                    });		
		}
	}
	
	public class CustomerController : IController
	{
	    public CustomerResponse SendMessage(CustomerRequest request)
		{
		    return null;	
		}
	}
	
	// http://localhost:7878/customers?{page=},{count=} GET, PUT, POST
	// http://localhost:7878/customers/{id}.{format} GET, DELETE, POST
	// http://localhost:7878/customers/orders?{page=},{count=} GET, PUT, POST
	// http://localhost:7878/customers/orders/{id}.{format} GET, DELETE, POST
	
	public class CustomerRequest // : IViewModel?
	{
		
	}
	
	public class CustomerResponse
	{
		
	}
	
	public class Customer : IResource
	{
		public Identity Id { get; set; }
		
		public IHasMany<Order> Orders { get; set; }
	}
	
	public class Order : IResource
	{
		public Identity Id { get; set; }
	}
}