namespace Hammock.Server.Defaults
{
    public class HttpConnection : IHttpConnection
    {
        public virtual IEndpoint Endpoint { get; set; }
    }
}
