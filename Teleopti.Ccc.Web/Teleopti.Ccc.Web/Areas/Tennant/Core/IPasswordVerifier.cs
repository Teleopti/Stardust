using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IPasswordVerifier
	{
		bool Check(string userPassword, PasswordPolicyForUser passwordPolicyForUser);
	}
}