using System;
using System.Text;

namespace Hammock.Attributes.Specialized
{
#if !SILVERLIGHT
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute()
        {
            ContentType = "text/xml";
            ContentEncoding = Encoding.UTF8;
        }

        public string ContentType { get; private set; }
        public Encoding ContentEncoding { get; set; }
    }
}