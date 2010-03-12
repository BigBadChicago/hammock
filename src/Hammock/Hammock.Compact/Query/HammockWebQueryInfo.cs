using Hammock.Web.Attributes;
using Hammock.Web.Query;

namespace Hammock.Query
{
    public class HammockQueryInfo : IWebQueryInfo
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        [UserAgent]
        public string UserAgent { get; set; }
    }
}
