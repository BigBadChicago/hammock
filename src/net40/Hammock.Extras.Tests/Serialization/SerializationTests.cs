using Hammock.Extras.Serialization;
using NUnit.Framework;

namespace Hammock.Extras.Tests.Serialization
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Can_deserialize_to_dynamic_collection()
        {
            var serializer = new JsonSerializer();
            var response = new RestResponse<JsonObject>();
            response.SetContent(DoubleInput);
            var proxy = serializer.DeserializeDynamic(response);
            Assert.IsNotNull(proxy);
        }

        [Test]
        public void Can_deserialize_to_dynamic_single()
        {
            var serializer = new JsonSerializer();
            var response = new RestResponse<JsonObject>();
            response.SetContent(SingleInput);
            var proxy = serializer.DeserializeDynamic(response);
            Assert.IsNotNull(proxy);
        }

        private const string DoubleInput = "[" + SingleInput + "," + SingleInput + "]";

        private const string SingleInput =
                @"{
  ""contributors"": null,
  ""retweeted"": false,
  ""in_reply_to_user_id_str"": null,
  ""retweet_count"": 0,
  ""geo"": null,
  ""id_str"": ""40678260320768000"",
  ""in_reply_to_status_id"": null,
  ""created_at"": ""Thu Feb 24 07:43:47 +0000 2011"",
  ""place"": null,
  ""coordinates"": null,
  ""truncated"": false,
  ""favorited"": false,
  ""user"": {
    ""screen_name"": ""marutisuzukisx4"",
    ""verified"": false,
    ""friends_count"": 45,
    ""follow_request_sent"": null,
    ""time_zone"": ""Chennai"",
    ""profile_text_color"": ""333333"",
    ""location"": ""India"",
    ""notifications"": null,
    ""profile_sidebar_fill_color"": ""efefef"",
    ""id_str"": ""196143889"",
    ""contributors_enabled"": false,
    ""lang"": ""en"",
    ""profile_background_tile"": false,
    ""created_at"": ""Tue Sep 28 12:55:15 +0000 2010"",
    ""followers_count"": 117,
    ""show_all_inline_media"": true,
    ""listed_count"": 1,
    ""geo_enabled"": true,
    ""profile_link_color"": ""009999"",
    ""profile_sidebar_border_color"": ""eeeeee"",
    ""protected"": false,
    ""name"": ""Maruti Suzuki SX4"",
    ""statuses_count"": 637,
    ""following"": null,
    ""profile_use_background_image"": true,
    ""profile_image_url"": ""http://a3.twimg.com/profile_images/1170694644/Slide1_normal.JPG"",
    ""id"": 196143889,
    ""is_translator"": false,
    ""utc_offset"": 19800,
    ""favourites_count"": 0,
    ""profile_background_color"": ""131516""
  },
  ""in_reply_to_screen_name"": null,
  ""id"": 40678260320768000,
  ""in_reply_to_status_id_str"": null,
  ""in_reply_to_user_id"": null
}";
    }
}
