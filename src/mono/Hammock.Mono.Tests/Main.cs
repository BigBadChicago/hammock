using System;
using Hammock;

namespace Hammock.Mono.Tests
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var client = new RestClient();
			client.Authority = "https://api.twitter.com";
			client.VersionPath = "1";
			
			var request = new RestRequest();
			request.Path = "statuses/public_timeline.json";
			
			var response = client.Request(request);
			
			Console.WriteLine(response.Content);
			
			Console.ReadKey();
		}
	}
}
