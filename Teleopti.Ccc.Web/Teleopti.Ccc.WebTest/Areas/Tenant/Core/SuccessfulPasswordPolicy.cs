using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class SuccessfulPasswordPolicy : IPasswordPolicyCheck
	{
		public ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser)
		{
			return null;
		}
	}
}