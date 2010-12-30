using System.IO;

namespace Hammock.Server.Responders
{
    public class FileResponder : IResponder
    {
        public void Respond(string filename, long offset, long length)
        {
            if (length == 0)
            {
                return;
            }

            using(var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Respond(stream, offset, length);
            }
        }

        public void Respond(Stream stream, long offset, long length)
        {
            
        }
    }
}