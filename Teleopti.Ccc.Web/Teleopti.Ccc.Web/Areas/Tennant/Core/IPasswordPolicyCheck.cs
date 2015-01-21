using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IPasswordPolicyCheck
	{
		bool Verify(PasswordPolicyForUser passwordPolicyForUser, out string passwordPolicyFailureReason);
	}
}