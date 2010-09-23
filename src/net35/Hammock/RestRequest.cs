using System;
using System.Net;
using System.Text;
using Hammock.Extensions;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestRequest : RestBase
    {
        private object _entity;
        private object _expectEntity;

        protected internal virtual Web.WebHeaderCollection ExpectHeaders { get; set; }
        public virtual HttpStatusCode? ExpectStatusCode { get; set; }
        public virtual string ExpectStatusDescription { get; set; }
        public virtual string ExpectContent { get; set; }
        public virtual string ExpectContentType { get; set; }
        
        public RestRequest()
        {
            Initialize();
        }

        private void Initialize()
        {
            ExpectHeaders = new Web.WebHeaderCollection();
        }

        public virtual object Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                if (_entity != null && _entity.Equals(value))
                {
                    return;
                }

                _entity = value;
                OnPropertyChanged("Entity");

                // [DC] Automatically posts an entity unless put is declared
                RequestEntityType = _entity.GetType();
                if (_entity != null && (Method != WebMethod.Post && Method != WebMethod.Put))
                {
                    Method = WebMethod.Post;
                }
            }
        }

        public virtual object ExpectEntity
        {
            get
            {
                return _expectEntity;
            }
            set
            {
                if (_expectEntity != null && _expectEntity.Equals(value))
                {
                    return;
                }

                _expectEntity = value;
                OnPropertyChanged("ExpectEntity");
            }
        }

        public virtual Type ResponseEntityType { get; set; }
        public virtual Type RequestEntityType { get; set; }

        public Uri BuildEndpoint(RestClient client)
        {
            var sb = new StringBuilder();

            var path = Path.IsNullOrBlank()
                           ? client.Path.IsNullOrBlank() ? "" : client.Path
                           : Path;

            var versionPath = VersionPath.IsNullOrBlank()
                                  ? client.VersionPath.IsNullOrBlank() ? "" : client.VersionPath
                                  : VersionPath;
            var skipAuthority = client.Authority.IsNullOrBlank();

            sb.Append(skipAuthority ? "" : client.Authority);
            sb.Append(skipAuthority ? "" : client.Authority.EndsWith("/") ? "" : "/");
            sb.Append(skipAuthority ? "" : versionPath.IsNullOrBlank() ? "" : versionPath);
            if (!skipAuthority && !versionPath.IsNullOrBlank())
            {
                sb.Append(versionPath.EndsWith("/") ? "" : "/");
            }
            sb.Append(path.IsNullOrBlank() ? "" : path.StartsWith("/") ? path.Substring(1) : path);

            Uri uri;
            Uri.TryCreate(sb.ToString(), UriKind.RelativeOrAbsolute, out uri);

            // [DC]: If the path came in with parameters attached, we should scrub those
            WebParameterCollection parameters;
            uri = uri.UriMinusQuery(out parameters);
            foreach (var parameter in parameters)
            {
                Parameters.Add(parameter);
            }

            return uri;
        }

        public void ExpectHeader(string name, string value)
        {
            ExpectHeaders.Add(name, value);
        }
    }
}


