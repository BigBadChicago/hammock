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
    public class RestRequest  : RestBase
    {
        private object _entity;

        protected internal virtual Web.WebHeaderCollection ExpectHeaders { get; set; }
        public virtual HttpStatusCode? ExpectStatusCode { get; set; }
        public virtual string ExpectStatusDescription { get; set; }
        public virtual string ExpectContent { get; set; }
        public virtual string ExpectContentType { get; set; }
        public virtual object ExpectEntity { get; set; }

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
                if(_entity != null && (Method != WebMethod.Post && Method != WebMethod.Put))
                {
                    Method = WebMethod.Post;
                }
            }
        }

        public virtual Type ResponseEntityType { get; set; }
        public virtual Type RequestEntityType { get; set; }

        protected internal Uri BuildEndpoint(RestClient client)
        {
            var sb = new StringBuilder();

            var path = Path.IsNullOrBlank()
                           ? client.Path.IsNullOrBlank() ? "" : client.Path
                           : Path;
            var versionPath = VersionPath.IsNullOrBlank()
                                  ? client.VersionPath.IsNullOrBlank() ? "" : client.VersionPath
                                  : VersionPath;
            var hasAuthority = client.Authority.IsNullOrBlank();

            sb.Append(hasAuthority ? "" : client.Authority);
            sb.Append(hasAuthority ? "" : client.Authority.EndsWith("/") ? "" : "/");
            sb.Append(versionPath.IsNullOrBlank() ? "" : versionPath);
            if(!versionPath.IsNullOrBlank())
            {
                sb.Append(versionPath.EndsWith("/") ? "" : "/");
            }
            sb.Append(path.IsNullOrBlank() ? "" : path.StartsWith("/") ? path.Substring(1) : path);

            Uri uri;
            Uri.TryCreate(sb.ToString(), UriKind.RelativeOrAbsolute, out uri);

            return uri;
        }

        public void ExpectHeader(string name, string value)
        {
            ExpectHeaders.Add(name, value);
        }
    }
}


