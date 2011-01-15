using System.Net;

namespace Hammock.Server.Defaults
{
    public class Address : IAddress
    {
        private readonly IPAddress _address;

        public Address(IPAddress address)
        {
            _address = address;
        }

        private static readonly IAddress _loopback = new Address(IPAddress.Loopback);

        public static IAddress Loopback
        {
            get { return _loopback; }
        }

        public byte[] Value
        {
            get { return _address.GetAddressBytes(); }
        }
    }
}
