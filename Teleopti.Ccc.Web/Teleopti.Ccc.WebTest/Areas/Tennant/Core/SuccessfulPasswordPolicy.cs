using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class SuccessfulPasswordPolicy : IPasswordPolicyCheck
	{
		public bool Verify(PasswordPolicyForUser passwordPolicyForUser, out string passwordPolicyFailureReason, out bool passwordExpired)
		{
			passwordPolicyFailureReason = null;
			passwordExpired = false;
			return true;
		}
	}
}