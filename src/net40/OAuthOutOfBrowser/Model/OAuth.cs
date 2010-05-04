using System;
using System.Collections.Generic;
using System.Linq;
using Hammock.Authentication.OAuth;

namespace OAuthOutOfBrowser.Model
{
    public class OAuth
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string Verifier { get; set; }
        public string CallbackConfirmed { get; set; }

        public static OAuthToken ParseToken(string response)
        {
            if(Uri.IsWellFormedUriString(response, UriKind.Absolute))
            {
                response = new Uri(response).Query.Substring(1);
            }

            var parameters = GetQueryParameters(response);

            foreach(var key in parameters.Keys)
            {
                Console.WriteLine(key);
            }

            return new OAuthToken
                       {
                           Token = GetValueFor(parameters, "oauth_token"),
                           TokenSecret = GetValueFor(parameters, "oauth_token_secret")
                       };
        }

        private static string GetValueFor(IDictionary<string, string> parameters, string key)
        {
            return !parameters.ContainsKey(key) ? "" : Uri.UnescapeDataString(parameters[key]);
        }

        private static Dictionary<string, string> GetQueryParameters(string response)
        {
            var parts = response.Split(new[] { '&' });
            return parts.Select(
                part => part.Split(new[] { '=' })).ToDictionary(
                    pair => pair[0], pair => pair[1]
                );
        }
    }
}
