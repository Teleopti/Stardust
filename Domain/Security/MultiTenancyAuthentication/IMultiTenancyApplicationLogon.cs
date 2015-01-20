using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface IMultiTenancyApplicationLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel);
	}
}