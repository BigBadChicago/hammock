using System;

namespace Hammock.Server
{
    public interface IEndpoint : IDisposable
    {
        IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
        IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback, object state);

        int EndSend(IAsyncResult result);
        int EndReceive(IAsyncResult result);

        IAsyncResult BeginAccept(AsyncCallback callback, object state);
        IEndpoint EndAccept(IAsyncResult result);

        void Close();
        IEndpoint Accept();
    }
}