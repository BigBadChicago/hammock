using System;
using System.Net;

namespace Hammock.Web.Mocks
{
    public class MockWebRequestFactory : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
