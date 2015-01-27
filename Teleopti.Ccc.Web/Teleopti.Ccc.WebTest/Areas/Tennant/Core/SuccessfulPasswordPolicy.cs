using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class SuccessfulPasswordPolicy : IPasswordPolicyCheck
	{
		public ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser)
		{
			return null;
		}
	}
}