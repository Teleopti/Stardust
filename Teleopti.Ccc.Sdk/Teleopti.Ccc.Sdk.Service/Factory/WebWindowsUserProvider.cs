using System.IdentityModel.Tokens;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class WebWindowsUserProvider : IWindowsUserProvider
    {
        private string _domainName;
        private string _userName;
        private bool _initialized;

        public string DomainName
        {
            get
            {
                VerifyInitialized();
                return _domainName;
            }
        }

        public string UserName
        {
            get
            {
                VerifyInitialized();
                return _userName;
            }
        }

        private void VerifyInitialized()
        {
            if (!_initialized)
            {
                var operationContext = OperationContext.Current;
                string windowsName = string.Empty;

                var transportToken = operationContext.IncomingMessageProperties.Security.TransportToken;

                if (transportToken != null)
                {
                    var windowsToken = transportToken.SecurityToken as WindowsSecurityToken;
                    if (windowsToken != null)
                    {
                        var identity = windowsToken.WindowsIdentity;
                        if (identity != null &&
                            identity.IsAuthenticated)
                        {
                            windowsName = identity.Name;
                        }
                    }
                }

                string[] windowsNameArray = windowsName.Split('\\');
                _domainName = ".";
                _userName = ".";

                if (windowsNameArray.Length > 1)
                {
                    _domainName = windowsNameArray[0];
                    _userName = windowsNameArray[1];
                }
                _initialized = true;
            }
        }
    }
}
