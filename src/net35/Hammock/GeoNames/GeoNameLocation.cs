using Newtonsoft.Json;

namespace GeoNames
{
    /*
         {"postalcodes": [{
          "postalcode": "41230",
          "adminCode1": "KY",
          "countryCode": "US",
          "lng": -82.605591,
          "placeName": "Louisa",
          "lat": 38.104327,
          "adminName1": "Kentucky"
         }]}
    */

    [JsonObject]
    public class GeoNameLocation
    {
        [JsonProperty("placeName")]
        public string City { get; set; }

        [JsonProperty("adminName1")]
        public string State { get; set; }

        [JsonProperty("adminCode1")]
        public string StateCode { get; set; }

        [JsonProperty("postalcode")]
        public string PostalCode { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }

        [JsonProperty("lng")]
        public double Longitude { get; set; }

        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Region { get; set; }
    }
}