using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IMultiTenancyApplicationLogon
	{
		//AuthenticationResult Logon(ILogonModel logonModel, string userAgent);
		AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData, string userAgent);
	}
}