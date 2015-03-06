using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPasswordPolicyCheck
	{
		ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser);
	}
}