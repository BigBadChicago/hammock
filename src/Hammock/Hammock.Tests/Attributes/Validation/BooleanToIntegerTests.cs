using System;
using Hammock.Attributes.Specialized;
using Hammock.Attributes.Validation;
using Hammock.Web.Query;
using NUnit.Framework;

namespace Hammock.Tests.Attributes.Validation
{
    [TestFixture]
    public class BooleanToIntegerTests
    {
        public class BooleanToIntegerInfo : IWebQueryInfo
        {
            [BooleanToInteger]
            [Parameter("Result")]
            public bool IShouldBeANumber { get; set; }
        }

        [Test]
        public void Can_use_boolean_to_integer_validation_to_transform_parameter_value()
        {
            var info = new BooleanToIntegerInfo {IShouldBeANumber = false};

            var client = new RestClient
                             {
                                 Authority = "http://nowhere.com",
                                 Info = info
                             };

            var request = new RestRequest
                              {
                                  Path = "fast"
                              };

            var response = client.Request(request);
        }
    }

    [TestFixture]
    public class DateTimeFormatTests
    {
        public class DateTimeFormatInfo : IWebQueryInfo
        {
            [BooleanToInteger]
            [Header("Result")]
            public DateTime IAmADate { get; set; }
        }

        [Test]
        public void Can_use_date_time_validation_to_transform_header_value()
        {
            var info = new DateTimeFormatInfo {IAmADate = DateTime.Now };

            var client = new RestClient
            {
                Authority = "http://nowhere.com",
                Info = info
            };

            var request = new RestRequest
            {
                Path = "fast"
            };

            var response = client.Request(request);
        }
    }
}
