using System;
using System.Text;

namespace Hammock.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class WebEntityAttribute : Attribute
    {
        public WebEntityAttribute()
        {
            ContentType = "text/xml";
            ContentEncoding = Encoding.UTF8;
        }

        public string ContentType { get; private set; }
        public Encoding ContentEncoding { get; set; }
    }
}