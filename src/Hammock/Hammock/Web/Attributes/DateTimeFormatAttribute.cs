using System;

namespace Hammock.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class DateTimeFormatAttribute : Attribute
    {
        public DateTimeFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; private set; }
    }
}