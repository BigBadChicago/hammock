using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hammock.Authentication;
using Hammock.Caching;
using Hammock.Model;
using Hammock.Retries;
using Hammock.Serialization;
using Hammock.Tasks;
using Hammock.Web;
using Hammock.Streaming;

#if SILVERLIGHT
using Hammock.Silverlight.Compat;
#else
using System.Collections.Specialized;
#endif

namespace Hammock
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class RestBase : PropertyChangedBase
    {
        private byte[] _postContent;
        private TaskOptions _taskOptions;
        private RetryPolicy _retryPolicy;

        public WebParameterCollection GetAllHeaders()
        {
            var headers = new WebParameterCollection();
            
            var parameters = Headers.AllKeys.Select(key => new WebPair(key, Headers[key]));
            foreach (var parameter in parameters)
            {
                headers.Add(parameter.Name, parameter.Value);
            }

            return headers;
        }

        protected virtual internal NameValueCollection Headers { get; set; }
        protected virtual internal Encoding Encoding { get; set; }
        protected virtual internal WebParameterCollection Parameters { get; set; }
        protected virtual internal ICollection<HttpPostParameter> PostParameters { get; set; }
        protected virtual internal byte[] PostContent
        {
            get
            {
                return _postContent;
            }
            set
            {
                _postContent = value;
                if (value != null && (Method != WebMethod.Post && Method != WebMethod.Put))
                {
                    Method = WebMethod.Post;
                }
            }
        }

        public virtual string UserAgent { get; set; }
        public virtual WebMethod? Method { get; set; }
        public virtual IWebCredentials Credentials { get; set; }
        
        protected RestBase()
        {
            Initialize();
        }

        private void Initialize()
        {
            Headers = new NameValueCollection(0);
            Parameters = new WebParameterCollection();
            PostParameters = new List<HttpPostParameter>(0);
        }

#if !Silverlight
        public virtual ServicePoint ServicePoint { get; set; }
        public virtual bool? FollowRedirects { get; set; }
#endif
        public virtual string Proxy { get; set; }
        public virtual TimeSpan? Timeout { get; set; }
        public virtual string VersionPath { get; set; }
        public virtual ISerializer Serializer { get; set; }
        public virtual IDeserializer Deserializer { get; set; }
        public virtual ICache Cache { get; set; }
        public virtual CacheOptions CacheOptions { get; set; }
        public virtual RetryPolicy RetryPolicy
        {
            get { return _retryPolicy; }
            set
            {
                if (_retryPolicy != value)
                {
                    _retryPolicy = value;
                    RetryState = new TaskState();
                }

            }
        }
        public virtual TaskOptions TaskOptions
        {
            get { return _taskOptions; }
            set
            {
                if (_taskOptions != value)
                {
                    _taskOptions = value;
                    TaskState = new TaskState();
                }
            }
        }
        public virtual bool IsFirstIteration
        {
            get
            {
                if (RetryState != null)
                {
                    return RetryState.RepeatCount == 0;
                }
                if (TaskState != null)
                {
                    return TaskState.RepeatCount == 0;
                }
                return true; 
            }
        }
        
        public virtual ITaskState TaskState { get; set; }
        public virtual ITaskState RetryState { get; set; }
        public virtual StreamOptions StreamOptions { get; set; }
        public virtual Func<string> CacheKeyFunction { get; set; }
        public virtual DecompressionMethods DecompressionMethods { get; set; }
        public virtual IWebQueryInfo Info { get; set; }
        public virtual string Path { get; set; }
        public virtual object Tag { get; set; }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        public void AddParameter(string name, string value)
        {
            Parameters.Add(name, value);
        }

        public void AddField(string name, string value)
        {
            var field = new HttpPostParameter(name, value);
            PostParameters.Add(field);
        }

        public void AddFile(string name, string fileName, string filePath)
        {
            var parameter = HttpPostParameter.CreateFile(name, fileName, filePath, "application/octet-stream");
            PostParameters.Add(parameter);
        }

        public void AddFile(string name, string fileName, string filePath, string contentType)
        {
            var parameter = HttpPostParameter.CreateFile(name, fileName, filePath, contentType);
            PostParameters.Add(parameter);
        }

        public void AddPostContent(byte[] content)
        {
            if (PostContent == null)
            {
                PostContent = content;
            }
            else
            {
                var original = PostContent.Length;
                var current = content.Length;

                var final = new byte[current + original];
                Array.Copy(PostContent, 0, final, 0, original);
                Array.Copy(content, 0, final, original, current);

                PostContent = final;
            }
        }
    }
}