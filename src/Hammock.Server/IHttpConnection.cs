namespace Hammock.Server
{
    public interface IHttpConnection
    {
        IEndpoint Endpoint { get; }
    }
}