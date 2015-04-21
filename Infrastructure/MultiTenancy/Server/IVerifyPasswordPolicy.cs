using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IVerifyPasswordPolicy
	{
		AuthenticationResult Check(ApplicationLogonInfo userDetail);
	}
}