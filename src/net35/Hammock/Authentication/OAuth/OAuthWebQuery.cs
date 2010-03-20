using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hammock.Extensions;
using Hammock.Web;
#if !Silverlight
using System.Web;
#endif

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthWebQuery : WebQuery
    {
        public string Realm { get; set; }
        public OAuthParameterHandling ParameterHandling { get; private set; }

        public OAuthWebQuery(OAuthWebQueryInfo info)
            : base(info)
        {
            Method = info.WebMethod;
            ParameterHandling = info.ParameterHandling;
        }

#if SILVERLIGHT
        private string _authorizationHeader = "X-Twitter-Auth";
#endif
        protected override WebRequest BuildPostOrPutWebRequest(PostOrPut method, string url, out byte[] content)
        {
            Uri uri;
            url = PreProcessPostParameters(url, out uri);

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = method == PostOrPut.Post ? "POST" : "PUT";
            request.ContentType = "application/x-www-form-urlencoded";

            SetRequestMeta(request);
                
            content = PostProcessPostParameters(request, uri);
#if !SILVERLIGHT
            // [DC]: Silverlight sets this dynamically
            request.ContentLength = content.Length;
#endif
            return request;
        }

        protected override WebRequest BuildGetOrDeleteWebRequest(GetOrDelete method, string url)
        {
            var uri = new Uri(url);
            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    // [DC]: Handled in authentication
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    uri = GetAddressWithOAuthParameters(uri);
                    break;
            }

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method == GetOrDelete.Get ? "GET" : "DELETE";
            AuthenticateRequest(request);
            SetRequestMeta(request);

            return request;
        }

        private Uri GetAddressWithOAuthParameters(Uri address)
        {
            var sb = new StringBuilder("?");
            var parameters = 0;
            foreach (var parameter in Parameters)
            {
                if (parameter.Name.IsNullOrBlank() || parameter.Value.IsNullOrBlank())
                {
                    continue;
                }

                parameters++;
                var format = parameters < Parameters.Count ? "{0}={1}&" : "{0}={1}";
                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            return new Uri(address + sb.ToString());
        }

        private byte[] PostProcessPostParameters(WebRequest request, Uri uri)
        {
            var body = "";
            switch (ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    SetAuthorizationHeader(request, "Authorization");
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    body = GetPostParametersValue(Parameters, false);
                    break;
            }

            // Only use the POST parameters that exist in the body
#if SILVERLIGHT
            var postParameters = new WebParameterCollection(uri.Query.ParseQueryString());
#else
            var postParameters = new WebParameterCollection(HttpUtility.ParseQueryString(uri.Query));
#endif
            // Append any leftover values to the POST body
            var nonAuthParameters = GetPostParametersValue(postParameters, true);
            if (body.IsNullOrBlank())
            {
                body = nonAuthParameters;
            }
            else
            {
                if (!nonAuthParameters.IsNullOrBlank())
                {
                    body += "&".Then(nonAuthParameters);
                }
            }

            var content = Encoding.UTF8.GetBytes(body);
            return content;
        }

        private static string PreProcessPostParameters(string url, out Uri uri)
        {
            // Remove POST parameters from query
            uri = url.AsUri();
            url = uri.Scheme.Then("://")
#if !SILVERLIGHT
                .Then(uri.Authority);
#else
                .Then(uri.Host);
#endif
            if (uri.Port != 80)
            {
                url = url.Then(":" + uri.Port);
            }
            url = url.Then(uri.AbsolutePath);
            return url;
        }

        private static string GetPostParametersValue(ICollection<WebPair> postParameters, bool escapeParameters)
        {
            var body = "";
            var parameters = 0;
            foreach (var postParameter in postParameters)
            {
                // client_auth method does not function when these are escaped
                var name = escapeParameters
                               ? OAuthTools.UrlEncode(postParameter.Name)
                               : postParameter.Name;
                var value = escapeParameters
                                ? OAuthTools.UrlEncode(postParameter.Value)
                                : postParameter.Value;

                body = body.Then("{0}={1}".FormatWith(name, value));

                if (parameters < postParameters.Count - 1)
                {
                    body = body.Then("&");
                }

                parameters++;
            }
            return body;
        }

        protected override void AuthenticateRequest(WebRequest request)
        {
            switch(ParameterHandling)
            {
                case OAuthParameterHandling.HttpAuthorizationHeader:
                    SetAuthorizationHeader(request, "Authorization");
                    break;
                case OAuthParameterHandling.UrlOrPostParameters:
                    // [DC]: Handled in builder method
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void SetAuthorizationHeader(WebRequest request, string header)
        {
            var authorization = GetAuthorizationHeader();
            AuthorizationHeader = authorization;

#if !SILVERLIGHT
            request.Headers["Authorization"] = AuthorizationHeader;
#else
            request.Headers[_authorizationHeader] = AuthorizationHeader;
#endif
        }

        private string GetAuthorizationHeader()
        {
            var sb = new StringBuilder("OAuth ");
            if (!Realm.IsNullOrBlank())
            {
                sb.Append("realm=\"{0}\",".FormatWith(OAuthTools.UrlEncode(Realm)));
            }

            var parameters = 0;
            foreach (var parameter in Parameters.Where(parameter => 
                                                       !parameter.Name.IsNullOrBlank() && 
                                                       !parameter.Value.IsNullOrBlank()))
            {
                parameters++;
                var format = parameters < Parameters.Count ? "{0}=\"{1}\"," : "{0}=\"{1}\"";
                sb.Append(format.FormatWith(parameter.Name, parameter.Value));
            }

            var authorization = sb.ToString();
            return authorization;
        }
       
#if !SILVERLIGHT
        public override string Request(string url, IEnumerable<HttpPostParameter> parameters, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url);
            return base.Request(url, parameters, out exception);
        }

        public override string Request(string url, out WebException exception)
        {
            RecalculateProtectedResourceSignature(url); 
            return base.Request(url, out exception);
        }
#endif

        private void RecalculateProtectedResourceSignature(string url)
        {
            var info = (OAuthWebQueryInfo) Info;
            if (info.Token.IsNullOrBlank() || info.TokenSecret.IsNullOrBlank())
            {
                // No signature values to work with
                return;
            }

            if(!info.ClientUsername.IsNullOrBlank() || !info.ClientPassword.IsNullOrBlank())
            {
                // Not a protected resource request
                return;
            }

            var oauth = new OAuthWorkflow
                            {
                                ConsumerKey = info.ConsumerKey,
                                ConsumerSecret = info.ConsumerSecret,
                                Token = info.Token,
                                TokenSecret = info.TokenSecret,
                                ClientUsername = info.ClientUsername,
                                ClientPassword = info.ClientPassword,
                                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                                CallbackUrl = info.Callback,
                                Verifier = info.Verifier
                            };

            var parameters = new WebParameterCollection();
            Info = oauth.BuildProtectedResourceInfo(Method, parameters, url);
            Parameters = ParseInfoParameters();
        }
    }
}