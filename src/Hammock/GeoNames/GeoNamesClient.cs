using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Hammock;
using Hammock.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoNames
{
    public class GeoNamesClient
    {
        private readonly RestClient _client;
        private static readonly Regex _region = new Regex(@"\s(\((.*)\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public GeoNamesClient()
        {
            _client = new RestClient
                          {
                              Authority = "http://ws.geonames.org/postalCodeLookupJSON"
                          };
        }

        // http://ws.geonames.org/postalCodeLookupJSON?formatted=true&postalcode=6600&country=AT&style=full
        public IEnumerable<GeoNameLocation> GetLocations(string postalCode, string countryCode)
        {
            try
            {
                var info = new RegionInfo(countryCode);
                countryCode = countryCode.ToUpperInvariant();

                var request = new RestRequest();
                request.AddHeader("User-Agent", "GeoNames.NET");
                request.AddParameter("formatted", "false");
                request.AddParameter("style", "full");
                request.AddParameter("country", countryCode);
                request.AddParameter("postalcode", postalCode.ToUpperInvariant());

                var locations = GetResponse(request).ToList();

                foreach (var location in locations)
                {
                    location.CountryCode = countryCode;
                    location.Country = info.EnglishName;

                    var match = _region.Match(location.City);
                    if (match.Groups.Count != 3)
                    {
                        continue;
                    }

                    location.City = location.City.Replace(match.Groups[0].Value, "");
                    location.Region = match.Groups[2].Value;
                }
                return locations;
            }
            catch (ArgumentException)
            {
                throw new ValidationException("You must provide a valid ISO-3166 two-letter country code.");
            }
        }

        private IEnumerable<GeoNameLocation> GetResponse(RestRequest request)
        {
            var response = _client.Request(request);
            var property = (JProperty)((JContainer)JsonConvert.DeserializeObject(response.Content)).First();
            var postalcodes = (JArray)property.Value;

            return postalcodes.Select(postalcode =>
                                      JsonConvert.DeserializeObject<GeoNameLocation>(postalcode.ToString())).Where(
                                          location => location != null);
        }
    }
}


