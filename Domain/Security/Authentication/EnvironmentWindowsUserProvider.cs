using System;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
    public class EnvironmentWindowsUserProvider : IWindowsUserProvider
    {
        public string DomainName
        {
            get { return Environment.UserDomainName; }
        }

        public string UserName
        {
            get { return Environment.UserName; }
        }
    }
}