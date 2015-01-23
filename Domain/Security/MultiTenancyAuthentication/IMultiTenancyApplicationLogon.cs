using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IMultiTenancyApplicationLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData);
	}
}