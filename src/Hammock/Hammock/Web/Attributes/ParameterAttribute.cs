using System;

namespace Hammock.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class ParameterAttribute : Attribute, INamedAttribute
    {
        public ParameterAttribute(string name)
        {
            Name = name;
        }

        #region INamedAttribute Members

        public string Name { get; private set; }

        #endregion
    }
}