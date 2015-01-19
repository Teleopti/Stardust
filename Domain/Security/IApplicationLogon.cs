using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Domain.Security
{
	public interface IApplicationLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel);
	}
}