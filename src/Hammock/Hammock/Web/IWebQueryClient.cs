using System;
using System.IO;
using System.Net;
using System.Security.Permissions;

namespace Hammock.Web
{
    public interface IWebQueryClient
    {
        WebResponse Response { get; }
        WebRequest Request { get; }
        WebCredentials WebCredentials { get; set; }
        bool UseCompression { get; set; }
        TimeSpan? RequestTimeout { get; set; }
        string ProxyValue { get; set; }
        bool KeepAlive { get; set; }
        WebException Exception { get; set; }
        string SourceUrl { get; set; }

#if SILVERLIGHT4
        bool IsOutOfBrowser { get; }
#endif
        WebRequest GetWebRequestShim(Uri address);
        WebResponse GetWebResponseShim(WebRequest request, IAsyncResult result);
#if !SILVERLIGHT
#if !Smartphone
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
#endif
#endif
        void OpenReadAsync(Uri uri);
#if !SILVERLIGHT
#if !Smartphone
        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true )]
#endif
#endif
        void OpenReadAsync(Uri uri, object state);

#if !SILVERLIGHT
        void SetWebProxy(WebRequest request);
        event OpenReadCompletedEventHandler OpenReadCompleted;
        WebResponse GetWebResponseShim(WebRequest request);
        Stream OpenRead(string url);
#else
        event EventHandler<OpenReadCompletedEventArgs> OpenReadCompleted;
#endif
        void CancelAsync();
    }
}