using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IPasswordPolicyCheck
	{
		ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser);
	}
}