using System.Linq;
using NUnit.Framework;

namespace GeoNames.Tests
{
    [TestFixture]
    public class GeoNamesTests
    {
        [Test]
        public void Can_get_location()
        {
            var client = new GeoNamesClient();
            var locations = client.GetLocations("41230", "US");

            Assert.IsNotNull(locations);
            Assert.IsTrue(locations.Count() == 1);
            Assert.AreEqual("Louisa", locations.First().City);
        }

        [Test]
        public void Can_get_location_with_region()
        {
            var client = new GeoNamesClient();
            var locations = client.GetLocations("K1H", "CA");

            Assert.IsNotNull(locations);
            Assert.IsTrue(locations.Count() == 1);
            Assert.AreEqual("Ottawa", locations.First().City);
            Assert.AreEqual("Alta Vista", locations.First().Region);
        }

        [Test]
        public void Can_get_locations()
        {
            var client = new GeoNamesClient();
            var locations = client.GetLocations("N1H", "CA");

            Assert.IsNotNull(locations);
            Assert.IsTrue(locations.Count() > 1);
        }
    }
}