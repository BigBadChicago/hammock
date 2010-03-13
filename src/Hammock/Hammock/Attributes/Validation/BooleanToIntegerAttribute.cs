using System;
using System.Reflection;

namespace Hammock.Attributes.Validation
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class BooleanToIntegerAttribute : ValidationAttribute
    {
        public override string TransformValue(PropertyInfo property, object value)
        {
            bool result;
            return bool.TryParse(value.ToString(), out result)
                       ? result ? "1" : "0"
                       : base.TransformValue(property, value);
        }
    }
}