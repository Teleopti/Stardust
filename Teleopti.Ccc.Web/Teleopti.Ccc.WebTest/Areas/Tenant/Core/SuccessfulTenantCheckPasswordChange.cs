using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class SuccessfulTenantCheckPasswordChange : ITenantCheckPasswordChange
	{
		public AuthenticationResult Check(ApplicationLogonInfo userDetail)
		{
			return new AuthenticationResult{Successful = true};
		}
	}
}