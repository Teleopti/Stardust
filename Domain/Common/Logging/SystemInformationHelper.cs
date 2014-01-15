using System.Net;

namespace Teleopti.Ccc.Domain.Common.Logging
{
    public static class SystemInformationHelper
    {
        public static string GetSystemIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public static string GetCurrentUserName()
        {
            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            if (windowsIdentity != null)
                return windowsIdentity.Name;
            return "";
        }
    }
    
}
