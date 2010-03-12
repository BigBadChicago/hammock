namespace Hammock.Web
{
    /// <summary>
    /// The list of possible HTTP POST parameters sent with requests.
    /// </summary>
    public enum HttpPostParameterType
    {
        /// <summary>
        /// A POST field.
        /// </summary>
        Field,
        /// <summary>
        /// A POST file, sent as multi-part.
        /// </summary>
        File
    }
}