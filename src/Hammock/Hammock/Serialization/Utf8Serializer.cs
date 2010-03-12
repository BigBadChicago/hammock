using System.Text;

namespace Hammock.Serialization
{
    public class Utf8Serializer
    {
        public virtual Encoding ContentEncoding
        {
            get { return Encoding.UTF8; }
        }
    }
}