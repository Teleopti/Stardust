using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IMultiTenancyApplicationLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel, string userAgent);
	}
}