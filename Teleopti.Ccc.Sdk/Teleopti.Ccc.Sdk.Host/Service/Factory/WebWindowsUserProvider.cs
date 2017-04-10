using System.IdentityModel.Tokens;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class WebWindowsUserProvider : IWindowsUserProvider
    {
        private string _domainName;
        private string _userName;
        private bool _initialized;

        private void VerifyInitialized()
        {
            if (!_initialized)
            {
                var operationContext = OperationContext.Current;
                string windowsName = string.Empty;

                var transportToken = operationContext.IncomingMessageProperties.Security.TransportToken;

	            var windowsToken = transportToken?.SecurityToken as WindowsSecurityToken;
	            var identity = windowsToken?.WindowsIdentity;
	            if (identity != null &&
					identity.IsAuthenticated)
	            {
		            windowsName = identity.Name;
	            }

	            var windowsNameArray = IdentityHelper.Split(windowsName);
                _domainName = ".";
                _userName = ".";

                if (!string.IsNullOrEmpty(windowsNameArray.Item1))
                {
                    _domainName = windowsNameArray.Item1;
                    _userName = windowsNameArray.Item2;
                }
                _initialized = true;
            }
        }

	    public string Identity()
	    {
				VerifyInitialized();
		    return _domainName + "\\" + _userName;
	    }
    }
}
