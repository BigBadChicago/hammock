using System;
using System.Text;
using Hammock.Extensions;
using Hammock.Web;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System.Collections.Specialized;
using System.Diagnostics;
#endif

namespace Hammock
{
#if !Silverlight
    [Serializable]
#endif
    public class RestRequest  : RestBase
    {
        private object _entity;
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

        public virtual string Path { get; set; }
        public virtual Type ResponseEntityType { get; set; }
        public virtual Type RequestEntityType { get; set; }

        public RestRequest()
        {
            Headers = new NameValueCollection(0);
            Parameters = new WebParameterCollection();
        }

        protected internal Uri BuildEndpoint(RestClient client)
        {
            var sb = new StringBuilder();
            var versionPath = client.VersionPath.IsNullOrBlank()
                                  ? VersionPath.IsNullOrBlank() ? "" : VersionPath
                                  : client.VersionPath;

            sb.Append(client.Authority.IsNullOrBlank() ? "" : client.Authority);
            sb.Append(client.Authority.EndsWith("/") ? "" : "/");
            sb.Append(versionPath.IsNullOrBlank() ? "" : versionPath);
            if(!versionPath.IsNullOrBlank())
            {
                sb.Append(versionPath.EndsWith("/") ? "" : "/");
            }
            sb.Append(Path.IsNullOrBlank() ? "" : Path.StartsWith("/") ? Path.Substring(1) : Path);

            Uri uri;
            Uri.TryCreate(sb.ToString(), UriKind.RelativeOrAbsolute, out uri);

            return uri;
        }
    }
}


