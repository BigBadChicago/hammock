namespace Hammock.Web
{
    ///<summary>
    /// A <see cref="WebParameter" /> that maps to HTTP POST
    /// parameters in an HTTP body.
    ///</summary>
    public class HttpPostParameter : WebParameter
    {
        public HttpPostParameter(string name, string value) : base(name, value)
        {

        }

        ///<summary>
        /// The HTTP POST parameter type.
        ///</summary>
        public HttpPostParameterType Type { get; private set; }

        /// <summary>
        /// The physical file name.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The path to the physical file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// The content type of the file.
        /// </summary>
        public string ContentType { get; private set; }

        ///<summary>
        /// Creates a new HTTP POST parameter representing
        /// a file to transfer as multi-part form data.
        ///</summary>
        ///<param name="name">The logical name of the file</param>
        ///<param name="fileName">The physical file name</param>
        ///<param name="filePath">The path to the file</param>
        ///<param name="contentType">The file's content type</param>
        ///<returns>The created HTTP POST parameter</returns>
        public static HttpPostParameter CreateFile(string name, string fileName, string filePath, string contentType)
        {
            var parameter = new HttpPostParameter(name, string.Empty)
                                {
                                    Type = HttpPostParameterType.File,
                                    FileName = fileName,
                                    FilePath = filePath,
                                    ContentType = contentType,
                                };

            return parameter;
        }
    }
}