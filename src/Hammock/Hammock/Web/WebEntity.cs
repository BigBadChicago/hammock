using System.Text;

namespace Hammock.Web
{
    public class WebEntity
    {
        public object Content { get; set; }
        public string ContentType { get; set; }
        public Encoding ContentEncoding { get; set; }
    }
}