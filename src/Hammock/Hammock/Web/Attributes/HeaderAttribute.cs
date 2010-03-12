using System;

namespace Hammock.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class HeaderAttribute : Attribute, INamedAttribute
    {
        public HeaderAttribute(string name)
        {
            Name = name;
        }

        #region INamedAttribute Members

        public string Name { get; private set; }

        #endregion
    }
}