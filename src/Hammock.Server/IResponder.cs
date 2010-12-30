using System.IO;

namespace Hammock.Server
{
    public interface IResponder
    {
        void Respond(Stream stream, long offset, long length);
    }
}