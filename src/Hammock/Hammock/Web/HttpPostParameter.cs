namespace Hammock.Web
{
    public class HttpPostParameter : WebParameter
    {
        public HttpPostParameter(string name, string value) : base(name, value)
        {

        }

        public HttpPostParameterType Type { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string ContentType { get; private set; }
       
        public static HttpPostParameter CreateFile(string name, 
                                                   string fileName, 
                                                   string filePath, 
                                                   string contentType)
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