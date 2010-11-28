using System;
using System.IO;
using System.Net;

namespace Hammock.Web
{
    public class WebQueryResponseEventArgs : EventArgs
    {
        public WebQueryResponseEventArgs(Stream response)
        {
            Response = response;
        }

        public WebQueryResponseEventArgs(Stream response, WebException exception)
        {
            Response = response;
            Exception = exception;
        }

        public Stream Response { get; set; }
        public WebException Exception { get; set; }
    }
}