namespace Hammock.Web
{
    /// <summary>
    /// Enumeration of various return formats used for web requests
    /// </summary>
    public enum WebFormat
    {
        /// <summary>
        /// Request the results in XML format (.xml)
        /// </summary>
        Xml,
        /// <summary>
        /// Request the reasults in JavaScript Object Notation (JSON) format (.json)
        /// </summary>
        Json,
        /// <summary>
        /// Request the results in Really Simply Syndication (RSS) format (.rss)
        /// </summary>
        Rss,
        /// <summary>
        /// Request the results in the Atom syndication format (.atom)
        /// </summary>
        Atom,
        /// <summary>
        /// Request the results without a format specifier. Used for endpoints of the format (http://example.com/api/EndPoint/)
        /// </summary>
        None
    }
}