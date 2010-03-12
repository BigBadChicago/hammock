using System;
using System.Net;

namespace Hammock.Web
{
    public class WebQueryResponseEventArgs : EventArgs
    {
        public WebQueryResponseEventArgs(string response)
        {
            Response = response;
        }

        public WebQueryResponseEventArgs(string response, WebException exception)
        {
            Response = response;
            Exception = exception;
        }

        public string Response { get; set; }
        public WebException Exception { get; set; }
    }
}