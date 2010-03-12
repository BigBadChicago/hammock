using System;

namespace Hammock.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class UserAgentAttribute : Attribute
    {

    }
}