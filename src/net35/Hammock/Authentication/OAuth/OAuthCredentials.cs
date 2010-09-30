using System;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock.Authentication.OAuth
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class OAuthCredentials : IWebCredentials
    {
        public virtual string ConsumerKey { get; set; }
        public virtual string ConsumerSecret { get; set; }
        public virtual OAuthParameterHandling ParameterHandling { get; set; }
        public virtual OAuthSignatureMethod SignatureMethod { get; set; }
        public virtual OAuthSignatureTreatment SignatureTreatment { get; set; }
        public virtual OAuthType Type { get; set; }

        public virtual string Token { get; set; }
        public virtual string TokenSecret { get; set; }
        public virtual string Verifier { get; set; }
        public virtual string ClientUsername { get; set; }
        public virtual string ClientPassword { get; set; }
        public virtual string CallbackUrl { get; set; }
        public virtual string Version { get; set; }

        public OAuthCredentials()
        {
            
        }

        /// <summary>
        /// A copy constructor for creating a credentials instance 
        /// </summary>
        /// <param name="workflow"></param>
        public virtual WebQuery GetQueryFor(string url, 
                                            WebParameterCollection parameters, 
                                            IWebQueryInfo info, 
                                            WebMethod method)
        {
            OAuthWebQueryInfo oauth;

            var workflow = new OAuthWorkflow
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                ParameterHandling = ParameterHandling,
                SignatureMethod = SignatureMethod,
                SignatureTreatment = SignatureTreatment,
                CallbackUrl = CallbackUrl,
                ClientPassword = ClientPassword,
                ClientUsername = ClientUsername,
                Verifier = Verifier,
                Token = Token,
                TokenSecret = TokenSecret,
                Version = Version ?? "1.0"
            };

            switch (Type)
            {
                case OAuthType.RequestToken:
                    workflow.RequestTokenUrl = url;
                    oauth = workflow.BuildRequestTokenInfo(method, parameters);
                    break;
                case OAuthType.AccessToken:
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ClientAuthentication:
                    method = WebMethod.Post;
                    workflow.AccessTokenUrl = url;
                    oauth = workflow.BuildClientAuthAccessTokenInfo(method, parameters);
                    break;
                case OAuthType.ProtectedResource:
                    oauth = workflow.BuildProtectedResourceInfo(method, parameters, url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OAuthWebQuery(oauth);
        }

        public virtual WebQuery GetQueryFor(string url, RestBase request, IWebQueryInfo info, WebMethod method)
        {
            var query = GetQueryFor(url, request.Parameters, info, method);
            request.Method = method;
            return query;
        }
    }
}


