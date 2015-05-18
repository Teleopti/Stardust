using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IMultiTenancyApplicationLogon
	{
		AuthenticationResult Logon(string userName, string password, string userAgent);
	}
}