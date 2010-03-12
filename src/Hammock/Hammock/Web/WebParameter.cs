#if !Smartphone
using System.Diagnostics;
#endif

namespace Hammock.Web
{
#if !Smartphone
    ///<summary>
    /// A name value pair used in web requests.
    ///</summary>
    [DebuggerDisplay("{Name}:{Value}")]
#endif
    public class WebParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameter"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public WebParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        ///<summary>
        /// The parameter value.
        ///</summary>
        public string Value { get; set; }

        /// <summary>
        /// The parameter name.
        /// </summary>
        public string Name { get; private set; }
    }
}