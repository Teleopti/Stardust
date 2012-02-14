using System;
using System.Globalization;
using System.Net;

namespace Teleopti.Messaging.Core
{
    public static class SocketUtility
    {
        public static bool IsMulticastAddress(string ip)
        {
            int octet = Int32.Parse(ip.Split(new[] { '.' }, 4)[0], CultureInfo.InvariantCulture);
            return octet >= 224 && octet <= 239;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip")]
        public static IPAddress IsIpAddress(IPHostEntry hostEntry)
        {
            foreach (IPAddress address in hostEntry.AddressList)
            {
                string addressString = address.ToString();
                if (IsIpAddress(addressString))
                    return address;
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip")]
        public static IPAddress IsIpAddress(IPAddress address)
        {
            string addressString = address.ToString();
            if (IsIpAddress(addressString))
                return address;
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ip")]
        public static bool IsIpAddress(string address)
        {
            var firstPart = address.Split('.')[0];
            int result;
            return (int.TryParse(firstPart, out result));
        }

        public static String GetIPAddressByHostName(string hostName)
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress address in hostAddresses)
            {
                if (IsIpAddress(address) != null)
                    return address.ToString();
            }
            return string.Empty;
        }
    }
}