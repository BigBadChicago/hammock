using System;
using DotNetMerchant.Extensions;
using Hammock.Specifications;

namespace RestCore.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class SpecificationAttribute : Attribute
    {
        public SpecificationAttribute(Type specificationType)
        {
            if (!specificationType.Implements(typeof (ISpecification)))
            {
                throw new ArgumentException(
                    "You must provide a valid specification type.",
                    "specificationType");
            }

            SpecificationType = specificationType as ISpecification;
        }

        public ISpecification SpecificationType { get; private set; }
    }
}