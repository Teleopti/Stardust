using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IMultiTenancyWindowsLogon
	{
		AuthenticationResult Logon(string userAgent);
		bool CheckWindowsIsPossible();
	}
}