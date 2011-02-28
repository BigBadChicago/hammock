using System;
using System.Reflection;

namespace Hammock
{
    public static class Program
    {
        private static readonly string _version;
        private static readonly RestClient _client;

        static Program()
        {
            _version = Assembly.GetAssembly(typeof (Program)).GetName().Version.ToString();
            _client = new RestClient();
        }

        public static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintUsage();
                return;
            }

            if(args.Length == 1)
            {
                Uri uri;
                if(Uri.TryCreate(args[0], UriKind.Absolute, out uri))
                {
                    _client.Authority = uri.Scheme + "://" + uri.Authority;
                    var request = new RestRequest { Path = uri.PathAndQuery };
                    var response = _client.Request(request);
                    response.Content.Out();
                    return;
                }
                else
                {
                    
                }
            }
        }

        private static void PrintUsage()
        {
            "Hammock".Out();
        }

        private static void Out(this string input)
        {
            Console.WriteLine(input);
        }
    }
}
