using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPasswordPolicyCheck
	{
		ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser);
	}
}